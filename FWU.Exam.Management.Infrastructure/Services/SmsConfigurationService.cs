using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class SmsConfigurationService(AppDbContext context) : ISmsConfigurationService
{
    public async Task<List<SmsConfiguration>> GetAllAsync()
    {
        return await context.SmsConfigurations.AsNoTracking().ToListAsync();
    }

    public async Task<SmsConfiguration?> GetByIdAsync(int id)
    {
        return await context.SmsConfigurations.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task CreateAsync(SmsConfiguration config)
    {
        context.SmsConfigurations.Add(config);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SmsConfiguration config)
    {
        context.SmsConfigurations.Update(config);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var config = await context.SmsConfigurations.FindAsync(id);
        if (config != null)
        {
            context.SmsConfigurations.Remove(config);
            await context.SaveChangesAsync();
        }
    }
}
