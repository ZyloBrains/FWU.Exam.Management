using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IBulkUserCreationService
{
    Task<(List<StudentWithoutUserDto> Data, int TotalCount)> GetStudentsWithoutUsersAsync(int? collegeId, int? facultyId, int? programId, int page, int pageSize);
    Task<BulkUserCreationJob> StartJobAsync(List<int> registrationIds, string userId);
    Task<BulkUserCreationJob> StartJobFromFiltersAsync(int? collegeId, int? facultyId, int? programId, string userId);
    Task<BulkUserCreationJob?> GetJobStatusAsync(int jobId);
}
