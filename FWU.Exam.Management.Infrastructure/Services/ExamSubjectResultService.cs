using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ExamSubjectResultService(AppDbContext context, IUserContext userContext) : IExamSubjectResultService
{
    public async Task<(List<ExamSubjectResult> Items, int TotalCount)> GetExamSubjectResultsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? examScheduleId = null, int? examRegistrationId = null)
    {
        var query = BuildQuery(search, sort, sortDir, examScheduleId, examRegistrationId);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExamSubjectResult
            {
                Id = e.Id,
                ExamRegistrationId = e.ExamRegistrationId,
                ExamTypeId = e.ExamTypeId,
                SubjectOfferingId = e.SubjectOfferingId,
                ExamScheduleId = e.ExamScheduleId,
                ObtainedMarksTheory = e.ObtainedMarksTheory,
                ObtainedMarksTheoryConfirm = e.ObtainedMarksTheoryConfirm,
                ObtainedMarksPractical = e.ObtainedMarksPractical,
                ObtainedMarksPracticalConfirm = e.ObtainedMarksPracticalConfirm,
                ObtainedMarksTheoryInternal = e.ObtainedMarksTheoryInternal,
                ObtainedMarksPracticalInternal = e.ObtainedMarksPracticalInternal,
                GradeLetter = e.GradeLetter,
                Remarks = e.Remarks,
                IsActive = e.IsActive,
                IsSubmitted = e.IsSubmitted,
                ObtainedMarks = e.ObtainedMarks,
                ExamRegistration = e.ExamRegistration,
                ExamType = e.ExamType,
                SubjectOffering = e.SubjectOffering,
                ExamSchedule = e.ExamSchedule
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<ExamSubjectResult>> GetFilteredItemsAsync(string? search, int? examScheduleId = null)
    {
        var query = BuildQuery(search, "Id", "asc", examScheduleId, null);
        return await query
            .Select(e => new ExamSubjectResult
            {
                Id = e.Id,
                ExamRegistrationId = e.ExamRegistrationId,
                ExamTypeId = e.ExamTypeId,
                SubjectOfferingId = e.SubjectOfferingId,
                ExamScheduleId = e.ExamScheduleId,
                ObtainedMarksTheory = e.ObtainedMarksTheory,
                GradeLetter = e.GradeLetter,
                IsSubmitted = e.IsSubmitted,
                ExamRegistration = e.ExamRegistration,
                ExamType = e.ExamType,
                SubjectOffering = e.SubjectOffering
            })
            .ToListAsync();
    }

    public async Task<ExamSubjectResult?> GetExamSubjectResultByIdAsync(int id)
    {
        return await context.ExamSubjectResults
            .AsNoTracking()
            .Include(e => e.ExamRegistration)
            .Include(e => e.ExamType)
            .Include(e => e.SubjectOffering)
            .Include(e => e.ExamSchedule)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task CreateExamSubjectResultAsync(ExamSubjectResult examSubjectResult)
    {
        context.ExamSubjectResults.Add(examSubjectResult);
        await context.SaveChangesAsync();
    }

    public async Task UpdateExamSubjectResultAsync(ExamSubjectResult examSubjectResult)
    {
        var existing = await context.ExamSubjectResults.FindAsync(examSubjectResult.Id);
        if (existing != null)
        {
            examSubjectResult.TenantId = existing.TenantId;
            context.Entry(existing).CurrentValues.SetValues(examSubjectResult);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteExamSubjectResultAsync(int id)
    {
        var examSubjectResult = await context.ExamSubjectResults.FindAsync(id);
        if (examSubjectResult != null)
        {
            context.ExamSubjectResults.Remove(examSubjectResult);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExamSubjectResultExistsAsync(int id)
    {
        return await context.ExamSubjectResults.AnyAsync(e => e.Id == id);
    }

    public ExamSubjectResultSelectListsDto GetSelectListData(ExamSubjectResult? examSubjectResult = null)
    {
        var examRegistrationsQuery = context.ExamRegistrations.AsNoTracking();
        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
                examRegistrationsQuery = examRegistrationsQuery.Where(er => er.CollegeId == userContext.CollegeId.Value);
            else if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
                examRegistrationsQuery = examRegistrationsQuery.Where(er => er.ExamSchedule != null && er.ExamSchedule.Program != null && er.ExamSchedule.Program.CollegePrograms!.Any(cp => cp.College != null && cp.College.Faculties!.Any(f => f.Id == userContext.FacultyId.Value)));
        }
        var examRegistrations = examRegistrationsQuery.ToList();

        var subjectOfferings = context.SubjectOfferings.AsNoTracking().ToList();
        var examTypes = context.ExamTypes.AsNoTracking().ToList();

        var examSchedulesQuery = context.ExamSchedules.AsNoTracking();
        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
            {
                var collegeProgramIds = context.CollegePrograms.AsNoTracking()
                    .Where(cp => cp.CollegeId == userContext.CollegeId.Value)
                    .Select(cp => cp.ProgramId)
                    .ToList();
                examSchedulesQuery = examSchedulesQuery.Where(es => es.Program != null && collegeProgramIds.Contains(es.Program.Id));
            }
            else if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
                examSchedulesQuery = examSchedulesQuery.Where(es => es.Program != null && es.Program.CollegePrograms!.Any(cp => cp.College != null && cp.College.Faculties!.Any(f => f.Id == userContext.FacultyId.Value)));
        }
        var examSchedules = examSchedulesQuery.ToList();

        return new ExamSubjectResultSelectListsDto
        {
            ExamRegistrations = examRegistrations.Select(er => new SelectOption { Id = er.Id, Name = $"Reg #{er.Id}" }).ToList(),
            SubjectOfferings = subjectOfferings.Select(so => new SelectOption { Id = so.Id, Name = so.SubjectCatalog?.SubjectName ?? $"Offering #{so.Id}" }).ToList(),
            ExamTypes = examTypes.Select(et => new SelectOption { Id = et.Id, Name = et.Name }).ToList(),
            ExamSchedules = examSchedules.Select(es => new SelectOption { Id = es.Id, Name = es.ExamScheduleName }).ToList()
        };
    }

    private IQueryable<ExamSubjectResult> BuildQuery(string? search, string sort, string sortDir, int? examScheduleId = null, int? examRegistrationId = null)
    {
        var query = context.ExamSubjectResults.AsNoTracking();

        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
                query = query.Where(e => e.SubjectOffering != null && e.SubjectOffering.Program != null && e.SubjectOffering.Program.CollegePrograms!.Any(cp => cp.College != null && cp.College.Faculties!.Any(f => f.Id == userContext.FacultyId.Value)));
            else if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
                query = query.Where(e => e.ExamRegistration != null && e.ExamRegistration.CollegeId == userContext.CollegeId.Value);
        }

        if (examScheduleId.HasValue)
            query = query.Where(e => e.ExamScheduleId == examScheduleId.Value);

        if (examRegistrationId.HasValue)
            query = query.Where(e => e.ExamRegistrationId == examRegistrationId.Value);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e =>
                (e.GradeLetter != null && e.GradeLetter.Contains(search)) ||
                (e.Remarks != null && e.Remarks.Contains(search)) ||
                (e.SubjectOffering != null && e.SubjectOffering.SubjectCatalog != null && e.SubjectOffering.SubjectCatalog.SubjectName != null && e.SubjectOffering.SubjectCatalog.SubjectName.Contains(search)));
        }

        var descending = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sort.ToLower() switch
        {
            "grade" => descending ? query.OrderByDescending(e => e.GradeLetter) : query.OrderBy(e => e.GradeLetter),
            "subject" => descending
                ? query.OrderByDescending(e => e.SubjectOffering != null ? e.SubjectOffering.Id : 0)
                : query.OrderBy(e => e.SubjectOffering != null ? e.SubjectOffering.Id : 0),
            "issubmitted" => descending ? query.OrderByDescending(e => e.IsSubmitted) : query.OrderBy(e => e.IsSubmitted),
            _ => descending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id)
        };
    }
}
