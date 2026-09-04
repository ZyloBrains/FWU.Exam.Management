using System.Linq.Expressions;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class SemesterEnrollmentService(AppDbContext context, IUserContext userContext) : ISemesterEnrollmentService
{
    public async Task<(List<SemesterEnrollmentListItemDto> Items, int TotalCount)> GetEnrollmentsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? admissionId = null, int? collegeId = null, int? programId = null, int? semesterInstanceId = null, int? academicYearId = null)
    {
        var query = BuildQuery(search, admissionId, collegeId, programId, semesterInstanceId, academicYearId);
        var totalCount = await query.CountAsync();

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToListItemDto)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<SemesterEnrollmentListItemDto>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? admissionId = null, int? collegeId = null, int? programId = null, int? semesterInstanceId = null, int? academicYearId = null)
    {
        var query = BuildQuery(search, admissionId, collegeId, programId, semesterInstanceId, academicYearId);

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToListItemDto)
            .ToListAsync();
    }

    public async Task<SemesterEnrollment?> GetEnrollmentByIdAsync(int id)
    {
        return await context.SemesterEnrollments
            .Include(se => se.StudentAdmission)
                .ThenInclude(sa => sa!.College)
            .Include(se => se.StudentAdmission)
                .ThenInclude(sa => sa!.Program)
            .Include(se => se.SemesterInstance)
                .ThenInclude(si => si!.Semester)
            .AsNoTracking()
            .FirstOrDefaultAsync(se => se.Id == id);
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

    public async Task<List<SemesterInstance>> GetSemesterInstancesByProgramAsync(int programId, int? academicYearId = null)
    {
        var query = context.SemesterInstances
            .Include(si => si.Semester)
            .Include(si => si.AcademicYear)
            .Where(si => si.ProgramId == programId)
            .AsNoTracking();

        if (academicYearId.HasValue)
            query = query.Where(si => si.AcademicYearId == academicYearId.Value);

        return await query
            .OrderBy(si => si.Semester!.Number)
            .ToListAsync();
    }

    public async Task<(List<SemesterEnrollmentCandidateDto> Items, int TotalCount)> GetEnrollmentCandidatesAsync(string? search, int? academicYearId, int? collegeId, int? programId, int? semesterInstanceId, int page = 1, int pageSize = 25)
    {
        var query = BuildCandidateQuery(search, academicYearId, collegeId, programId);

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 25;

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(sa => sa.CollegeRollNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(sa => new SemesterEnrollmentCandidateDto
            {
                AdmissionId = sa.Id,
                StudentName = sa.FirstName + (sa.MiddleName != null ? " " + sa.MiddleName : "") + (sa.LastName != null ? " " + sa.LastName : ""),
                RegistrationNumber = sa.StudentRegistration != null ? sa.StudentRegistration.RegistrationNumber : null,
                CollegeRollNumber = sa.CollegeRollNumber,
                ProgramName = sa.Program != null ? sa.Program.ProgramName : null,
                CollegeName = sa.College != null ? sa.College.Name : null,
                AcademicYearName = sa.AcademicYear != null ? sa.AcademicYear.AcademicYearCode : null,
                IsEnrolled = semesterInstanceId.HasValue && context.SemesterEnrollments.Any(se =>
                    se.StudentAdmissionId == sa.Id && se.SemesterInstanceId == semesterInstanceId.Value)
            })
            .ToListAsync();

        return (items, totalCount);
    }

    private IQueryable<StudentAdmission> BuildCandidateQuery(string? search, int? academicYearId, int? collegeId, int? programId)
    {
        var query = context.StudentAdmissions
            .AsNoTracking()
            .Include(sa => sa.StudentRegistration)
            .Where(sa => sa.IsActive);

        if (academicYearId.HasValue)
            query = query.Where(sa => sa.AcademicYearId == academicYearId.Value);

        if (collegeId.HasValue)
            query = query.Where(sa => sa.CollegeId == collegeId.Value);

        if (programId.HasValue)
            query = query.Where(sa => sa.ProgramsId == programId.Value);

        if (!userContext.IsSuperAdmin && userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
            query = query.Where(sa => sa.CollegeId == userContext.CollegeId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(sa =>
                (sa.CollegeRollNumber != null && sa.CollegeRollNumber.Contains(term)) ||
                (sa.StudentRegistration != null && sa.StudentRegistration.RegistrationNumber != null && sa.StudentRegistration.RegistrationNumber.Contains(term)) ||
                (sa.Program != null && sa.Program.ProgramName.Contains(term)) ||
                ((sa.FirstName + (sa.MiddleName != null ? " " + sa.MiddleName : "") + (sa.LastName != null ? " " + sa.LastName : "")).Contains(term)));
        }

        return query;
    }

    public async Task<(int Created, int Skipped)> BulkCreateAllEnrollmentsAsync(string? search, int? academicYearId, int? collegeId, int? programId, int semesterInstanceId, EnrollmentType? enrollmentType = null)
    {
        var admissionIds = await BuildCandidateQuery(search, academicYearId, collegeId, programId)
            .Select(sa => sa.Id)
            .ToListAsync();

        if (admissionIds.Count == 0)
            return (0, 0);

        return await BulkCreateEnrollmentsAsync(admissionIds, semesterInstanceId, enrollmentType);
    }

    public async Task<(int Created, int Skipped)> BulkCreateEnrollmentsAsync(List<int> admissionIds, int semesterInstanceId, EnrollmentType? enrollmentType = null)
    {
        if (admissionIds == null || admissionIds.Count == 0)
            return (0, 0);

        var distinctIds = admissionIds.Distinct().ToList();

        var admissionsQuery = context.StudentAdmissions
            .AsNoTracking()
            .Where(sa => distinctIds.Contains(sa.Id));

        if (!userContext.IsSuperAdmin && userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
            admissionsQuery = admissionsQuery.Where(sa => sa.CollegeId == userContext.CollegeId.Value);

        var admissions = await admissionsQuery.ToListAsync();
        if (admissions.Count == 0)
            return (0, 0);

        var scopedIds = admissions.Select(a => a.Id).ToList();

        var alreadyEnrolled = await context.SemesterEnrollments
            .Where(se => se.SemesterInstanceId == semesterInstanceId && scopedIds.Contains(se.StudentAdmissionId))
            .Select(se => se.StudentAdmissionId)
            .ToListAsync();

        var toCreate = admissions.Where(a => !alreadyEnrolled.Contains(a.Id)).ToList();
        var skipped = admissionIds.Count - toCreate.Count;
        var resolvedType = enrollmentType ?? EnrollmentType.FullTime;

        foreach (var admission in toCreate)
        {
            context.SemesterEnrollments.Add(new SemesterEnrollment
            {
                TenantId = admission.TenantId,
                StudentAdmissionId = admission.Id,
                SemesterInstanceId = semesterInstanceId,
                EnrollmentStatus = StudentEnrollmentStatus.Active,
                EnrollmentType = resolvedType,
                PaymentStatus = PaymentStatus.Pending,
                ResultStatus = ResultStatus.Incomplete,
                EnrolledDate = DateTime.UtcNow,
                TotalCredits = 0,
                GradePoints = 0,
                TotalFee = 0,
                PaidAmount = 0,
                Deficiency = false
            });
        }

        if (toCreate.Count > 0)
            await context.SaveChangesAsync();

        return (toCreate.Count, skipped);
    }

    public async Task<bool> EnrollInFirstSemesterAsync(int admissionId)
    {
        var admission = await context.StudentAdmissions
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.Id == admissionId);
        if (admission == null) return false;

        var alreadyEnrolled = await context.SemesterEnrollments
            .AnyAsync(se => se.StudentAdmissionId == admissionId);
        if (alreadyEnrolled) return false;

        var firstProgramSemester = await context.ProgramSemesters
            .AsNoTracking()
            .Where(ps => ps.ProgramId == admission.ProgramsId && ps.IsActive)
            .OrderBy(ps => ps.DisplayOrder)
            .FirstOrDefaultAsync();
        if (firstProgramSemester == null) return false;

        var firstSemester = await context.Semesters
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == firstProgramSemester.SemesterId);
        if (firstSemester == null) return false;

        var currentAcademicYearId = admission.AcademicYearId;
        var semesterInstance = await context.SemesterInstances
            .AsNoTracking()
            .FirstOrDefaultAsync(si => si.SemesterId == firstSemester.Id && si.AcademicYearId == currentAcademicYearId && si.ProgramId == admission.ProgramsId);
        if (semesterInstance == null) return false;

        context.SemesterEnrollments.Add(new SemesterEnrollment
        {
            TenantId = admission.TenantId,
            StudentAdmissionId = admission.Id,
            SemesterInstanceId = semesterInstance.Id,
            EnrollmentStatus = StudentEnrollmentStatus.Active,
            EnrollmentType = EnrollmentType.FullTime,
            PaymentStatus = PaymentStatus.Pending,
            ResultStatus = ResultStatus.Incomplete,
            EnrolledDate = DateTime.UtcNow,
            TotalCredits = 0,
            GradePoints = 0,
            TotalFee = 0,
            PaidAmount = 0,
            Deficiency = false
        });
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<int> PromoteCompletedSemestersAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var created = 0;

        var activeEnrollments = await context.SemesterEnrollments
            .AsNoTracking()
            .Include(se => se.StudentAdmission)
            .Include(se => se.SemesterInstance)
                .ThenInclude(si => si!.Semester)
            .Where(se => se.EnrollmentStatus == StudentEnrollmentStatus.Active)
            .ToListAsync();

        foreach (var enrollment in activeEnrollments)
        {
            var admission = enrollment.StudentAdmission;
            var semesterInstance = enrollment.SemesterInstance;
            var semester = semesterInstance?.Semester;
            if (admission == null || semesterInstance == null || semester == null) continue;

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

            var submittedExamForm = await context.ExamRegistrations
                .AsNoTracking()
                .AnyAsync(er => er.SemesterEnrollmentId == enrollment.Id
                             && er.IsActive
                             && er.Status != RegistrationStatus.Rejected);
            if (!submittedExamForm) continue;

            var programSemesters = await context.ProgramSemesters
                .AsNoTracking()
                .Where(ps => ps.ProgramId == admission.ProgramsId && ps.IsActive)
                .Include(ps => ps.Semester)
                .OrderBy(ps => ps.DisplayOrder)
                .ToListAsync();

            var currentOrder = programSemesters.FirstOrDefault(ps => ps.SemesterId == semester.Id)?.DisplayOrder ?? 0;
            var nextProgramSemester = programSemesters.FirstOrDefault(ps => ps.DisplayOrder == currentOrder + 1);
            if (nextProgramSemester == null) continue;

            var nextSemester = nextProgramSemester.Semester;
            if (nextSemester == null) continue;

            var nextSemesterInstance = await context.SemesterInstances
                .AsNoTracking()
                .FirstOrDefaultAsync(si => si.SemesterId == nextSemester.Id
                                        && si.AcademicYearId == semesterInstance.AcademicYearId
                                        && si.ProgramId == admission.ProgramsId);
            if (nextSemesterInstance == null) continue;

            var alreadyEnrolled = await context.SemesterEnrollments
                .AnyAsync(se => se.StudentAdmissionId == admission.Id && se.SemesterInstanceId == nextSemesterInstance.Id);
            if (alreadyEnrolled) continue;

            context.SemesterEnrollments.Add(new SemesterEnrollment
            {
                TenantId = admission.TenantId,
                StudentAdmissionId = admission.Id,
                SemesterInstanceId = nextSemesterInstance.Id,
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
                      && es.SemesterInstanceId == semesterId
                      && es.ExamType != null
                      && es.ExamType.Name != "Entrance"
                      && es.ExamType.Name != "Supplementary"
                      && es.ExamType.Name != "Partial"
                      && es.ExamType.Name != "Chance"
                      && es.ExamType.Name != "Special Chance")
            .OrderBy(es => es.ExamType!.Name == "Regular" ? 0 : 1)
            .ThenByDescending(es => es.Id)
            .ToListAsync();

        return schedules.FirstOrDefault();
    }

    private IQueryable<SemesterEnrollment> BuildQuery(string? search, int? admissionId = null, int? collegeId = null, int? programId = null, int? semesterInstanceId = null, int? academicYearId = null)
    {
        var query = context.SemesterEnrollments
            .Include(se => se.StudentAdmission)
                .ThenInclude(sa => sa!.College)
            .Include(se => se.StudentAdmission)
                .ThenInclude(sa => sa!.Program)
            .Include(se => se.StudentAdmission)
                .ThenInclude(sa => sa!.StudentRegistration)
            .Include(se => se.SemesterInstance)
                .ThenInclude(si => si!.Semester)
            .AsNoTracking();

        if (admissionId.HasValue)
            query = query.Where(se => se.StudentAdmissionId == admissionId.Value);

        if (collegeId.HasValue)
            query = query.Where(se => se.StudentAdmission != null && se.StudentAdmission.CollegeId == collegeId.Value);

        if (programId.HasValue)
            query = query.Where(se => se.StudentAdmission != null && se.StudentAdmission.ProgramsId == programId.Value);

        if (semesterInstanceId.HasValue)
            query = query.Where(se => se.SemesterInstanceId == semesterInstanceId.Value);

        if (academicYearId.HasValue)
            query = query.Where(se => se.StudentAdmission != null && se.StudentAdmission.AcademicYearId == academicYearId.Value);

        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
                query = query.Where(se => se.StudentAdmission != null && se.StudentAdmission.CollegeId == userContext.CollegeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(se =>
                (se.SemesterInstance != null && se.SemesterInstance.Semester != null && se.SemesterInstance.Semester.Name != null && se.SemesterInstance.Semester.Name.Contains(term)) ||
                (se.StudentAdmission != null && se.StudentAdmission.CollegeRollNumber != null && se.StudentAdmission.CollegeRollNumber.Contains(term)) ||
                (se.StudentAdmission != null && se.StudentAdmission.StudentRegistration != null && se.StudentAdmission.StudentRegistration.RegistrationNumber != null && se.StudentAdmission.StudentRegistration.RegistrationNumber.Contains(term)) ||
                (se.StudentAdmission != null && (se.StudentAdmission.FirstName + (se.StudentAdmission.MiddleName != null ? " " + se.StudentAdmission.MiddleName : "") + (se.StudentAdmission.LastName != null ? " " + se.StudentAdmission.LastName : "")).Contains(term)));
        }

        return query;
    }

    private readonly System.Linq.Expressions.Expression<Func<SemesterEnrollment, SemesterEnrollmentListItemDto>> ToListItemDto = se =>
        new SemesterEnrollmentListItemDto
        {
            Id = se.Id,
            StudentName = se.StudentAdmission!.FirstName + (se.StudentAdmission.MiddleName != null ? " " + se.StudentAdmission.MiddleName : "") + (se.StudentAdmission.LastName != null ? " " + se.StudentAdmission.LastName : ""),
            RegistrationNumber = se.StudentAdmission!.StudentRegistration != null ? se.StudentAdmission.StudentRegistration.RegistrationNumber : null,
            CollegeRollNumber = se.StudentAdmission!.CollegeRollNumber,
            ProgramName = se.StudentAdmission!.Program != null ? se.StudentAdmission.Program.ProgramName : null,
            CollegeName = se.StudentAdmission!.College != null ? se.StudentAdmission.College.Name : null,
            SemesterName = se.SemesterInstance != null && se.SemesterInstance.Semester != null ? se.SemesterInstance.Semester.Name : null,
            AcademicYearName = se.StudentAdmission != null && se.StudentAdmission.AcademicYear != null ? se.StudentAdmission.AcademicYear.AcademicYearCode : null,
            EnrollmentStatus = se.EnrollmentStatus,
            EnrollmentType = se.EnrollmentType,
            PaymentStatus = se.PaymentStatus,
            ResultStatus = se.ResultStatus,
            TotalFee = se.TotalFee,
            TotalCredits = se.TotalCredits
        };

    private static Expression<Func<SemesterEnrollment, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "semester" => se => se.SemesterInstance != null && se.SemesterInstance.Semester != null ? se.SemesterInstance.Semester.Name! : "",
            "studentname" => se => se.StudentAdmission!.CollegeRollNumber!,
            "enrollmentstatus" => se => se.EnrollmentStatus,
            "enrollmenttype" => se => se.EnrollmentType,
            "enrolleddate" => se => se.EnrolledDate,
            "resultstatus" => se => se.ResultStatus,
            _ => se => se.EnrolledDate
        };
    }
}
