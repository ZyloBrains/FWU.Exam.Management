using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ExamCenterDistributionService(AppDbContext context) : IExamCenterDistributionService
{
    public async Task AssignSymbolNumbersAsync(int examScheduleId)
    {
        var registrations = await context.ExamRegistrations.IgnoreQueryFilters()
            .Where(er => er.ExamScheduleId == examScheduleId && er.IsActive && er.Status >= RegistrationStatus.Registered)
            .Include(er => er.College)
                .ThenInclude(c => c.Faculties)
            .Include(er => er.ExamSchedule)
                .ThenInclude(es => es!.AcademicYear)
            .OrderBy(er => er.Id)
            .ToListAsync();

        var examSchedule = registrations.FirstOrDefault()?.ExamSchedule;
        var academicYearCode = examSchedule?.AcademicYear?.AcademicYearCode
            ?? DateTime.Now.Year.ToString();
        var yy = academicYearCode.Length >= 2
            ? academicYearCode[^2..]
            : academicYearCode.PadLeft(2, '0');

        var existingCount = await context.ExamRegistrations.IgnoreQueryFilters()
            .CountAsync(er => er.ExamScheduleId == examScheduleId && er.SymbolNumber != null);

        foreach (var reg in registrations)
        {
            if (!string.IsNullOrEmpty(reg.SymbolNumber))
                continue;

            var collegeIdPart = reg.CollegeId.ToString("D3");
            var facultyId = reg.College?.Faculties?.FirstOrDefault()?.Id ?? 0;
            var facultyIdPart = facultyId.ToString("D2");
            var seq = (existingCount + 1).ToString("D3");

            reg.SymbolNumber = $"{yy}{collegeIdPart}{facultyIdPart}{seq}";
            existingCount++;
        }

        await context.SaveChangesAsync();
    }

    public async Task<int> DistributeStudentsAsync(int examScheduleId)
    {
        var centers = await context.ExamCenters
            .Where(ec => ec.ExamScheduleId == examScheduleId && ec.IsActive)
            .OrderBy(ec => ec.Id)
            .ToListAsync();

        if (centers.Count == 0) return 0;

        var registrations = await context.ExamRegistrations.IgnoreQueryFilters()
            .Where(er => er.ExamScheduleId == examScheduleId && er.IsActive && er.Status >= RegistrationStatus.Registered && er.SymbolNumber != null)
            .OrderBy(er => er.SymbolNumber)
            .ToListAsync();

        if (registrations.Count == 0) return 0;

        int centerIndex = 0;
        foreach (var reg in registrations)
        {
            reg.ExamCenterId = centers[centerIndex].Id;
            centerIndex = (centerIndex + 1) % centers.Count;
        }

        await context.SaveChangesAsync();
        return registrations.Count;
    }

    public async Task ResetDistributionAsync(int examScheduleId)
    {
        var registrations = await context.ExamRegistrations.IgnoreQueryFilters()
            .Where(er => er.ExamScheduleId == examScheduleId && er.ExamCenterId != null)
            .ToListAsync();

        foreach (var reg in registrations)
        {
            reg.ExamCenterId = null;
        }

        await context.SaveChangesAsync();
    }

    public async Task<int> GetRegisteredCountAsync(int examScheduleId)
    {
        return await context.ExamRegistrations.IgnoreQueryFilters()
            .CountAsync(er => er.ExamScheduleId == examScheduleId && er.IsActive && er.Status >= RegistrationStatus.Registered);
    }

    public async Task<int> GetAssignedCountAsync(int examScheduleId)
    {
        return await context.ExamRegistrations.IgnoreQueryFilters()
            .CountAsync(er => er.ExamScheduleId == examScheduleId && er.ExamCenterId != null);
    }

    public async Task<int> GetUnassignedCountAsync(int examScheduleId)
    {
        return await context.ExamRegistrations.IgnoreQueryFilters()
            .CountAsync(er => er.ExamScheduleId == examScheduleId && er.IsActive && er.Status >= RegistrationStatus.Registered && er.ExamCenterId == null);
    }

    public async Task<Dictionary<int, int>> GetCenterDistributionCountsAsync(int examScheduleId)
    {
        return await context.ExamRegistrations.IgnoreQueryFilters()
            .Where(er => er.ExamScheduleId == examScheduleId && er.ExamCenterId != null)
            .GroupBy(er => er.ExamCenterId!.Value)
            .Select(g => new { ExamCenterId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.ExamCenterId, g => g.Count);
    }
}
