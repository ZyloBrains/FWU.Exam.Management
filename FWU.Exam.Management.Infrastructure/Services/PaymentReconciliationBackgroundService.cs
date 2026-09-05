using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;

public class PaymentReconciliationBackgroundService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<PaymentReconciliationBackgroundService> logger) : BackgroundService
{
    // Automated reconciliation is intentionally infrequent: the primary way to
    // verify pending payments is the manual "Check All Pending" button in the
    // admin Payment Reconciliation page. Frequent polling loads the gateway and
    // the server, so the background run is opt-in via configuration. Set
    // PaymentReconciliation:IntervalMinutes to 0 (or leave unset) to disable
    // background reconciliation entirely and rely on the manual button.
    private TimeSpan? GetInterval()
    {
        var minutes = configuration.GetValue<int?>("PaymentReconciliation:IntervalMinutes");
        if (minutes is null or <= 0)
            return null;

        return TimeSpan.FromMinutes(minutes.Value);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PaymentReconciliationBackgroundService: started.");

        var interval = GetInterval();

        // Disabled: do nothing (pending payments are checked via the manual
        // "Check All Pending" button in the admin UI).
        if (interval == null)
        {
            logger.LogInformation("PaymentReconciliationBackgroundService: disabled via configuration (PaymentReconciliation:IntervalMinutes).");
            await Task.Delay(Timeout.Infinite, stoppingToken);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunReconciliationAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "PaymentReconciliationBackgroundService: reconciliation run failed.");
                await WriteAuditAsync(
                    "Payment reconciliation run failed.",
                    new { error = ex.Message },
                    AuditSeverity.Error);
            }

            try
            {
                await Task.Delay(interval.Value, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        logger.LogInformation("PaymentReconciliationBackgroundService: stopped.");
    }

    private async Task RunReconciliationAsync()
    {
        using var scope = scopeFactory.CreateScope();

        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
        tenantContext.SetTenant(1, "OCE", TenantType.Central);

        var reconciliationService = scope.ServiceProvider.GetRequiredService<Application.Interfaces.IPaymentReconciliationService>();
        var reconciledCount = await reconciliationService.ReconcilePendingBatchAsync();

        var message = reconciledCount > 0
            ? $"Payment reconciliation completed; {reconciledCount} payment(s) reconciled."
            : "Payment reconciliation run completed; no pending payments were reconciled.";

        await WriteAuditAsync(message, new { reconciledCount }, AuditSeverity.Info);
        logger.LogInformation("PaymentReconciliationBackgroundService: run completed; {Count} payment(s) reconciled.", reconciledCount);
    }

    private async Task WriteAuditAsync(string description, object? details, string severity)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();
            tenantContext.SetTenant(1, "OCE", TenantType.Central);
            var auditLogWriter = scope.ServiceProvider.GetRequiredService<Application.Interfaces.IAuditLogWriter>();
            await auditLogWriter.LogAsync(ActivityTypes.PaymentReconciled, description, details, severity);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PaymentReconciliationBackgroundService: failed to write audit record.");
        }
    }
}