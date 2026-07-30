using FWU.Exam.Management.Domain.Entities.Payments;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IKhaltiConfigurationService
{
    Task<KhaltiConfiguration?> GetActiveAsync();
}
