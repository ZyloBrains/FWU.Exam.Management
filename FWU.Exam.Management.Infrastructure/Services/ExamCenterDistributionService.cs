using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ExamCenterDistributionService(AppDbContext context) : IExamCenterDistributionService
{
    public async Task<int> DistributeStudentsAsync(int examScheduleId)
    {
        var registrations = await context.ExamRegistrations
            .Where(er => er.ExamScheduleId == examScheduleId
                && er.IsActive
                && er.Status >= RegistrationStatus.CollegeVerified
                && er.SymbolNumber != null)
            .OrderBy(er => er.SymbolNumber)
            .ToListAsync();

        if (registrations.Count == 0) return 0;

        var centers = await context.ExamCenters
            .Where(ec => ec.ExamScheduleId == examScheduleId && ec.IsActive)
            .ToListAsync();

        var missingCollegeIds = registrations
            .Select(r => r.CollegeId)
            .Distinct()
            .Where(cid => !centers.Any(c => c.CollegeId == cid))
            .ToList();

        if (missingCollegeIds.Count > 0)
        {
            var colleges = await context.Colleges
                .Where(c => missingCollegeIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c);

            foreach (var collegeId in missingCollegeIds)
            {
                var college = colleges.GetValueOrDefault(collegeId);
                var center = new ExamCenter
                {
                    ExamScheduleId = examScheduleId,
                    CollegeId = collegeId,
                    IsActive = true,
                    Code = BuildCenterCode(college, collegeId),
                };
                context.ExamCenters.Add(center);
                centers.Add(center);
            }

            await context.SaveChangesAsync();
        }

        var centerByCollege = centers
            .GroupBy(c => c.CollegeId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.Id).First());

        var assigned = 0;
        foreach (var reg in registrations)
        {
            if (centerByCollege.TryGetValue(reg.CollegeId, out var center))
            {
                reg.ExamCenterId = center.Id;
                assigned++;
            }
        }

        await context.SaveChangesAsync();
        return assigned;
    }

    public async Task MoveStudentToCenterAsync(int registrationId, int examCenterId)
    {
        var reg = await context.ExamRegistrations
            .FirstOrDefaultAsync(er => er.Id == registrationId)
            ?? throw new InvalidOperationException("Registration not found.");

        var centerExists = await context.ExamCenters
            .AnyAsync(ec => ec.Id == examCenterId && ec.IsActive);
        if (!centerExists)
            throw new InvalidOperationException("Exam center not found or inactive.");

        reg.ExamCenterId = examCenterId;
        await context.SaveChangesAsync();
    }

    public async Task<List<DistributedStudentInfo>> GetDistributedStudentsAsync(int examScheduleId)
    {
        return await context.ExamRegistrations
            .AsNoTracking()
            .Include(er => er.College)
            .Include(er => er.ExamCenter)
            .Where(er => er.ExamScheduleId == examScheduleId && er.IsActive && er.Status >= RegistrationStatus.CollegeVerified)
            .OrderBy(er => er.SymbolNumber)
            .Select(er => new DistributedStudentInfo
            {
                RegistrationId = er.Id,
                SymbolNumber = er.SymbolNumber,
                CollegeName = er.College != null ? er.College.Name : null,
                IsSupplementary = er.IsSupplementary,
                ExamCenterId = er.ExamCenterId,
                ExamCenterCode = er.ExamCenter != null ? er.ExamCenter.Code : null,
            })
            .ToListAsync();
    }

    public async Task ResetDistributionAsync(int examScheduleId)
    {
        var registrations = await context.ExamRegistrations
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
        return await context.ExamRegistrations
            .CountAsync(er => er.ExamScheduleId == examScheduleId && er.IsActive && er.Status >= RegistrationStatus.CollegeVerified);
    }

    public async Task<int> GetAssignedCountAsync(int examScheduleId)
    {
        return await context.ExamRegistrations
            .CountAsync(er => er.ExamScheduleId == examScheduleId && er.ExamCenterId != null);
    }

    public async Task<int> GetUnassignedCountAsync(int examScheduleId)
    {
        return await context.ExamRegistrations
            .CountAsync(er => er.ExamScheduleId == examScheduleId && er.IsActive && er.Status >= RegistrationStatus.CollegeVerified && er.ExamCenterId == null);
    }

    public async Task<Dictionary<int, int>> GetCenterDistributionCountsAsync(int examScheduleId)
    {
        return await context.ExamRegistrations
            .Where(er => er.ExamScheduleId == examScheduleId && er.ExamCenterId != null)
            .GroupBy(er => er.ExamCenterId!.Value)
            .Select(g => new { ExamCenterId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.ExamCenterId, g => g.Count);
    }

    private static string BuildCenterCode(Domain.Entities.Colleges.College? college, int collegeId)
    {
        var code = college?.ShortName;
        if (string.IsNullOrWhiteSpace(code)) code = college?.Code;
        if (string.IsNullOrWhiteSpace(code)) code = $"C{collegeId:D3}";
        return code.Length > 30 ? code[..30] : code;
    }
}
