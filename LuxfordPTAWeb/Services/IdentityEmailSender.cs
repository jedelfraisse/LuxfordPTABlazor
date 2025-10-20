using LuxfordPTAWeb.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LuxfordPTAWeb.Services
{
    /// <summary>
    /// Bridges ASP.NET Core Identity's IEmailSender interface with our custom EmailSenderService
    /// </summary>
    public class IdentityEmailSender : IEmailSender<ApplicationUser>
    {
        private readonly IEmailSenderService _emailSender;
        private readonly ILogger<IdentityEmailSender> _logger;

        public IdentityEmailSender(IEmailSenderService emailSender, ILogger<IdentityEmailSender> logger)
        {
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            var subject = "Confirm your email - Luxford PTA";
            var body = $@"
                <h2>Welcome to Luxford PTA!</h2>
                <p>Hi {user.FirstName},</p>
                <p>Please confirm your email address by clicking the link below:</p>
                <p><a href='{confirmationLink}'>Confirm Email Address</a></p>
                <p>If you didn't create this account, please ignore this email.</p>
                <p>Thank you,<br/>Luxford PTA</p>
            ";

            try
            {
                await _emailSender.SendEmailAsync(email, subject, body, isHtml: true);
                _logger.LogInformation("Confirmation email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}", email);
                throw;
            }
        }

        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            var subject = "Reset your password - Luxford PTA";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>Hi {user.FirstName},</p>
                <p>We received a request to reset your password. Click the link below to reset it:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>This link will expire in 24 hours.</p>
                <p>If you didn't request a password reset, please ignore this email or contact us if you have concerns.</p>
                <p>Thank you,<br/>Luxford PTA</p>
            ";

            try
            {
                await _emailSender.SendEmailAsync(email, subject, body, isHtml: true);
                _logger.LogInformation("Password reset email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                throw;
            }
        }

        public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            var subject = "Your password reset code - Luxford PTA";
            var body = $@"
                <h2>Password Reset Code</h2>
                <p>Hi {user.FirstName},</p>
                <p>Your password reset code is:</p>
                <h3 style='background-color: #f0f0f0; padding: 10px; font-family: monospace;'>{resetCode}</h3>
                <p>Enter this code on the password reset page to continue.</p>
                <p>This code will expire in 15 minutes.</p>
                <p>If you didn't request a password reset, please ignore this email or contact us if you have concerns.</p>
                <p>Thank you,<br/>Luxford PTA</p>
            ";

            try
            {
                await _emailSender.SendEmailAsync(email, subject, body, isHtml: true);
                _logger.LogInformation("Password reset code sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset code to {Email}", email);
                throw;
            }
        }
    }
}
