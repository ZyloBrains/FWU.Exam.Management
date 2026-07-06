using System.Threading.Tasks;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ISmsService
{
    Task SendSmsAsync(string toPhoneNumber, string message);
}
