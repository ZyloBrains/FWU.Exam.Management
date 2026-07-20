using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class GenderService(AppDbContext context) : IGenderService
{
    public async Task<(List<Gender> Items, int TotalCount)> GetGendersAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.Genders.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(g => g.GenderName!.Contains(search));
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

    public async Task<Gender?> GetGenderByIdAsync(int id)
    {
        return await context.Genders.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateGenderAsync(Gender gender)
    {
        context.Genders.Add(gender);
        await context.SaveChangesAsync();
    }

    public async Task UpdateGenderAsync(Gender gender)
    {
        context.Genders.Update(gender);
        await context.SaveChangesAsync();
    }

    public async Task DeleteGenderAsync(int id)
    {
        var gender = await context.Genders.FindAsync(id);
        if (gender != null)
        {
            context.Genders.Remove(gender);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> GenderExistsAsync(int id)
    {
        return await context.Genders.AnyAsync(e => e.Id == id);
    }

    private static Expression<Func<Gender, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "gendername" => g => g.GenderName!,
            "isactive" => g => g.IsActive,
            _ => g => g.Id
        };
    }
}
