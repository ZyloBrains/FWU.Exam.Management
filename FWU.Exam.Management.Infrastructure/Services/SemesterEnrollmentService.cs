using System.Linq.Expressions;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class SemesterEnrollmentService(AppDbContext context, IUserContext userContext) : ISemesterEnrollmentService
{
    public async Task<(List<SemesterEnrollment> Items, int TotalCount)> GetEnrollmentsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? admissionId = null)
    {
        var query = BuildQuery(search, admissionId);
        var totalCount = await query.CountAsync();

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<SemesterEnrollment>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? admissionId = null)
    {
        var query = BuildQuery(search, admissionId);

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<SemesterEnrollment?> GetEnrollmentByIdAsync(int id)
    {
        return await context.SemesterEnrollments
            .Include(se => se.StudentAdmission)
                .ThenInclude(sa => sa!.College)
            .Include(se => se.StudentAdmission)
                .ThenInclude(sa => sa!.Program)
            .Include(se => se.Semester)
            .AsNoTracking()
            .FirstOrDefaultAsync(se => se.Id == id);
    }

    public async Task CreateEnrollmentAsync(SemesterEnrollment enrollment)
    {
        enrollment.EnrolledDate = DateTime.UtcNow;
        enrollment.EnrollmentStatus = StudentEnrollmentStatus.Active;
        enrollment.PaymentStatus = PaymentStatus.Pending;
        enrollment.ResultStatus = ResultStatus.Incomplete;

        context.SemesterEnrollments.Add(enrollment);
        await context.SaveChangesAsync();
    }

    public async Task UpdateEnrollmentAsync(SemesterEnrollment enrollment)
    {
        context.SemesterEnrollments.Update(enrollment);
        await context.SaveChangesAsync();
    }

    public async Task DeleteEnrollmentAsync(int id)
    {
        var enrollment = await context.SemesterEnrollments.FindAsync(id);
        if (enrollment != null)
        {
            context.SemesterEnrollments.Remove(enrollment);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> EnrollmentExistsAsync(int id)
    {
        return await context.SemesterEnrollments.AnyAsync(se => se.Id == id);
    }

    public async Task<List<StudentAdmission>> GetActiveAdmissionsAsync()
    {
        var query = context.StudentAdmissions
            .Include(sa => sa.College)
            .Include(sa => sa.Program)
            .AsNoTracking()
            .Where(sa => sa.IsActive);

        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
                query = query.Where(sa => sa.CollegeId == userContext.CollegeId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<List<Semester>> GetSemestersByProgramAsync(int programId)
    {
        return await context.SubjectOfferings
            .Where(so => so.ProgramId == programId)
            .Select(so => so.Semester!)
            .Distinct()
            .OrderBy(s => s.Number)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> PromoteCompletedSemestersAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var created = 0;

        var activeEnrollments = await context.SemesterEnrollments
            .AsNoTracking()
            .Include(se => se.StudentAdmission)
            .Include(se => se.Semester)
            .Where(se => se.EnrollmentStatus == StudentEnrollmentStatus.Active)
            .ToListAsync();

        foreach (var enrollment in activeEnrollments)
        {
            var admission = enrollment.StudentAdmission;
            var semester = enrollment.Semester;
            if (admission == null || semester == null) continue;

            var mainSchedule = await GetMainExamScheduleAsync(admission.ProgramsId, semester.Id);
            if (mainSchedule == null) continue;

            var endedDate = mainSchedule.EndDate;
            if (mainSchedule.ExtendedDate.HasValue)
            {
                var extended = DateOnly.FromDateTime(mainSchedule.ExtendedDate.Value);
                if (endedDate == null || extended > endedDate)
                    endedDate = extended;
            }

            if (endedDate == null || endedDate >= today) continue;
            if (mainSchedule.AdmissionCardReleaseDate == null ||
                mainSchedule.AdmissionCardReleaseDate.Value.Date >= DateTime.UtcNow.Date)
            {
                continue;
            }

            var programSemesters = await GetSemestersByProgramAsync(admission.ProgramsId);
            var nextSemester = programSemesters
                .FirstOrDefault(s => s.Year == semester.Year && s.Number == semester.Number + 1)
                ?? programSemesters
                    .FirstOrDefault(s => s.Year == semester.Year + 1 && s.Number == 1);
            if (nextSemester == null) continue;

            var alreadyEnrolled = await context.SemesterEnrollments
                .AnyAsync(se => se.StudentAdmissionId == admission.Id && se.SemesterId == nextSemester.Id);
            if (alreadyEnrolled) continue;

            context.SemesterEnrollments.Add(new SemesterEnrollment
            {
                TenantId = admission.TenantId,
                StudentAdmissionId = admission.Id,
                SemesterId = nextSemester.Id,
                EnrollmentStatus = StudentEnrollmentStatus.Active,
                EnrollmentType = enrollment.EnrollmentType,
                PaymentStatus = PaymentStatus.Pending,
                ResultStatus = ResultStatus.Incomplete,
                EnrolledDate = DateTime.UtcNow,
                TotalCredits = 0,
                GradePoints = 0,
                TotalFee = 0,
                PaidAmount = 0,
                Deficiency = false
            });
            created++;
        }

        if (created > 0)
            await context.SaveChangesAsync();

        return created;
    }

    private async Task<Domain.Entities.Exams.ExamSchedule?> GetMainExamScheduleAsync(int programId, int semesterId)
    {
        var schedules = await context.ExamSchedules
            .AsNoTracking()
            .Include(es => es.ExamType)
            .Where(es => es.IsActive
                      && es.ProgramId == programId
                      && es.SemesterId == semesterId
                      && es.ExamType != null
                      && es.ExamType.Name != "Entrance"
                      && es.ExamType.Name != "Supplementary")
            .OrderBy(es => es.ExamType!.Name == "Regular" ? 0 : 1)
            .ThenByDescending(es => es.Id)
            .ToListAsync();

        return schedules.FirstOrDefault();
    }

    private IQueryable<SemesterEnrollment> BuildQuery(string? search, int? admissionId = null)
    {
        var query = context.SemesterEnrollments
            .Include(se => se.StudentAdmission)
                .ThenInclude(sa => sa!.College)
            .Include(se => se.StudentAdmission)
                .ThenInclude(sa => sa!.Program)
            .Include(se => se.Semester)
            .AsNoTracking();

        if (admissionId.HasValue)
            query = query.Where(se => se.StudentAdmissionId == admissionId.Value);

        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
                query = query.Where(se => se.StudentAdmission != null && se.StudentAdmission.CollegeId == userContext.CollegeId.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(se =>
                se.Semester!.Name!.Contains(search) ||
                se.StudentAdmission!.CollegeRollNumber!.Contains(search));
        }

        return query;
    }

    private static Expression<Func<SemesterEnrollment, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "semester" => se => se.Semester!.Name!,
            "enrollmentstatus" => se => se.EnrollmentStatus,
            "enrollmenttype" => se => se.EnrollmentType,
            "enrolleddate" => se => se.EnrolledDate,
            "resultstatus" => se => se.ResultStatus,
            _ => se => se.EnrolledDate
        };
    }
}