using FWU.Exam.Management.Domain.Entities.Semesters;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ISemesterEnrollmentService
{
    Task<(List<SemesterEnrollment> Items, int TotalCount)> GetEnrollmentsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? admissionId = null);
    Task<List<SemesterEnrollment>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? admissionId = null);
    Task<SemesterEnrollment?> GetEnrollmentByIdAsync(int id);
    Task CreateEnrollmentAsync(SemesterEnrollment enrollment);
    Task UpdateEnrollmentAsync(SemesterEnrollment enrollment);
    Task DeleteEnrollmentAsync(int id);
    Task<bool> EnrollmentExistsAsync(int id);
    Task<List<Domain.Entities.Students.StudentAdmission>> GetActiveAdmissionsAsync();
    Task<List<Semester>> GetSemestersByProgramAsync(int programId);
    Task<int> PromoteCompletedSemestersAsync();
}