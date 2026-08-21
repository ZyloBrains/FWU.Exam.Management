using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IExamCenterDistributionService
{
    Task<int> DistributeStudentsAsync(int examScheduleId);
    Task MoveStudentToCenterAsync(int registrationId, int examCenterId);
    Task<List<DistributedStudentInfo>> GetDistributedStudentsAsync(int examScheduleId);
    Task ResetDistributionAsync(int examScheduleId);
    Task<int> GetRegisteredCountAsync(int examScheduleId);
    Task<int> GetAssignedCountAsync(int examScheduleId);
    Task<int> GetUnassignedCountAsync(int examScheduleId);
    Task<Dictionary<int, int>> GetCenterDistributionCountsAsync(int examScheduleId);
}
