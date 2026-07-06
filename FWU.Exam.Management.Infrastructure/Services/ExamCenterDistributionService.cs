using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ExamCenterDistributionService(AppDbContext context) : IExamCenterDistributionService
{
    public async Task AssignSymbolNumbersAsync(int examScheduleId)
    {
        var registrations = await context.ExamRegistrations
            .Where(er => er.ExamScheduleId == examScheduleId && er.IsActive && er.Status >= RegistrationStatus.Registered)
            .Include(er => er.College)
                .ThenInclude(c => c.Faculties)
            .Include(er => er.ExamSchedule)
            .OrderBy(er => er.Id)
            .ToListAsync();

        var existingCount = await context.ExamRegistrations
            .CountAsync(er => er.ExamScheduleId == examScheduleId && er.SymbolNumber != null);

        foreach (var reg in registrations)
        {
            if (!string.IsNullOrEmpty(reg.SymbolNumber))
                continue;

            var facultyCode = reg.College?.Faculties?.FirstOrDefault()?.OfficeCode ?? "FWU";
            var seq = (existingCount + 1).ToString("D4");
            reg.SymbolNumber = $"{facultyCode}{seq}";
            existingCount++;
        }

        await context.SaveChangesAsync();
    }

    public async Task<List<ExamCenterSymbolRange>> GetRangesAsync(int examScheduleId)
    {
        return await context.ExamCenterSymbolRanges
            .AsNoTracking()
            .Where(r => r.ExamScheduleId == examScheduleId)
            .Include(r => r.ExamCenter)
                .ThenInclude(ec => ec.College)
            .ToListAsync();
    }

    public async Task SetSymbolRangeAsync(int examCenterId, int examScheduleId, long fromSymbol, long toSymbol)
    {
        var existing = await context.ExamCenterSymbolRanges
            .FirstOrDefaultAsync(r => r.ExamCenterId == examCenterId && r.ExamScheduleId == examScheduleId);

        if (existing != null)
        {
            existing.FromSymbolNumber = fromSymbol;
            existing.ToSymbolNumber = toSymbol;
        }
        else
        {
            context.ExamCenterSymbolRanges.Add(new ExamCenterSymbolRange
            {
                ExamCenterId = examCenterId,
                ExamScheduleId = examScheduleId,
                FromSymbolNumber = fromSymbol,
                ToSymbolNumber = toSymbol
            });
        }

        await context.SaveChangesAsync();
    }

    public async Task ClearRangesAsync(int examScheduleId)
    {
        var ranges = await context.ExamCenterSymbolRanges
            .Where(r => r.ExamScheduleId == examScheduleId)
            .ToListAsync();

        context.ExamCenterSymbolRanges.RemoveRange(ranges);
        await context.SaveChangesAsync();
    }

    public async Task<int> DistributeStudentsAsync(int examScheduleId)
    {
        var ranges = await context.ExamCenterSymbolRanges
            .Where(r => r.ExamScheduleId == examScheduleId)
            .ToListAsync();

        if (ranges.Count == 0) return 0;

        var registrations = await context.ExamRegistrations
            .Where(er => er.ExamScheduleId == examScheduleId && er.IsActive && er.Status >= RegistrationStatus.Registered)
            .ToListAsync();

        var assignedCount = 0;
        foreach (var reg in registrations)
        {
            if (string.IsNullOrEmpty(reg.SymbolNumber))
                continue;

            if (!long.TryParse(reg.SymbolNumber, out var symbolNum))
            {
                if (reg.SymbolNumber != null && reg.SymbolNumber.Length >= 3)
                {
                    var lastPart = reg.SymbolNumber[^4..];
                    symbolNum = long.TryParse(lastPart, out var n) ? n : 0;
                }
            }

            var range = ranges.FirstOrDefault(r =>
            {
                var from = r.FromSymbolNumber;
                var to = r.ToSymbolNumber;

                if (symbolNum > 0)
                    return symbolNum >= from && symbolNum <= to;

                return false;
            });

            if (range != null)
            {
                reg.ExamCenterId = range.ExamCenterId;
                assignedCount++;
            }
        }

        await context.SaveChangesAsync();
        return assignedCount;
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
            .CountAsync(er => er.ExamScheduleId == examScheduleId && er.IsActive && er.Status >= RegistrationStatus.Registered);
    }

    public async Task<int> GetAssignedCountAsync(int examScheduleId)
    {
        return await context.ExamRegistrations
            .CountAsync(er => er.ExamScheduleId == examScheduleId && er.ExamCenterId != null);
    }

    public async Task<int> GetUnassignedCountAsync(int examScheduleId)
    {
        return await context.ExamRegistrations
            .CountAsync(er => er.ExamScheduleId == examScheduleId && er.IsActive && er.Status >= RegistrationStatus.Registered && er.ExamCenterId == null);
    }

    public async Task<Dictionary<int, int>> GetCenterDistributionCountsAsync(int examScheduleId)
    {
        return await context.ExamRegistrations
            .Where(er => er.ExamScheduleId == examScheduleId && er.ExamCenterId != null)
            .GroupBy(er => er.ExamCenterId!.Value)
            .Select(g => new { ExamCenterId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.ExamCenterId, g => g.Count);
    }
}
