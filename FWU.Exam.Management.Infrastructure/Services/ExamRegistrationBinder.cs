using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

/// <summary>
/// Resolves the cohort (batch) context of exam registrations so marks entry can
/// present and grade the offering a partial/re-exam student actually registered
/// instead of the schedule year's curriculum. Partial forms belong to older
/// cohorts for which the batch academic year differs from the schedule year.
/// </summary>
public static class ExamRegistrationBinder
{
    public static bool IsReExamSchedule(ExamSchedule? schedule) =>
        schedule != null
        && schedule.ExamType != null
        && StudentDashboardService.IsReExamTypeStatic(schedule.ExamType.Name);

    /// <summary>
    /// Maps exam registration ids to their batch academic year id. Resolved from
    /// the payment voucher's student registration first (the authoritative link
    /// for online forms), then through semester enrollment → admission.
    /// </summary>
    public static async Task<Dictionary<int, int>> ResolveBatchAcademicYearIdsAsync(
        AppDbContext context, IReadOnlyCollection<int> examRegistrationIds)
    {
        var result = new Dictionary<int, int>();
        if (examRegistrationIds is not { Count: > 0 })
            return result;

        var ids = examRegistrationIds.Distinct().ToList();

        var voucherLinks = await context.ExamRegistrations
            .AsNoTracking()
            .Where(er => ids.Contains(er.Id) && er.ApplicationVoucherId != null)
            .Select(er => new { er.Id, er.ApplicationVoucherId })
            .ToListAsync();

        var voucherIds = voucherLinks
            .Where(v => v.ApplicationVoucherId.HasValue)
            .Select(v => v.ApplicationVoucherId!.Value)
            .Distinct()
            .ToList();

        if (voucherIds.Count > 0)
        {
            var srByVoucher = await context.ApplicationVouchers!
                .AsNoTracking()
                .Where(v => voucherIds.Contains(v.Id) && v.StudentRegistrationId != null)
                .Select(v => new { v.Id, v.StudentRegistrationId })
                .ToDictionaryAsync(v => v.Id, v => v.StudentRegistrationId!.Value);

            var srIds = srByVoucher.Values.Distinct().ToList();
            var yearBySr = srIds.Count > 0
                ? await context.StudentRegistrations!
                    .AsNoTracking()
                    .Where(sr => srIds.Contains(sr.Id) && sr.AcademicYearId > 0)
                    .ToDictionaryAsync(sr => sr.Id, sr => sr.AcademicYearId)
                : new Dictionary<int, int>();

            foreach (var link in voucherLinks)
            {
                if (link.ApplicationVoucherId.HasValue
                    && srByVoucher.TryGetValue(link.ApplicationVoucherId.Value, out var srId)
                    && yearBySr.TryGetValue(srId, out var yearId))
                {
                    result[link.Id] = yearId;
                }
            }
        }

        var remaining = ids.Where(id => !result.ContainsKey(id)).ToList();
        if (remaining.Count == 0)
            return result;

        var enrollments = await context.Set<SemesterEnrollment>()
            .AsNoTracking()
            .Include(se => se.StudentAdmission)
            .Include(se => se.ExamRegistrations)
            .Where(se => se.ExamRegistrations!.Any(er => remaining.Contains(er.Id)))
            .ToListAsync();

        var admissionIds = enrollments
            .Where(se => se.StudentAdmission != null)
            .Select(se => se.StudentAdmission!.Id)
            .Distinct()
            .ToList();

        var yearByAdmission = new Dictionary<int, int>();
        if (admissionIds.Count > 0)
        {
            yearByAdmission = await context.StudentRegistrations!
                .AsNoTracking()
                .Where(sr => sr.StudentAdmissionId != null
                          && admissionIds.Contains(sr.StudentAdmissionId!.Value)
                          && sr.AcademicYearId > 0)
                .ToDictionaryAsync(sr => sr.StudentAdmissionId!.Value, sr => sr.AcademicYearId);
        }

        foreach (var enrollment in enrollments)
        {
            if (enrollment.StudentAdmission == null || enrollment.ExamRegistrations == null) continue;
            if (!yearByAdmission.TryGetValue(enrollment.StudentAdmission.Id, out var yearId)) continue;

            foreach (var er in enrollment.ExamRegistrations.Where(er => remaining.Contains(er.Id)))
                result.TryAdd(er.Id, yearId);
        }

        return result;
    }

    /// <summary>
    /// Maps exam registration ids to their batch curriculum version id using the
    /// student's batch academic year (falls back to null when no version applies).
    /// </summary>
    public static async Task<Dictionary<int, int?>> ResolveBatchVersionIdsAsync(
        AppDbContext context, int programId, IReadOnlyDictionary<int, int> batchYears)
    {
        var result = new Dictionary<int, int?>();
        if (batchYears.Count == 0)
            return result;

        var versionByYear = new Dictionary<int, int>();
        foreach (var year in batchYears.Values.Distinct())
        {
            var versionId = await CurriculumVersionResolver.ResolveAsync(context, programId, year);
            if (versionId.HasValue)
                versionByYear[year] = versionId.Value;
        }

        foreach (var kvp in batchYears)
            result[kvp.Key] = versionByYear.TryGetValue(kvp.Value, out var v) ? v : null;

        return result;
    }

    /// <summary>
    /// Maps exam registration ids to the grading scheme id bound to their batch
    /// academic year (year-specific wins over a year-agnostic scheme).
    /// </summary>
    public static async Task<Dictionary<int, int?>> ResolveBatchSchemeIdsAsync(
        AppDbContext context, int programId, IReadOnlyDictionary<int, int> batchYears)
    {
        var result = new Dictionary<int, int?>();
        if (batchYears.Count == 0)
            return result;

        var years = batchYears.Values.Distinct().ToList();

        var rows = await context.GradingSchemePrograms
            .AsNoTracking()
            .Where(gsp => gsp.ProgramId == programId
                       && gsp.IsActive
                       && (gsp.AcademicYearId == null || gsp.AcademicYearId != null && years.Contains(gsp.AcademicYearId.Value)))
            .Select(gsp => new { gsp.AcademicYearId, gsp.GradingSchemeId })
            .ToListAsync();

        var schemeByYear = new Dictionary<int, int>();
        foreach (var year in years)
        {
            var schemeId = rows
                .Where(r => r.AcademicYearId == null || r.AcademicYearId == year)
                .OrderByDescending(r => r.AcademicYearId.HasValue)
                .ThenByDescending(r => r.GradingSchemeId)
                .Select(r => r.GradingSchemeId)
                .FirstOrDefault();
            if (schemeId > 0)
                schemeByYear[year] = schemeId;
        }

        foreach (var kvp in batchYears)
            result[kvp.Key] = schemeByYear.TryGetValue(kvp.Value, out var s) ? s : null;

        return result;
    }

    /// <summary>
    /// Distinct subject offerings that have an active marks row registered on the
    /// schedule. These may belong to older curriculum versions than the schedule
    /// resolves to, which is exactly why partial schedules need them surfaced.
    /// </summary>
    public static async Task<List<SubjectOffering>> GetRegisteredSubjectOfferingsAsync(
        AppDbContext context, int examScheduleId)
    {
        var offeringIds = await context.ExamSubjectResults
            .AsNoTracking()
            .Where(esr => esr.ExamScheduleId == examScheduleId && esr.IsActive)
            .Select(esr => esr.SubjectOfferingId)
            .Distinct()
            .ToListAsync();

        if (offeringIds.Count == 0)
            return [];

        return await context.SubjectOfferings
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .Include(so => so.CurriculumVersion)
                .ThenInclude(cv => cv!.EffectiveAcademicYear)
            .Include(so => so.Semester)
            .Where(so => offeringIds.Contains(so.Id))
            .OrderBy(so => so.DisplayOrder)
            .ThenBy(so => so.Id)
            .ToListAsync();
    }
}