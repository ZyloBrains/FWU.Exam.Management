using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class GumpNowEmailConfigurationService(AppDbContext context) : IGumpNowEmailConfigurationService
{
    public async Task<List<GumpNowEmailConfiguration>> GetAllAsync()
    {
        return await context.GumpNowEmailConfigurations.AsNoTracking().ToListAsync();
    }

    public async Task<GumpNowEmailConfiguration?> GetByIdAsync(int id)
    {
        return await context.GumpNowEmailConfigurations.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task CreateAsync(GumpNowEmailConfiguration config)
    {
        context.GumpNowEmailConfigurations.Add(config);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(GumpNowEmailConfiguration config)
    {
        context.GumpNowEmailConfigurations.Update(config);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var config = await context.GumpNowEmailConfigurations.FindAsync(id);
        if (config != null)
        {
            context.GumpNowEmailConfigurations.Remove(config);
            await context.SaveChangesAsync();
        }
    }
}
