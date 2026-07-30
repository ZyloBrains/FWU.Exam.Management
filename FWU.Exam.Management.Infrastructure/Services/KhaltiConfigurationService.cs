using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class KhaltiConfigurationService(AppDbContext context) : IKhaltiConfigurationService
{
    public async Task<KhaltiConfiguration?> GetActiveAsync()
    {
        return await context.KhaltiConfigurations.AsNoTracking().FirstOrDefaultAsync();
    }
}
