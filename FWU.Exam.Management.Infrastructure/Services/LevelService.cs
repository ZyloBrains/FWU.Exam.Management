using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class LevelService : ILevelService
{
    private readonly AppDbContext _context;

    public LevelService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Level> Items, int TotalCount)> GetLevelsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.Levels.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(l =>
                l.LevelCode.Contains(search) ||
                l.LevelName.Contains(search) ||
                (l.Remarks != null && l.Remarks.Contains(search)));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Level>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.Levels.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(l =>
                l.LevelCode.Contains(search) ||
                l.LevelName.Contains(search) ||
                (l.Remarks != null && l.Remarks.Contains(search)));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<Level?> GetLevelByIdAsync(int id)
    {
        return await _context.Levels.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateLevelAsync(Level level)
    {
        _context.Levels.Add(level);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateLevelAsync(Level level)
    {
        _context.Levels.Update(level);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteLevelAsync(int id)
    {
        var level = await _context.Levels.FindAsync(id);
        if (level != null)
        {
            _context.Levels.Remove(level);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> LevelExistsAsync(int id)
    {
        return await _context.Levels.AnyAsync(e => e.Id == id);
    }

    private static Expression<Func<Level, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "levelcode" => l => l.LevelCode,
            "levelname" => l => l.LevelName,
            "leveldisplayorder" => l => l.LevelDisplayOrder,
            "remarks" => l => l.Remarks,
            "isrunning" => l => l.IsRunning,
            "isactive" => l => l.IsActive,
            _ => l => l.LevelDisplayOrder
        };
    }
}
