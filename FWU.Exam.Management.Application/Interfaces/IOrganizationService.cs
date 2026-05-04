using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IOrganizationService
{
    Task<Organization?> GetOrganizationAsync();
    Task<Organization?> GetOrganizationByOfficeCodeAsync(string officeCode);
}
