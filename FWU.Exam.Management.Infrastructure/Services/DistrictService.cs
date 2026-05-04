using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class DistrictService : IDistrictService
{
    private readonly AppDbContext _context;

    public DistrictService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<District> Items, int TotalCount)> GetDistrictsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.Districts
            .Include(d => d.Province)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(d =>
                d.DistrictCode.Contains(search) ||
                d.DistrictName.Contains(search) ||
                (d.Province != null && d.Province.ProvinceName.Contains(search)));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(d => d.DistrictName)
            : query.OrderBy(d => d.DistrictName);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<District>> GetFilteredDistrictsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.Districts
            .Include(d => d.Province)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(d =>
                d.DistrictCode.Contains(search) ||
                d.DistrictName.Contains(search) ||
                (d.Province != null && d.Province.ProvinceName.Contains(search)));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(d => d.DistrictName)
            : query.OrderBy(d => d.DistrictName);

        return await query.ToListAsync();
    }

    public async Task<District?> GetDistrictByIdAsync(int id)
    {
        return await _context.Districts
            .Include(d => d.Province)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateDistrictAsync(District district)
    {
        _context.Districts.Add(district);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateDistrictAsync(District district)
    {
        _context.Districts.Update(district);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteDistrictAsync(int id)
    {
        var district = await _context.Districts.FindAsync(id);
        if (district != null)
        {
            _context.Districts.Remove(district);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> DistrictExistsAsync(int id)
    {
        return await _context.Districts.AnyAsync(e => e.Id == id);
    }

    public async Task<List<Province>> GetActiveProvincesAsync()
    {
        return await _context.Provinces
            .Where(p => p.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }
}
