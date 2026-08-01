using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Semesters;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ISemesterEnrollmentService
{
    Task<(List<SemesterEnrollmentListItemDto> Items, int TotalCount)> GetEnrollmentsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? admissionId = null, int? collegeId = null, int? programId = null, int? semesterId = null, int? academicYearId = null);
    Task<List<SemesterEnrollmentListItemDto>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? admissionId = null, int? collegeId = null, int? programId = null, int? semesterId = null, int? academicYearId = null);
    Task<SemesterEnrollment?> GetEnrollmentByIdAsync(int id);
    Task CreateEnrollmentAsync(SemesterEnrollment enrollment);
    Task UpdateEnrollmentAsync(SemesterEnrollment enrollment);
    Task DeleteEnrollmentAsync(int id);
    Task<bool> EnrollmentExistsAsync(int id);
    Task<List<Domain.Entities.Students.StudentAdmission>> GetActiveAdmissionsAsync();
    Task<List<Semester>> GetSemestersByProgramAsync(int programId, int? academicYearId = null);
    Task<(List<SemesterEnrollmentCandidateDto> Items, int TotalCount)> GetEnrollmentCandidatesAsync(string? search, int? academicYearId, int? collegeId, int? programId, int? semesterId, int page = 1, int pageSize = 25);
    Task<(int Created, int Skipped)> BulkCreateEnrollmentsAsync(List<int> admissionIds, int semesterId);
    Task<(int Created, int Skipped)> BulkCreateAllEnrollmentsAsync(string? search, int? academicYearId, int? collegeId, int? programId, int semesterId);
    Task<int> PromoteCompletedSemestersAsync();
}
