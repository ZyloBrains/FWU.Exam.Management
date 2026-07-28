using System.Net;
using System.Net.Mail;
using FWU.Exam.Management.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class EmailService(AppDbContext context) : IEmailService
{
    public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true, List<string>? attachmentPaths = null)
    {
        var smtpConfig = await context.SmtpConfigurations.FirstOrDefaultAsync(c => c.IsActive);
        if (smtpConfig == null)
            return;

        using var message = new MailMessage
        {
            From = new MailAddress(smtpConfig.From!),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };
        message.To.Add(toEmail);

        if (attachmentPaths != null)
        {
            foreach (var path in attachmentPaths)
            {
                if (File.Exists(path))
                    message.Attachments.Add(new Attachment(path));
            }
        }

        using var client = new SmtpClient(smtpConfig.Host, smtpConfig.Port)
        {
            Credentials = new NetworkCredential(smtpConfig.UserName, smtpConfig.Password),
            EnableSsl = smtpConfig.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 15000
        };

        try
        {
            await client.SendMailAsync(message);
        }
        catch (SmtpException ex)
        {
            var inner = ex.InnerException?.Message;
            throw new InvalidOperationException(
                $"SMTP error: {ex.Message}{(inner != null ? $" ({inner})" : "")}. " +
                $"Host: {smtpConfig.Host}:{smtpConfig.Port}, SSL: {smtpConfig.EnableSsl}", ex);
        }
    }
}
