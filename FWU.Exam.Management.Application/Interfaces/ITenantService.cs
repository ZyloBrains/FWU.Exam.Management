using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ITenantService
{
    Task<Tenant?> GetTenantAsync();
    Task<Tenant?> GetTenantByOfficeCodeAsync(string officeCode);
}
