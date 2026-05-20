using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Location;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class LocalLevelService(AppDbContext context) : ILocalLevelService
{
    public async Task<(List<LocalLevel> Items, int TotalCount)> GetLocalLevelsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.LocalLevels
            .Include(ll => ll.District)
            .ThenInclude(d => d!.Province)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(ll =>
                ll.LocalLevelName.Contains(search) ||
                (ll.District != null && ll.District.DistrictName.Contains(search)));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(ll => ll.LocalLevelName)
            : query.OrderBy(ll => ll.LocalLevelName);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<LocalLevel>> GetFilteredLocalLevelsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.LocalLevels
            .Include(ll => ll.District)
            .ThenInclude(d => d!.Province)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(ll =>
                ll.LocalLevelName.Contains(search) ||
                (ll.District != null && ll.District.DistrictName.Contains(search)));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(ll => ll.LocalLevelName)
            : query.OrderBy(ll => ll.LocalLevelName);

        return await query.ToListAsync();
    }

    public async Task<LocalLevel?> GetLocalLevelByIdAsync(int id)
    {
        return await context.LocalLevels
            .Include(ll => ll.District)
            .ThenInclude(d => d!.Province)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateLocalLevelAsync(LocalLevel localLevel)
    {
        context.LocalLevels.Add(localLevel);
        await context.SaveChangesAsync();
    }

    public async Task UpdateLocalLevelAsync(LocalLevel localLevel)
    {
        context.LocalLevels.Update(localLevel);
        await context.SaveChangesAsync();
    }

    public async Task DeleteLocalLevelAsync(int id)
    {
        var localLevel = await context.LocalLevels.FindAsync(id);
        if (localLevel != null)
        {
            context.LocalLevels.Remove(localLevel);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> LocalLevelExistsAsync(int id)
    {
        return await context.LocalLevels.AnyAsync(e => e.Id == id);
    }

    public async Task<List<District>> GetActiveDistrictsAsync()
    {
        return await context.Districts
            .Where(d => d.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }
}
