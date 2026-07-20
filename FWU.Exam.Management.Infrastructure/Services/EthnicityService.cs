using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class EthnicityService(AppDbContext context) : IEthnicityService
{
    public async Task<(List<Ethnicity> Items, int TotalCount)> GetEthnicitiesAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.Ethnicities.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e => e.EthnicityName!.Contains(search));
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

    public async Task<Ethnicity?> GetEthnicityByIdAsync(int id)
    {
        return await context.Ethnicities.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateEthnicityAsync(Ethnicity ethnicity)
    {
        context.Ethnicities.Add(ethnicity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateEthnicityAsync(Ethnicity ethnicity)
    {
        context.Ethnicities.Update(ethnicity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteEthnicityAsync(int id)
    {
        var ethnicity = await context.Ethnicities.FindAsync(id);
        if (ethnicity != null)
        {
            context.Ethnicities.Remove(ethnicity);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> EthnicityExistsAsync(int id)
    {
        return await context.Ethnicities.AnyAsync(e => e.Id == id);
    }

    private static Expression<Func<Ethnicity, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "ethnicityname" => e => e.EthnicityName!,
            "isdefault" => e => e.IsDefault,
            "isactive" => e => e.IsActive,
            _ => e => e.Id
        };
    }
}
