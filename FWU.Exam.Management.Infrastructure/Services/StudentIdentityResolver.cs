using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public sealed class StudentIdentityItem
{
    public string StudentName { get; init; } = string.Empty;
    public string RegistrationNumber { get; init; } = string.Empty;
    public string AcademicYearName { get; init; } = string.Empty;
}

/// <summary>
/// Resolves display identity (name, registration number, batch academic year)
/// for exam registrations. Old-cohort (partial/re-exam) registrations often
/// lack either the voucher link or a legacy registration number, so the lookup
/// walks every available chain and keeps the first non-empty value.
/// </summary>
public static class StudentIdentityResolver
{
    public static async Task<Dictionary<int, StudentIdentityItem>> ResolveAsync(
        AppDbContext context, List<int> examRegistrationIds)
    {
        var result = new Dictionary<int, StudentIdentityItem>();
        if (examRegistrationIds.Count == 0) return result;

        var registrations = await context.ExamRegistrations
            .AsNoTracking()
            .Include(er => er.AcademicYear)
            .Include(er => er.ApplicationVoucher)
                .ThenInclude(av => av!.StudentRegistration)
                    .ThenInclude(sr => sr!.AcademicYear)
            .Include(er => er.SemesterEnrollment)
                .ThenInclude(se => se!.StudentAdmission)
                    .ThenInclude(sa => sa!.StudentRegistration)
                        .ThenInclude(sr => sr!.AcademicYear)
            .Include(er => er.SemesterEnrollment)
                .ThenInclude(se => se!.StudentAdmission)
                    .ThenInclude(sa => sa!.AcademicYear)
            .Where(er => examRegistrationIds.Contains(er.Id))
            .ToListAsync();

        var userNames = await ResolveUserNamesAsync(context, registrations);
        var admissions = registrations
            .Where(er => er.SemesterEnrollment?.StudentAdmission != null)
            .Select(er => er.SemesterEnrollment!.StudentAdmission!)
            .DistinctBy(a => a.Id)
            .ToList();

        foreach (var er in registrations)
        {
            var admission = er.SemesterEnrollment?.StudentAdmission;
            var voucherSr = er.ApplicationVoucher?.StudentRegistration;
            var admissionSr = admission?.StudentRegistration;

            result[er.Id] = new StudentIdentityItem
            {
                StudentName = ResolveName(er, admission, admissions, userNames),
                RegistrationNumber = NonEmpty(voucherSr?.RegistrationNumber)
                                   ?? NonEmpty(admissionSr?.RegistrationNumber)
                                   ?? "",
                AcademicYearName = NonEmpty(voucherSr?.AcademicYear?.AcademicYearName)
                                 ?? NonEmpty(admissionSr?.AcademicYear?.AcademicYearName)
                                 ?? NonEmpty(admission?.AcademicYear?.AcademicYearName)
                                 ?? NonEmpty(er.AcademicYear?.AcademicYearName)
                                 ?? ""
            };
        }

        return result;
    }

    private static async Task<Dictionary<string, string>> ResolveUserNamesAsync(
        AppDbContext context, IReadOnlyCollection<ExamRegistration> registrations)
    {
        var userIds = registrations
            .Where(er => er.SemesterEnrollment?.StudentAdmission?.AppUserId != null)
            .Select(er => er.SemesterEnrollment!.StudentAdmission!.AppUserId)
            .Where(id => id != null)
            .Distinct()
            .Cast<string>()
            .ToList();

        if (userIds.Count == 0) return new Dictionary<string, string>();

        return await context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, Name = u.FullName ?? u.Email ?? "" })
            .ToDictionaryAsync(u => u.Id, u => u.Name);
    }

    private static string ResolveName(
        ExamRegistration er,
        StudentAdmission? admission,
        IReadOnlyCollection<StudentAdmission> admissions,
        Dictionary<string, string> userNames)
    {
        if (admission != null)
        {
            if (admission.AppUserId != null && userNames.TryGetValue(admission.AppUserId, out var userName))
                return userName;

            var fullName = admission.FirstName.GetFullName(admission.MiddleName, admission.LastName);
            if (!string.IsNullOrWhiteSpace(fullName))
                return fullName;
        }

        return er.ApplicationVoucher?.StudentName ?? "";
    }

    private static string? NonEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}