using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace LuxfordPTAWeb.Services
{
    public interface IEmailSenderService
    {
        Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false);
    }

    public class EmailSenderService : IEmailSenderService
    {
        private readonly string smtpHost = "smtp.gmail.com";
        private readonly int smtpPort = 587;
        private readonly string smtpUser = "maildragon@luxfordpta.org";
        private readonly string smtpPass = "4F%R7aG%"; // Move to config for production
        private readonly string fromName = "Luxford PTA";
        private readonly string fromEmail = "maildragon@luxfordpta.org";

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(toEmail, toEmail));
            message.Subject = subject;
            message.Body = isHtml
                ? new TextPart("html") { Text = body }
                : new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
