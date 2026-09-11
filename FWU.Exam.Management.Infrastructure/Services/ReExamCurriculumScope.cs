using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Exams;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

/// <summary>
/// Resolves which curriculum versions participate in a schedule's marks-entry
/// subject list. Regular schedules map to the single version effective for the
/// schedule's academic year; re-exam schedules may mix cohorts, so they surface
/// every version actually ticked in the schedule (plus unversioned offerings).
/// </summary>
public static class ReExamCurriculumScope
{
    public static bool IsReExamSchedule(ExamSchedule schedule)
        => StudentDashboardService.IsReExamTypeStatic(schedule.ExamType?.Name);

    /// <summary>
    /// Allowed curriculum version ids for a schedule's subject list. Re-exam
    /// schedules resolve to the versions ticked by this schedule's students
    /// (falling back to the instance-resolved version when nothing is ticked
    /// yet); regular schedules resolve the single instance version.
    /// </summary>
    public static async Task<List<int?>> ResolveVersionIdsAsync(
        AppDbContext context, ExamSchedule schedule, int semesterNumber, CancellationToken cancellationToken = default)
    {
        if (!IsReExamSchedule(schedule))
        {
            var resolved = await CurriculumVersionResolver.ResolveAsync(
                context, schedule.ProgramId, schedule.SemesterInstance!.AcademicYearId);
            return resolved.HasValue ? [resolved.Value] : [];
        }

        var ticked = await (
            from esr in context.ExamSubjectResults
            join so in context.SubjectOfferings on esr.SubjectOfferingId equals so.Id
            where esr.ExamScheduleId == schedule.Id && esr.IsActive
            select (int?)so.CurriculumVersionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (ticked.Count == 0)
        {
            var resolved = await CurriculumVersionResolver.ResolveAsync(
                context, schedule.ProgramId, schedule.SemesterInstance!.AcademicYearId);
            return resolved.HasValue ? [resolved.Value] : [];
        }

        return ticked;
    }

    public static async Task<Dictionary<int, string>> GetVersionNamesAsync(
        AppDbContext context, int programId, CancellationToken cancellationToken = default)
    {
        return await context.CurriculumVersions!
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(cv => cv.ProgramId == programId)
            .ToDictionaryAsync(cv => cv.Id, cv => cv.Name ?? $"Version {cv.Id}", cancellationToken);
    }

    /// <summary>
    /// Appends the curriculum version name to subject names when a schedule's
    /// subject list spans more than one version group, so the admin can tell
    /// cross-cohort duplicates apart.
    /// </summary>
    public static void ApplyVersionLabels(List<SubjectOptionDto> rows, Dictionary<int, string> versionNames)
    {
        if (rows.Select(r => r.CurriculumVersionId).Distinct().Count() <= 1) return;

        foreach (var row in rows)
        {
            if (row.CurriculumVersionId is int versionId
                && versionNames.TryGetValue(versionId, out var versionName))
            {
                row.Name = $"{row.Name} ({versionName})";
            }
        }
    }
}