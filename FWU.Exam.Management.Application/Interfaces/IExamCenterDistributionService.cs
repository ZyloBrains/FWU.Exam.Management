using FWU.Exam.Management.Domain.Entities.Exams;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IExamCenterDistributionService
{
    Task AssignSymbolNumbersAsync(int examScheduleId);
    Task<List<ExamCenterSymbolRange>> GetRangesAsync(int examScheduleId);
    Task SetSymbolRangeAsync(int examCenterId, int examScheduleId, long fromSymbol, long toSymbol);
    Task ClearRangesAsync(int examScheduleId);
    Task<int> DistributeStudentsAsync(int examScheduleId);
    Task ResetDistributionAsync(int examScheduleId);
    Task<int> GetRegisteredCountAsync(int examScheduleId);
    Task<int> GetAssignedCountAsync(int examScheduleId);
    Task<int> GetUnassignedCountAsync(int examScheduleId);
    Task<Dictionary<int, int>> GetCenterDistributionCountsAsync(int examScheduleId);
}
