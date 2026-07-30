using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ESewaConfigurationService(AppDbContext context) : IESewaConfigurationService
{
    public async Task<ESewaConfiguration?> GetActiveAsync()
    {
        return await context.ESewaConfigurations.AsNoTracking().FirstOrDefaultAsync();
    }
}
