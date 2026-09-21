using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

internal static class StudentNameResolver
{
    public static async Task<Dictionary<int, string>> ResolveAsync(
        AppDbContext context,
        List<int> examRegistrationIds)
    {
        var result = new Dictionary<int, string>();
        if (examRegistrationIds.Count == 0) return result;

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

        var userNames = new Dictionary<string, string>();
        if (userIds.Count > 0)
        {
            userNames = await context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, Name = u.FullName ?? u.Email ?? "" })
                .ToDictionaryAsync(u => u.Id, u => u.Name);
        }

        foreach (var se in semEnrollments)
        {
            if (se.ExamRegistrations == null) continue;
            var admission = se.StudentAdmission;
            var name = "";
            if (admission != null)
            {
                name = admission.FirstName.GetFullName(admission.MiddleName, admission.LastName);
                if (string.IsNullOrWhiteSpace(name) && admission.AppUserId != null
                    && userNames.TryGetValue(admission.AppUserId, out var n))
                    name = n;
            }
            foreach (var er in se.ExamRegistrations.Where(er => examRegistrationIds.Contains(er.Id)))
            {
                result.TryAdd(er.Id, name);
            }
        }

        var missingIds = examRegistrationIds.Where(id => !result.ContainsKey(id)).ToList();
        if (missingIds.Count > 0)
        {
            var voucherNames = await context.ExamRegistrations
                .AsNoTracking()
                .Where(er => missingIds.Contains(er.Id) && er.ApplicationVoucherId != null)
                .Select(er => new { er.Id, VoucherName = er.ApplicationVoucher!.StudentName })
                .ToListAsync();

            foreach (var v in voucherNames)
            {
                result.TryAdd(v.Id, v.VoucherName);
            }
        }

        return result;
    }
}