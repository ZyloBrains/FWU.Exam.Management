using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IOrganizationService
{
    Task<List<Organization>> GetAllOrganizationsAsync();
    Task<Organization?> GetOrganizationByIdAsync(int id);
    Task<Organization?> GetOrganizationByOfficeCodeAsync(string officeCode);
    Task CreateOrganizationAsync(Organization organization);
    Task UpdateOrganizationAsync(Organization organization);
    Task DeleteOrganizationAsync(int id);
    Task<bool> OrganizationExistsAsync(int id);
}
