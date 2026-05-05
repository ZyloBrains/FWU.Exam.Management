using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ProvinceService : IProvinceService
{
    private readonly AppDbContext _context;

    public ProvinceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Province> Items, int TotalCount)> GetProvincesAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.Provinces.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p =>
                p.ProvinceName != null && p.ProvinceName.Contains(search));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(p => p.IsActive)
            : query.OrderBy(p => p.ProvinceName);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Province>> GetFilteredProvincesAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.Provinces.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p =>
                p.ProvinceName != null && p.ProvinceName.Contains(search));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(p => p.IsActive)
            : query.OrderBy(p => p.ProvinceName);

        return await query.ToListAsync();
    }

    public async Task<Province?> GetProvinceByIdAsync(int id)
    {
        return await _context.Provinces.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateProvinceAsync(Province province)
    {
        _context.Provinces.Add(province);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateProvinceAsync(Province province)
    {
        _context.Provinces.Update(province);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProvinceAsync(int id)
    {
        var province = await _context.Provinces.FindAsync(id);
        if (province != null)
        {
            _context.Provinces.Remove(province);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ProvinceExistsAsync(int id)
    {
        return await _context.Provinces.AnyAsync(e => e.Id == id);
    }
}
