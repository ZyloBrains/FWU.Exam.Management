using System.Linq.Expressions;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class SmtpConfigurationService(AppDbContext context) : ISmtpConfigurationService
{
    public async Task<(List<SmtpConfiguration> Items, int TotalCount)> GetSmtpConfigurationsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.SmtpConfigurations.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.Host!.Contains(search) ||
                s.From!.Contains(search) ||
                s.UserName!.Contains(search));
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

    public async Task<List<SmtpConfiguration>> GetFilteredSmtpConfigurationsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.SmtpConfigurations.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.Host!.Contains(search) ||
                s.From!.Contains(search) ||
                s.UserName!.Contains(search));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<SmtpConfiguration?> GetSmtpConfigurationByIdAsync(int id)
    {
        return await context.SmtpConfigurations.FindAsync(id);
    }

    public async Task CreateSmtpConfigurationAsync(SmtpConfiguration smtpConfiguration)
    {
        context.SmtpConfigurations.Add(smtpConfiguration);
        await context.SaveChangesAsync();
    }

    public async Task UpdateSmtpConfigurationAsync(SmtpConfiguration smtpConfiguration)
    {
        context.SmtpConfigurations.Update(smtpConfiguration);
        await context.SaveChangesAsync();
    }

    public async Task DeleteSmtpConfigurationAsync(int id)
    {
        var config = await context.SmtpConfigurations.FindAsync(id);
        if (config != null)
        {
            context.SmtpConfigurations.Remove(config);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> SmtpConfigurationExistsAsync(int id)
    {
        return await context.SmtpConfigurations.AnyAsync(s => s.Id == id);
    }

    private static Expression<Func<SmtpConfiguration, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "host" => s => s.Host!,
            "from" => s => s.From!,
            "username" => s => s.UserName!,
            "port" => s => s.Port,
            _ => s => s.Id
        };
    }
}
