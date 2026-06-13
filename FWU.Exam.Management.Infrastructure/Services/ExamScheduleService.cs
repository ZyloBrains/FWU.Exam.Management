using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ExamScheduleService(AppDbContext context) : IExamScheduleService
{
    private IQueryable<ExamSchedule> ApplyScope(IQueryable<ExamSchedule> query, int? collegeId, int? facultyId)
    {
        if (collegeId.HasValue)
        {
            var programIds = context.CollegePrograms!
                .Where(cp => cp.CollegeId == collegeId.Value)
                .Select(cp => cp.ProgramId)
                .Distinct()
                .ToList();

            return query.Where(e => programIds.Contains(e.ProgramId));
        }

        if (facultyId.HasValue)
        {
            var collegeIds = context.Colleges
                .Where(c => c.Faculties.Any(f => f.Id == facultyId.Value))
                .Select(c => c.Id)
                .ToList();

            var programIds = context.CollegePrograms!
                .Where(cp => collegeIds.Contains(cp.CollegeId))
                .Select(cp => cp.ProgramId)
                .Distinct()
                .ToList();

            return query.Where(e => programIds.Contains(e.ProgramId));
        }

        return query;
    }

    public async Task<(List<ExamSchedule> Items, int TotalCount)> GetExamSchedulesAsync(int page, int pageSize, string? search, string sort, string sortDir, int? collegeId = null, int? facultyId = null)
    {
        var query = ApplyScope(BuildQuery(search, sort, sortDir), collegeId, facultyId);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExamSchedule
            {
                Id = e.Id,
                AcademicYearId = e.AcademicYearId,
                ProgramId = e.ProgramId,
                ExamTypeId = e.ExamTypeId,
                ExamScheduleName = e.ExamScheduleName,
                StartDateBs = e.StartDateBs,
                EndDateBs = e.EndDateBs,
                PublishedDate = e.PublishedDate,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Remarks = e.Remarks,
                IsActive = e.IsActive,
                ExtendedDate = e.ExtendedDate,
                ExtendedDateCharge = e.ExtendedDateCharge,
                CollegeApprovalDate = e.CollegeApprovalDate,
                AdmissionCardReleaseDate = e.AdmissionCardReleaseDate,
                ExamScheduleCode = e.ExamScheduleCode,
                AcademicYear = e.AcademicYear,
                Program = e.Program,
                ExamType = e.ExamType
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<ExamSchedule>> GetFilteredItemsAsync(string? search, int? collegeId = null, int? facultyId = null)
    {
        var query = ApplyScope(BuildQuery(search, "Id", "asc"), collegeId, facultyId);
        return await query
            .Select(e => new ExamSchedule
            {
                Id = e.Id,
                AcademicYearId = e.AcademicYearId,
                ProgramId = e.ProgramId,
                ExamTypeId = e.ExamTypeId,
                ExamScheduleName = e.ExamScheduleName,
                StartDateBs = e.StartDateBs,
                EndDateBs = e.EndDateBs,
                PublishedDate = e.PublishedDate,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Remarks = e.Remarks,
                IsActive = e.IsActive,
                ExtendedDate = e.ExtendedDate,
                ExtendedDateCharge = e.ExtendedDateCharge,
                CollegeApprovalDate = e.CollegeApprovalDate,
                AdmissionCardReleaseDate = e.AdmissionCardReleaseDate,
                ExamScheduleCode = e.ExamScheduleCode,
                AcademicYear = e.AcademicYear,
                Program = e.Program,
                ExamType = e.ExamType
            })
            .ToListAsync();
    }

    public async Task<ExamSchedule?> GetExamScheduleByIdAsync(int id)
    {
        return await context.ExamSchedules
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new ExamSchedule
            {
                Id = e.Id,
                AcademicYearId = e.AcademicYearId,
                ProgramId = e.ProgramId,
                ExamTypeId = e.ExamTypeId,
                ExamScheduleName = e.ExamScheduleName,
                StartDateBs = e.StartDateBs,
                EndDateBs = e.EndDateBs,
                PublishedDate = e.PublishedDate,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Remarks = e.Remarks,
                IsActive = e.IsActive,
                ExtendedDate = e.ExtendedDate,
                ExtendedDateCharge = e.ExtendedDateCharge,
                CollegeApprovalDate = e.CollegeApprovalDate,
                AdmissionCardReleaseDate = e.AdmissionCardReleaseDate,
                ExamScheduleCode = e.ExamScheduleCode,
                AcademicYear = e.AcademicYear,
                Program = e.Program,
                ExamType = e.ExamType
            })
            .FirstOrDefaultAsync();
    }

    public async Task CreateExamScheduleAsync(ExamSchedule examSchedule)
    {
        context.ExamSchedules.Add(examSchedule);
        await context.SaveChangesAsync();
    }

    public async Task UpdateExamScheduleAsync(ExamSchedule examSchedule)
    {
        context.ExamSchedules.Update(examSchedule);
        await context.SaveChangesAsync();
    }

    public async Task DeleteExamScheduleAsync(int id)
    {
        var examSchedule = await context.ExamSchedules.FindAsync(id);
        if (examSchedule != null)
        {
            context.ExamSchedules.Remove(examSchedule);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExamScheduleExistsAsync(int id)
    {
        return await context.ExamSchedules.AnyAsync(e => e.Id == id);
    }

    public ExamScheduleSelectListsDto GetSelectListData(ExamSchedule? examSchedule = null)
    {
        var academicYears = context.AcademicYears.AsNoTracking().ToList();
        var examTypes = context.ExamTypes.AsNoTracking().ToList();
        var programs = context.Programs.AsNoTracking().ToList();
        var semesters = context.Semesters.AsNoTracking().ToList();

        return new ExamScheduleSelectListsDto
        {
            AcademicYears = academicYears.Select(ay => new SelectOption { Id = ay.Id, Name = ay.AcademicYearName }).ToList(),
            ExamTypes = examTypes.Select(et => new SelectOption { Id = et.Id, Name = et.Name }).ToList(),
            Programs = programs.Select(p => new SelectOption { Id = p.Id, Name = p.ProgramName }).ToList(),
            Semesters = semesters.Select(s => new SelectOption { Id = s.Id, Name = s.Name }).ToList()
        };
    }

    private IQueryable<ExamSchedule> BuildQuery(string? search, string sort, string sortDir)
    {
        var query = context.ExamSchedules.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                (s.ExamScheduleName != null && s.ExamScheduleName.Contains(search)) ||
                (s.ExamScheduleCode != null && s.ExamScheduleCode.Contains(search)) ||
                (s.Remarks != null && s.Remarks.Contains(search)) ||
                (s.AcademicYear != null && s.AcademicYear.AcademicYearName != null && s.AcademicYear.AcademicYearName.Contains(search)) ||
                (s.Program != null && s.Program.ProgramName != null && s.Program.ProgramName.Contains(search)) ||
                (s.ExamType != null && s.ExamType.Name != null && s.ExamType.Name.Contains(search)));
        }

        var descending = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sort.ToLower() switch
        {
            "name" => descending ? query.OrderByDescending(e => e.ExamScheduleName) : query.OrderBy(e => e.ExamScheduleName),
            "code" => descending ? query.OrderByDescending(e => e.ExamScheduleCode) : query.OrderBy(e => e.ExamScheduleCode),
            "academicyear" => descending
                ? query.OrderByDescending(e => e.AcademicYear != null ? e.AcademicYear.AcademicYearName : string.Empty)
                : query.OrderBy(e => e.AcademicYear != null ? e.AcademicYear.AcademicYearName : string.Empty),
            "level" => descending
                ? query.OrderByDescending(e => e.Program != null ? e.Program.ProgramName : string.Empty)
                : query.OrderBy(e => e.Program != null ? e.Program.ProgramName : string.Empty),
            "examtype" => descending
                ? query.OrderByDescending(e => e.ExamType != null ? e.ExamType.Name : string.Empty)
                : query.OrderBy(e => e.ExamType != null ? e.ExamType.Name : string.Empty),
            _ => descending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id)
        };
    }
}
