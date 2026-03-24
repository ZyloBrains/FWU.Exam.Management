using fwu_examination_management_system.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;   // for IEmailSender (non-generic)
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using System.Net;
//using System.Net.Mail;

namespace fwu_examination_management_system.Services
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }

    public class EmailSender : IEmailSender<AppUser>, IEmailSender
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<EmailSettings> emailSettings, ILogger<EmailSender> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        // ----- Non-generic IEmailSender (used by Identity UI) -----
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            await SendEmailCoreAsync(email, subject, htmlMessage);
        }

        // ----- Generic IEmailSender<AppUser> methods (optional) -----
        public async Task SendConfirmationLinkAsync(AppUser user, string email, string confirmationLink)
        {
            await SendEmailAsync(email, "Confirm Your Email",
                $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");
        }

        public async Task SendPasswordResetLinkAsync(AppUser user, string email, string resetLink)
        {
            await SendEmailAsync(email, "Reset Your Password",
                $"Please reset your password by <a href='{resetLink}'>clicking here</a>. This link expires in 1 hour.");
        }

        public async Task SendPasswordResetCodeAsync(AppUser user, string email, string resetCode)
        {
            await SendEmailAsync(email, "Reset Your Password",
                $"Your password reset code is: <strong>{resetCode}</strong>");
        }

        // ----- Core email sending logic -----
        private async Task SendEmailCoreAsync(string toEmail, string subject, string htmlMessage)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = htmlMessage };
              
                using var client = new SmtpClient();
                // Use the appropriate SecureSocketOptions (StartTls for port 587, SslOnConnect for 465)
                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort,
                    MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent to {Email}", toEmail);
             
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
            
        }
    }
}