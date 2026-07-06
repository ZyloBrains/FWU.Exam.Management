using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ExamRegistrationService(AppDbContext context) : IExamRegistrationService
{
    private IQueryable<ExamRegistration> ApplyScope(IQueryable<ExamRegistration> query, int? collegeId, int? facultyId)
    {
        if (collegeId.HasValue)
            return query.Where(e => e.CollegeId == collegeId.Value);

        if (facultyId.HasValue)
        {
            var collegeIds = context.Colleges
                .Where(c => c.Faculties.Any(f => f.Id == facultyId.Value))
                .Select(c => c.Id)
                .ToList();

            return query.Where(e => collegeIds.Contains(e.CollegeId));
        }

        return query;
    }

    public async Task<(List<ExamRegistration> Items, int TotalCount)> GetExamRegistrationsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? collegeId = null, int? facultyId = null, int? examScheduleId = null)
    {
        var query = ApplyScope(BuildQuery(search, sort, sortDir, examScheduleId), collegeId, facultyId);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExamRegistration
            {
                Id = e.Id,
                ExamScheduleId = e.ExamScheduleId,
                CollegeId = e.CollegeId,
                AcademicYearId = e.AcademicYearId,
                ExamCenterId = e.ExamCenterId,
                ProgramsId = e.ProgramsId,
                ExamRollNumber = e.ExamRollNumber,
                FeeEnclosed = e.FeeEnclosed,
                AttendancePercentage = e.AttendancePercentage,
                RegistrationDate = e.RegistrationDate,
                Status = e.Status,
                Sgpa = e.Sgpa,
                Remarks = e.Remarks,
                IsActive = e.IsActive,
                RollNumberIndex = e.RollNumberIndex,
                IsAppliedByStudent = e.IsAppliedByStudent,
                ExamSchedule = e.ExamSchedule,
                College = e.College,
                ExamCenter = e.ExamCenter,
                AcademicYear = e.AcademicYear,
                Program = e.Program
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<ExamRegistration>> GetFilteredItemsAsync(string? search, int? collegeId = null, int? facultyId = null)
    {
        var query = ApplyScope(BuildQuery(search, "Id", "asc", null), collegeId, facultyId);
        return await query
            .Select(e => new ExamRegistration
            {
                Id = e.Id,
                ExamScheduleId = e.ExamScheduleId,
                CollegeId = e.CollegeId,
                AcademicYearId = e.AcademicYearId,
                ExamCenterId = e.ExamCenterId,
                ProgramsId = e.ProgramsId,
                ExamRollNumber = e.ExamRollNumber,
                FeeEnclosed = e.FeeEnclosed,
                RegistrationDate = e.RegistrationDate,
                Status = e.Status,
                Sgpa = e.Sgpa,
                Remarks = e.Remarks,
                IsActive = e.IsActive,
                ExamSchedule = e.ExamSchedule,
                College = e.College,
                AcademicYear = e.AcademicYear,
                Program = e.Program
            })
            .ToListAsync();
    }

    public async Task<ExamRegistration?> GetExamRegistrationByIdAsync(int id)
    {
        return await context.ExamRegistrations
            .AsNoTracking()
            .Include(e => e.ExamSchedule)
            .Include(e => e.College)
            .Include(e => e.ExamCenter)
            .Include(e => e.AcademicYear)
            .Include(e => e.Program)
            .Include(e => e.ApplicationVoucher)
            .Include(e => e.ExamSubjectResults)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task CreateExamRegistrationAsync(ExamRegistration examRegistration)
    {
        context.ExamRegistrations.Add(examRegistration);
        await context.SaveChangesAsync();
    }

    public async Task UpdateExamRegistrationAsync(ExamRegistration examRegistration)
    {
        var existing = await context.ExamRegistrations.FindAsync(examRegistration.Id);
        if (existing != null)
        {
            examRegistration.TenantId = existing.TenantId;
            context.Entry(existing).CurrentValues.SetValues(examRegistration);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteExamRegistrationAsync(int id)
    {
        var examRegistration = await context.ExamRegistrations.FindAsync(id);
        if (examRegistration != null)
        {
            examRegistration.IsActive = false;
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExamRegistrationExistsAsync(int id)
    {
        return await context.ExamRegistrations.AnyAsync(e => e.Id == id);
    }

    public async Task VerifyExamRegistrationAsync(int id)
    {
        var examRegistration = await context.ExamRegistrations.FindAsync(id);
        if (examRegistration != null && examRegistration.Status == RegistrationStatus.Pending)
        {
            examRegistration.Status = RegistrationStatus.CollegeVerified;
            examRegistration.VerifiedByUsername = null;
            examRegistration.VerifiedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public async Task ApproveExamRegistrationAsync(int id)
    {
        var examRegistration = await context.ExamRegistrations.FindAsync(id);
        if (examRegistration != null && examRegistration.Status == RegistrationStatus.CollegeVerified)
        {
            examRegistration.Status = RegistrationStatus.AdminVerified;
            examRegistration.AdminVerifiedByUsername = null;
            examRegistration.AdminVerifiedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public ExamRegistrationSelectListsDto GetSelectListData(ExamRegistration? examRegistration = null, int? collegeId = null, int? facultyId = null)
    {
        var examSchedulesQuery = context.ExamSchedules.AsNoTracking();
        if (facultyId.HasValue)
            examSchedulesQuery = examSchedulesQuery.Where(es => es.Program != null && es.Program.Department != null && es.Program.Department.FacultyId == facultyId.Value);
        var examSchedules = examSchedulesQuery.ToList();

        var collegesQuery = context.Colleges.AsNoTracking();
        if (collegeId.HasValue)
            collegesQuery = collegesQuery.Where(c => c.Id == collegeId.Value);
        var colleges = collegesQuery.ToList();

        var academicYears = context.AcademicYears.AsNoTracking().ToList();

        var programsQuery = context.Programs.AsNoTracking();
        if (collegeId.HasValue)
        {
            var collegeProgramIds = context.CollegePrograms.AsNoTracking()
                .Where(cp => cp.CollegeId == collegeId.Value)
                .Select(cp => cp.ProgramId)
                .ToList();
            programsQuery = programsQuery.Where(p => collegeProgramIds.Contains(p.Id));
        }
        if (facultyId.HasValue)
            programsQuery = programsQuery.Where(p => p.Department != null && p.Department.FacultyId == facultyId.Value);
        var programs = programsQuery.ToList();

        var examCentersQuery = context.ExamCenters.AsNoTracking();
        if (collegeId.HasValue)
        {
            var examCenterIds = context.ExamCenterVenues.AsNoTracking()
                .Where(ecv => ecv.CollegeId == collegeId.Value)
                .Select(ecv => ecv.ExamCenterId)
                .Distinct()
                .ToList();
            examCentersQuery = examCentersQuery.Where(ec => examCenterIds.Contains(ec.Id));
        }
        var examCenters = examCentersQuery.ToList();

        return new ExamRegistrationSelectListsDto
        {
            ExamSchedules = examSchedules.Select(es => new SelectOption { Id = es.Id, Name = es.ExamScheduleName }).ToList(),
            Colleges = colleges.Select(c => new SelectOption { Id = c.Id, Name = c.Name }).ToList(),
            AcademicYears = academicYears.Select(ay => new SelectOption { Id = ay.Id, Name = ay.AcademicYearName }).ToList(),
            Programs = programs.Select(p => new SelectOption { Id = p.Id, Name = p.ProgramName }).ToList(),
            ExamCenters = examCenters.Select(ec => new SelectOption { Id = ec.Id, Name = $"Center {ec.Code}" }).ToList()
        };
    }

    private IQueryable<ExamRegistration> BuildQuery(string? search, string sort, string sortDir, int? examScheduleId = null)
    {
        var query = context.ExamRegistrations.AsNoTracking();

        if (examScheduleId.HasValue)
            query = query.Where(e => e.ExamScheduleId == examScheduleId.Value);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e =>
                (e.ExamRollNumber != null && e.ExamRollNumber.Contains(search)) ||
                (e.Remarks != null && e.Remarks.Contains(search)) ||
                (e.Sgpa != null && e.Sgpa.Contains(search)) ||
                (e.ExamSchedule != null && e.ExamSchedule.ExamScheduleName != null && e.ExamSchedule.ExamScheduleName.Contains(search)) ||
                (e.College != null && e.College.Name != null && e.College.Name.Contains(search)));
        }

        var descending = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sort.ToLower() switch
        {
            "rollnumber" => descending ? query.OrderByDescending(e => e.ExamRollNumber) : query.OrderBy(e => e.ExamRollNumber),
            "schedule" => descending
                ? query.OrderByDescending(e => e.ExamSchedule != null ? e.ExamSchedule.ExamScheduleName : string.Empty)
                : query.OrderBy(e => e.ExamSchedule != null ? e.ExamSchedule.ExamScheduleName : string.Empty),
            "college" => descending
                ? query.OrderByDescending(e => e.College != null ? e.College.Name : string.Empty)
                : query.OrderBy(e => e.College != null ? e.College.Name : string.Empty),
            "status" => descending ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
            _ => descending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id)
        };
    }
}
