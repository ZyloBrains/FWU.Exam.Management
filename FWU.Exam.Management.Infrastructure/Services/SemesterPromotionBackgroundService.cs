using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;

public class SemesterPromotionBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<SemesterPromotionBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan _interval = TimeSpan.FromDays(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunPromotionAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SemesterPromotionBackgroundService: promotion run failed.");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunPromotionAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        tenantContext.SetTenant(0, string.Empty, TenantType.Central);

        var enrollmentService = scope.ServiceProvider.GetRequiredService<ISemesterEnrollmentService>();
        var count = await enrollmentService.PromoteCompletedSemestersAsync();
        if (count > 0)
        {
            logger.LogInformation("SemesterPromotionBackgroundService: promoted {Count} student(s).", count);
            var auditLogWriter = scope.ServiceProvider.GetRequiredService<IAuditLogWriter>();
            await auditLogWriter.LogAsync(ActivityTypes.SemesterPromotionCompleted, $"Semester promotion completed for {count} student(s)", new { promotedCount = count });
        }
    }
}
