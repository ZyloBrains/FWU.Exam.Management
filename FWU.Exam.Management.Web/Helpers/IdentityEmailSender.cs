using FWU.Exam.Management.Application.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace FWU.Exam.Management.Web.Helpers;

public class IdentityEmailSender(IEmailService emailService) : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await emailService.SendEmailAsync(email, subject, htmlMessage, isHtml: true);
    }
}
