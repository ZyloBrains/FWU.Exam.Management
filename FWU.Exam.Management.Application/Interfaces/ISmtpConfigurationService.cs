using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ISmtpConfigurationService
{
    Task<(List<SmtpConfiguration> Items, int TotalCount)> GetSmtpConfigurationsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<SmtpConfiguration>> GetFilteredSmtpConfigurationsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<SmtpConfiguration?> GetSmtpConfigurationByIdAsync(int id);
    Task CreateSmtpConfigurationAsync(SmtpConfiguration smtpConfiguration);
    Task UpdateSmtpConfigurationAsync(SmtpConfiguration smtpConfiguration);
    Task DeleteSmtpConfigurationAsync(int id);
    Task<bool> SmtpConfigurationExistsAsync(int id);
}
