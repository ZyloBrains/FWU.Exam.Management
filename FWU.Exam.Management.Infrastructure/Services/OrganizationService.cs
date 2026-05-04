using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class OrganizationService : IOrganizationService
{
    private readonly AppDbContext _context;

    public OrganizationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Organization?> GetOrganizationAsync()
    {
        return await _context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<Organization?> GetOrganizationByOfficeCodeAsync(string officeCode)
    {
        return await _context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.OfficeCode == officeCode);
    }
}
