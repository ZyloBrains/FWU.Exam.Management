using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
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

    public async Task<ExamSubjectResultSelectListsDto> GetSelectListDataAsync(ExamSubjectResult? examSubjectResult = null)
    {
        var examRegistrationsQuery = context.ExamRegistrations.IgnoreQueryFilters()
            .AsNoTracking()
            .Include(er => er.ApplicationVoucher)
            .AsQueryable();
        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
                examRegistrationsQuery = examRegistrationsQuery.Where(er => er.CollegeId == userContext.CollegeId.Value);
            else if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
                examRegistrationsQuery = examRegistrationsQuery.Where(er => er.ExamSchedule != null && er.ExamSchedule.Program != null && er.ExamSchedule.Program.FacultyId == userContext.FacultyId.Value);
        }
        var examRegistrations = await examRegistrationsQuery.ToListAsync();

        var erIds = examRegistrations.Select(er => er.Id).ToList();
        var studentNames = await GetStudentNamesForExamRegistrationsAsync(erIds);

        var subjectOfferings = await context.SubjectOfferings.AsNoTracking().ToListAsync();
        var examTypes = await context.ExamTypes.AsNoTracking().ToListAsync();

        var examSchedulesQuery = context.ExamSchedules.IgnoreQueryFilters().AsNoTracking();
        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
            {
                var collegeProgramIds = await context.CollegePrograms.IgnoreQueryFilters().AsNoTracking()
                    .Where(cp => cp.CollegeId == userContext.CollegeId.Value)
                    .Select(cp => cp.ProgramId)
                    .ToListAsync();
                examSchedulesQuery = examSchedulesQuery.Where(es => es.Program != null && collegeProgramIds.Contains(es.Program.Id));
            }
            else if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
                examSchedulesQuery = examSchedulesQuery.Where(es => es.Program != null && es.Program.FacultyId == userContext.FacultyId.Value);
        }
        var examSchedules = await examSchedulesQuery.ToListAsync();

        return new ExamSubjectResultSelectListsDto
        {
            ExamRegistrations = examRegistrations.Select(er =>
            {
                studentNames.TryGetValue(er.Id, out var name);
                if (string.IsNullOrEmpty(name))
                    name = er.ApplicationVoucher?.StudentName;
                var label = !string.IsNullOrEmpty(er.SymbolNumber) ? er.SymbolNumber : $"Reg #{er.Id}";
                if (!string.IsNullOrEmpty(name)) label += $" - {name}";
                return new SelectOption { Id = er.Id, Name = label };
            }).ToList(),
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
                query = query.Where(e => e.SubjectOffering != null && e.SubjectOffering.Program != null && e.SubjectOffering.Program.FacultyId == userContext.FacultyId.Value);
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

    public async Task<(List<ExamRegistrationGroupedDto> Items, int TotalCount)> GetRegistrationsWithSubjectResultsAsync(int page, int pageSize, string? search, int? examScheduleId = null)
    {
        var registrationsQuery = context.ExamRegistrations.IgnoreQueryFilters()
            .AsNoTracking()
            .Include(er => er.ExamSchedule)
            .Include(er => er.College)
            .Include(er => er.AcademicYear)
            .Include(er => er.Program)
            .Include(er => er.ApplicationVoucher)
            .Include(er => er.ExamSubjectResults!)
                .ThenInclude(sr => sr.SubjectOffering!)
                    .ThenInclude(so => so!.SubjectCatalog)
            .Include(er => er.ExamSubjectResults!)
                .ThenInclude(sr => sr.ExamType)
            .AsQueryable();

        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
                registrationsQuery = registrationsQuery.Where(er => er.CollegeId == userContext.CollegeId.Value);
            else if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
                registrationsQuery = registrationsQuery.Where(er => er.Program != null && er.Program.FacultyId == userContext.FacultyId.Value);
        }

        if (examScheduleId.HasValue)
            registrationsQuery = registrationsQuery.Where(er => er.ExamScheduleId == examScheduleId.Value);

        if (!string.IsNullOrEmpty(search))
        {
            registrationsQuery = registrationsQuery.Where(er =>
                (er.ExamRollNumber != null && er.ExamRollNumber.Contains(search)) ||
                (er.SymbolNumber != null && er.SymbolNumber.Contains(search)) ||
                (er.ExamSchedule != null && er.ExamSchedule.ExamScheduleName != null && er.ExamSchedule.ExamScheduleName.Contains(search)) ||
                (er.College != null && er.College.Name != null && er.College.Name.Contains(search)) ||
                (er.Program != null && er.Program.ProgramName != null && er.Program.ProgramName.Contains(search)));
        }

        registrationsQuery = registrationsQuery.OrderByDescending(er => er.Id);

        var totalCount = await registrationsQuery.CountAsync();
        var registrations = await registrationsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var erIds = registrations.Select(r => r.Id).ToList();
        var studentNames = await GetStudentNamesForExamRegistrationsAsync(erIds);

        var items = registrations.Select(er =>
        {
            studentNames.TryGetValue(er.Id, out var name);
            if (string.IsNullOrEmpty(name))
                name = er.ApplicationVoucher?.StudentName;
            return new ExamRegistrationGroupedDto
            {
                Id = er.Id,
                ExamScheduleId = er.ExamScheduleId,
                ExamScheduleName = er.ExamSchedule?.ExamScheduleName,
                CollegeId = er.CollegeId,
                CollegeName = er.College?.Name,
                AcademicYearId = er.AcademicYearId,
                AcademicYearName = er.AcademicYear?.AcademicYearName,
                ProgramsId = er.ProgramsId,
                ProgramName = er.Program?.ProgramName,
                ExamRollNumber = er.ExamRollNumber,
                SymbolNumber = er.SymbolNumber,
                StudentName = name,
                FeeEnclosed = er.FeeEnclosed,
                Status = er.Status,
                RegistrationDate = er.RegistrationDate,
                IsActive = er.IsActive,
                SubjectResults = er.ExamSubjectResults?.OrderBy(sr => sr.SubjectOffering?.SubjectCatalog?.SubjectCode).ToList() ?? []
            };
        }).ToList();

        return (items, totalCount);
    }

    private async Task<Dictionary<int, string>> GetStudentNamesForExamRegistrationsAsync(List<int> examRegistrationIds)
    {
        var names = new Dictionary<int, string>();

        var semEnrollments = await context.Set<SemesterEnrollment>()
            .AsNoTracking()
            .Include(se => se.StudentAdmission)
            .Include(se => se.ExamRegistrations)
            .Where(se => se.ExamRegistrations!.Any(er => examRegistrationIds.Contains(er.Id)))
            .ToListAsync();

        var userIds = semEnrollments
            .Select(se => se.StudentAdmission?.AppUserId)
            .Where(id => id != null)
            .Distinct()
            .Cast<string>()
            .ToList();

        if (userIds.Count > 0)
        {
            var userNames = await context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, Name = u.FullName ?? u.Email ?? "" })
                .ToDictionaryAsync(u => u.Id, u => u.Name);

            foreach (var se in semEnrollments)
            {
                if (se.ExamRegistrations == null) continue;
                var appUserId = se.StudentAdmission?.AppUserId;
                var name = appUserId != null && userNames.TryGetValue(appUserId, out var n) ? n : "";
                foreach (var er in se.ExamRegistrations.Where(er => examRegistrationIds.Contains(er.Id)))
                {
                    if (!string.IsNullOrEmpty(name))
                        names[er.Id] = name;
                }
            }
        }

        return names;
    }
}
