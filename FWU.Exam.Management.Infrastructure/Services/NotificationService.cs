using FWU.Exam.Management.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;

public class NotificationService(
    AppDbContext context,
    INotificationTemplateService templateService,
    IEmailService emailService,
    IGumpNowEmailService gumpNowEmailService,
    ISmsService smsService,
    ILogger<NotificationService> logger) : INotificationService
{
    public async Task<NotificationResult> SendAsync(
        string? email,
        string? phone,
        string templateCode,
        IReadOnlyDictionary<string, string> context,
        NotificationEmailChannel emailChannel = NotificationEmailChannel.Auto)
    {
        var result = new NotificationResult();

        if (!string.IsNullOrWhiteSpace(email))
        {
            var provider = await ResolveEmailProviderAsync(emailChannel);

            if (provider == null)
            {
                logger.LogWarning("No active email gateway configured; skipping {TemplateCode} email to {Email}", templateCode, email);
                result = result with { EmailError = "No active email gateway (SMTP or GumpNow) is configured." };
            }
            else
            {
                try
                {
                    var rendered = await templateService.RenderEmailAsync(templateCode, context);
                    if (provider == NotificationEmailChannel.GumpNow)
                    {
                        await gumpNowEmailService.SendHtmlEmailAsync(email, rendered.Subject, rendered.BodyHtml);
                    }
                    else
                    {
                        await emailService.SendEmailAsync(email, rendered.Subject, rendered.BodyHtml, isHtml: true);
                    }

                    result = result with { EmailSent = true };
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to send {TemplateCode} email to {Email}", templateCode, email);
                    result = result with { EmailError = ex.Message };
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(phone))
        {
            try
            {
                var rendered = await templateService.RenderSmsAsync(templateCode, context);
                await smsService.SendSmsAsync(phone, rendered.Body);

                result = result with { SmsSent = true };
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send {TemplateCode} SMS to {Phone}", templateCode, phone);
                result = result with { SmsError = ex.Message };
            }
        }

        return result;
    }

    /// <summary>
    /// Resolves the email gateway to use. Auto prefers SMTP when an active SMTP
    /// configuration exists, otherwise falls back to the active GumpNow gateway.
    /// Returns null when the requested gateway is not configured.
    /// </summary>
    private async Task<NotificationEmailChannel?> ResolveEmailProviderAsync(NotificationEmailChannel requested)
    {
        if (requested == NotificationEmailChannel.Auto)
        {
            if (await context.SmtpConfigurations.AnyAsync(c => c.IsActive))
                return NotificationEmailChannel.Smtp;

            if (await context.GumpNowEmailConfigurations.AnyAsync(c => c.IsActive))
                return NotificationEmailChannel.GumpNow;

            return null;
        }

        var configured = requested == NotificationEmailChannel.Smtp
            ? await context.SmtpConfigurations.AnyAsync(c => c.IsActive)
            : await context.GumpNowEmailConfigurations.AnyAsync(c => c.IsActive);

        return configured ? requested : null;
    }
}
