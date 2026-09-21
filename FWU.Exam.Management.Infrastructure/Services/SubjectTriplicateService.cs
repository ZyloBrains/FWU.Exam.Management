using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class SubjectTriplicateService(AppDbContext context, IUserContext userContext) : ISubjectTriplicateService
{
    public async Task<SubjectTriplicateReportDto?> BuildAsync(
        int? examScheduleId,
        int? collegeId,
        int? programId = null,
        int? semesterId = null,
        int? examTypeId = null)
    {
        if (!examScheduleId.HasValue) return null;

        var schedule = await context.ExamSchedules
            .AsNoTracking()
            .Include(es => es.College)
            .Include(es => es.Program)
            .Include(es => es.Level)
            .Include(es => es.ExamType)
            .Include(es => es.SemesterInstance)
                .ThenInclude(si => si!.Semester)
            .Include(es => es.SemesterInstance)
                .ThenInclude(si => si!.AcademicYear)
            .ApplyScope(userContext)
            .FirstOrDefaultAsync(es => es.Id == examScheduleId.Value);

        if (schedule == null) return null;

        var registrations = await LoadEligibleAsync(examScheduleId.Value, collegeId, programId);

        if (registrations.Count == 0) return null;

        var identityIds = registrations.Select(er => er.Id).ToList();
        var identities = await StudentIdentityResolver.ResolveAsync(context, identityIds);

        var groups = registrations
            .GroupBy(er => er.CollegeId)
            .Select(grp => new SubjectTriplicateCollegeGroupDto
            {
                CollegeId = grp.Key,
                CollegeName = grp.First().College?.Name,
                Students = grp.Select(er => BuildStudent(er, identities)).ToList()
            })
            .OrderBy(g => g.CollegeName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new SubjectTriplicateReportDto
        {
            ExamScheduleName = schedule.ExamScheduleName,
            ExamScheduleCode = schedule.ExamScheduleCode,
            AcademicYearName = schedule.SemesterInstance?.AcademicYear?.AcademicYearName,
            LevelName = schedule.Level?.LevelName,
            SemesterName = schedule.SemesterInstance?.Semester?.Name,
            ExamTypeName = schedule.ExamType?.Name,
            GeneratedDate = DateTime.UtcNow,
            Groups = groups
        };
    }

    private async Task<List<Domain.Entities.Exams.ExamRegistration>> LoadEligibleAsync(
        int examScheduleId,
        int? collegeId,
        int? programId)
    {
        var query = context.ExamRegistrations
            .Include(er => er.College)
            .Include(er => er.Program)
            .Include(er => er.ExamSubjectResults.Where(esr => esr.IsActive))
                .ThenInclude(esr => esr.SubjectOffering)
                    .ThenInclude(so => so!.SubjectCatalog)
            .Where(er => er.ExamScheduleId == examScheduleId
                      && er.IsActive
                      && er.Status >= RegistrationStatus.CollegeVerified)
            .AsNoTracking()
            .AsQueryable();

        if (collegeId.HasValue)
            query = query.Where(er => er.CollegeId == collegeId.Value);

        if (programId.HasValue)
            query = query.Where(er => er.ProgramsId == programId.Value);

        var list = await query.ToListAsync();

        return list
            .OrderBy(er => er.College?.Name ?? "", StringComparer.OrdinalIgnoreCase)
            .ThenBy(er => er.ProgramsId)
            .ThenBy(er => er.IsSupplementary)
            .ThenBy(er => er.SymbolNumber ?? "", StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static SubjectTriplicateStudentDto BuildStudent(
        Domain.Entities.Exams.ExamRegistration er,
        Dictionary<int, StudentIdentityItem> identities)
    {
        identities.TryGetValue(er.Id, out var identity);

        var subjects = er.ExamSubjectResults
            .Where(r => r.SubjectOffering?.SubjectCatalog != null)
            .OrderBy(r => r.SubjectOffering!.DisplayOrder)
            .ThenBy(r => r.Id)
            .Select(r => new SubjectTriplicateSubjectDto
            {
                SubjectCode = r.SubjectOffering!.SubjectCatalog!.SubjectCode,
                SubjectName = r.SubjectOffering.SubjectCatalog.SubjectName,
                HasTheory = r.SubjectOffering.HasTheory,
                HasPractical = r.SubjectOffering.HasPractical
            })
            .ToList();

        return new SubjectTriplicateStudentDto
        {
            SymbolNumber = er.SymbolNumber,
            RegistrationNumber = identity?.RegistrationNumber,
            StudentName = identity?.StudentName,
            ProgramName = er.Program?.ProgramName ?? er.Program?.ShortName,
            Subjects = subjects
        };
    }
}
