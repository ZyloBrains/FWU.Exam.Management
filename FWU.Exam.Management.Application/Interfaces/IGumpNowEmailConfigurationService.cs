using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IGumpNowEmailConfigurationService
{
    Task<List<GumpNowEmailConfiguration>> GetAllAsync();
    Task<GumpNowEmailConfiguration?> GetByIdAsync(int id);
    Task CreateAsync(GumpNowEmailConfiguration config);
    Task UpdateAsync(GumpNowEmailConfiguration config);
    Task DeleteAsync(int id);
}
