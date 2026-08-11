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
        logger.LogInformation("SemesterPromotionBackgroundService: started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunPromotionAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SemesterPromotionBackgroundService: promotion run failed.");
                await WriteAuditAsync(
                    "Semester promotion run failed.",
                    new { error = ex.Message },
                    AuditSeverity.Error);
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
        logger.LogInformation("SemesterPromotionBackgroundService: stopped.");
    }

    private async Task RunPromotionAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        tenantContext.SetTenant(1, "OCE", TenantType.Central);

        var enrollmentService = scope.ServiceProvider.GetRequiredService<ISemesterEnrollmentService>();
        var count = await enrollmentService.PromoteCompletedSemestersAsync();

        await WriteAuditAsync(
            count > 0
                ? $"Semester promotion completed for {count} student(s)"
                : "Semester promotion run completed; no students were eligible.",
            new { promotedCount = count },
            AuditSeverity.Info);

        logger.LogInformation("SemesterPromotionBackgroundService: promotion run completed; promoted {Count} student(s).", count);
    }

    private async Task WriteAuditAsync(string description, object? details, string severity)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
            tenantContext.SetTenant(1, "OCE", TenantType.Central);
            var auditLogWriter = scope.ServiceProvider.GetRequiredService<IAuditLogWriter>();
            await auditLogWriter.LogAsync(ActivityTypes.SemesterPromotionCompleted, description, details, severity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SemesterPromotionBackgroundService: failed to write failure audit record.");
        }
    }
}
