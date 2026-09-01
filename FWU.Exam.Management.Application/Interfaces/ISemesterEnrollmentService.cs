using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Enums;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ISemesterEnrollmentService
{
    Task<(List<SemesterEnrollmentListItemDto> Items, int TotalCount)> GetEnrollmentsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? admissionId = null, int? collegeId = null, int? programId = null, int? semesterInstanceId = null, int? academicYearId = null);
    Task<List<SemesterEnrollmentListItemDto>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? admissionId = null, int? collegeId = null, int? programId = null, int? semesterInstanceId = null, int? academicYearId = null);
    Task<SemesterEnrollment?> GetEnrollmentByIdAsync(int id);
    Task UpdateEnrollmentAsync(SemesterEnrollment enrollment);
    Task DeleteEnrollmentAsync(int id);
    Task<bool> EnrollmentExistsAsync(int id);
    Task<List<Domain.Entities.Students.StudentAdmission>> GetActiveAdmissionsAsync();
    Task<List<SemesterInstance>> GetSemesterInstancesByProgramAsync(int programId, int? academicYearId = null);
    Task<(List<SemesterEnrollmentCandidateDto> Items, int TotalCount)> GetEnrollmentCandidatesAsync(string? search, int? academicYearId, int? collegeId, int? programId, int? semesterInstanceId, int page = 1, int pageSize = 25);
    Task<(int Created, int Skipped)> BulkCreateEnrollmentsAsync(List<int> admissionIds, int semesterInstanceId, EnrollmentType? enrollmentType = null);
    Task<(int Created, int Skipped)> BulkCreateAllEnrollmentsAsync(string? search, int? academicYearId, int? collegeId, int? programId, int semesterInstanceId, EnrollmentType? enrollmentType = null);
    Task<bool> EnrollInFirstSemesterAsync(int admissionId);
    Task<int> PromoteCompletedSemestersAsync();
}
