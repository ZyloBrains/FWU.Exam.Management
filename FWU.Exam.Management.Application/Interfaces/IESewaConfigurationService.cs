using FWU.Exam.Management.Domain.Entities.Payments;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IESewaConfigurationService
{
    Task<ESewaConfiguration?> GetActiveAsync();
}
