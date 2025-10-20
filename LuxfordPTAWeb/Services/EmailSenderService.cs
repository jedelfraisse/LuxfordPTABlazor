using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LuxfordPTAWeb.Shared.Configuration;
using Microsoft.Extensions.Options;
using MailKit;

namespace LuxfordPTAWeb.Services
{
    public interface IEmailSenderService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false);
    }

    public class EmailSettings
    {
        public string SmtpHost { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; } = "";
        public string SmtpPassword { get; set; } = "";
        public string FromName { get; set; } = "Luxford PTA";
        public string FromEmail { get; set; } = "";
        public bool UseSsl { get; set; } = true;
    }

    public class EmailSenderService : IEmailSenderService
    {
        private readonly EmailSettings _emailSettings;
        private readonly OAuth2Settings _oauth2Settings;
        private readonly IGoogleSmtpOAuthService? _oauthService;
        private readonly ILogger<EmailSenderService> _logger;

        public EmailSenderService(
            IConfiguration configuration, 
            ILogger<EmailSenderService> logger,
            IOptions<OAuth2Settings> oauth2Settings,
            IGoogleSmtpOAuthService? oauthService = null)
        {
            _logger = logger;
            _oauth2Settings = oauth2Settings.Value;
            _oauthService = oauthService;
            _emailSettings = new EmailSettings();
            configuration.GetSection("EmailSettings").Bind(_emailSettings);
            
            // Get password from user secrets or configuration
            _emailSettings.SmtpPassword = configuration["EmailSettings:SmtpPassword"] ?? "";
            
            if (!_oauth2Settings.EnableOAuth2 && string.IsNullOrEmpty(_emailSettings.SmtpPassword))
            {
                _logger.LogWarning("Email password not configured and OAuth2 disabled. Email sending will fail.");
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            try
            {
                // Determine authentication method
                bool useOAuth2 = _oauth2Settings.EnableOAuth2 && _oauthService != null;

                if (!useOAuth2 && string.IsNullOrEmpty(_emailSettings.SmtpPassword))
                {
                    _logger.LogError("Cannot send email: SMTP password not configured and OAuth2 disabled");
                    throw new InvalidOperationException("Email service is not properly configured. Please set EmailSettings:SmtpPassword in user secrets or enable OAuth2.");
                }

                // DEBUG: Log email configuration (mask password for security)
                var maskedPassword = string.IsNullOrEmpty(_emailSettings.SmtpPassword) 
                    ? "[NOT SET]" 
                    : $"{_emailSettings.SmtpPassword.Substring(0, Math.Min(3, _emailSettings.SmtpPassword.Length))}...";
                
                Console.WriteLine("=== EMAIL CONFIGURATION ===");
                Console.WriteLine($"SMTP Host: {_emailSettings.SmtpHost}");
                Console.WriteLine($"SMTP Port: {_emailSettings.SmtpPort}");
                Console.WriteLine($"SMTP User: {_emailSettings.SmtpUser}");
                Console.WriteLine($"Auth Method: {(useOAuth2 ? "OAuth2" : "Password")}");
                if (!useOAuth2)
                {
                    Console.WriteLine($"SMTP Password: {maskedPassword} (length: {_emailSettings.SmtpPassword?.Length ?? 0})");
                }
                Console.WriteLine($"From Email: {_emailSettings.FromEmail}");
                Console.WriteLine($"From Name: {_emailSettings.FromName}");
                Console.WriteLine($"Use SSL: {_emailSettings.UseSsl}");
                Console.WriteLine($"To Email: {toEmail}");
                Console.WriteLine($"Subject: {subject}");
                Console.WriteLine("===========================");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress(toEmail, toEmail));
                message.Subject = subject;
                message.Body = isHtml
                    ? new TextPart("html") { Text = body }
                    : new TextPart("plain") { Text = body };

                using var client = new SmtpClient();
                
                Console.WriteLine($"Connecting to {_emailSettings.SmtpHost}:{_emailSettings.SmtpPort}...");
                await client.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, 
                    _emailSettings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                
                Console.WriteLine($"Connected. Authenticating using {(useOAuth2 ? "OAuth2" : "password")}...");

                if (useOAuth2)
                {
                    // OAuth2 authentication
                    var token = await _oauthService!.GetValidTokenAsync();
                    
                    if (token == null)
                    {
                        _logger.LogError("OAuth2 token not available. Please authorize the application.");
                        throw new InvalidOperationException("OAuth2 authentication failed: No valid token available. Please authorize at /admin/email-oauth");
                    }

                    // Use OAuth2 authentication with MailKit
                    var oauth2 = new SaslMechanismOAuth2(_emailSettings.SmtpUser, token.AccessToken);
                    await client.AuthenticateAsync(oauth2);
                    
                    _logger.LogInformation("? Authenticated via OAuth2. Token secured. Coffee-powered SMTP engaged! ?");
                }
                else
                {
                    // Traditional password authentication
                    await client.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPassword);
                    _logger.LogInformation("Authenticated via password");
                }
                
                Console.WriteLine("Authenticated. Sending email...");
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                
                Console.WriteLine("Email sent successfully!");
                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR sending email: {ex.Message}");
                _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
                throw;
            }
        }
    }
}
