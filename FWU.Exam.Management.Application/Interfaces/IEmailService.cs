using System.Threading.Tasks;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
}
