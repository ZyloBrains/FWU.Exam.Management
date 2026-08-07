namespace FWU.Exam.Management.Application.Interfaces;

public interface IGumpNowEmailService
{
    Task SendEmailAsync(string toAddr, string subject, string templateId, Dictionary<string, string> context);
    Task SendHtmlEmailAsync(string toAddr, string subject, string htmlContent);
}
