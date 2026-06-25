using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ISmsConfigurationService
{
    Task<List<SmsConfiguration>> GetAllAsync();
    Task<SmsConfiguration?> GetByIdAsync(int id);
    Task CreateAsync(SmsConfiguration config);
    Task UpdateAsync(SmsConfiguration config);
    Task DeleteAsync(int id);
}
