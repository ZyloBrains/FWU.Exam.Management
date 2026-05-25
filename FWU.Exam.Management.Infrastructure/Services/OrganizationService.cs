using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class OrganizationService(AppDbContext context) : IOrganizationService
{
    public async Task<List<Organization>> GetAllOrganizationsAsync()
    {
        return await context.Organizations
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .ToListAsync();
    }

    public async Task<Organization?> GetOrganizationByIdAsync(int id)
    {
        return await context.Organizations.FindAsync(id);
    }

    public async Task<Organization?> GetOrganizationByOfficeCodeAsync(string officeCode)
    {
        return await context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OfficeCode == officeCode);
    }

    public async Task CreateOrganizationAsync(Organization organization)
    {
        context.Organizations.Add(organization);
        await context.SaveChangesAsync();
    }

    public async Task UpdateOrganizationAsync(Organization organization)
    {
        context.Organizations.Update(organization);
        await context.SaveChangesAsync();
    }

    public async Task DeleteOrganizationAsync(int id)
    {
        var organization = await context.Organizations.FindAsync(id);
        if (organization != null)
        {
            context.Organizations.Remove(organization);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> OrganizationExistsAsync(int id)
    {
        return await context.Organizations.AnyAsync(o => o.Id == id);
    }
}