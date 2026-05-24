using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class TenantService(AppDbContext context) : ITenantService
{
    public async Task<Tenant?> GetTenantAsync()
    {
        return await context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<Tenant?> GetTenantByOfficeCodeAsync(string officeCode)
    {
        return await context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.OfficeCode == officeCode);
    }
}
