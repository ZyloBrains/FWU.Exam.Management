using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class CountryService(AppDbContext context) : ICountryService
{
    public async Task<List<Country>> GetAllAsync()
    {
        return await context.Countries
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.CountryName)
            .ToListAsync();
    }

    public async Task<Country?> FindByNameAsync(string name)
    {
        return await context.Countries
            .FirstOrDefaultAsync(c => c.CountryName == name);
    }

    public async Task<Country> CreateAsync(string name)
    {
        var country = new Country
        {
            CountryName = name,
            IsActive = true
        };
        context.Countries.Add(country);
        await context.SaveChangesAsync();
        return country;
    }

    public async Task<(List<Country> Items, int TotalCount)> GetCountriesAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.Countries.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.CountryName!.Contains(search));
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

    public async Task<Country?> GetCountryByIdAsync(int id)
    {
        return await context.Countries.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateCountryAsync(Country country)
    {
        context.Countries.Add(country);
        await context.SaveChangesAsync();
    }

    public async Task UpdateCountryAsync(Country country)
    {
        context.Countries.Update(country);
        await context.SaveChangesAsync();
    }

    public async Task DeleteCountryAsync(int id)
    {
        var country = await context.Countries.FindAsync(id);
        if (country != null)
        {
            context.Countries.Remove(country);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> CountryExistsAsync(int id)
    {
        return await context.Countries.AnyAsync(c => c.Id == id);
    }

    private static Expression<Func<Country, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "countryname" => c => c.CountryName!,
            "isactive" => c => c.IsActive,
            _ => c => c.Id
        };
    }
}
