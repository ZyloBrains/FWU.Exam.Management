using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ProvinceService(AppDbContext context) : IProvinceService
{
    public async Task<(List<Province> Items, int TotalCount)> GetProvincesAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.Provinces.AsNoTracking();

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
        var query = context.Provinces.AsNoTracking();

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
        return await context.Provinces.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateProvinceAsync(Province province)
    {
        context.Provinces.Add(province);
        await context.SaveChangesAsync();
    }

    public async Task UpdateProvinceAsync(Province province)
    {
        context.Provinces.Update(province);
        await context.SaveChangesAsync();
    }

    public async Task DeleteProvinceAsync(int id)
    {
        var province = await context.Provinces.FindAsync(id);
        if (province != null)
        {
            context.Provinces.Remove(province);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ProvinceExistsAsync(int id)
    {
        return await context.Provinces.AnyAsync(e => e.Id == id);
    }
}
