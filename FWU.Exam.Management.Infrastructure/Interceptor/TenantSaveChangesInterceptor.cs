using FWU.Exam.Management.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Interceptor;

public class TenantSaveChangesInterceptor(ITenantContext tenantContext, ILogger<TenantSaveChangesInterceptor> logger) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SetTenantId(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        SetTenantId(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void SetTenantId(DbContext? context)
    {
        if (context == null) return;
        if (tenantContext.TenantId <= 0) return;
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is ITenantScoped && e.State == EntityState.Added);
        foreach (var entry in entries)
        {
            var currentValue = entry.Property("TenantId").CurrentValue;
            if (currentValue is int id && id != 0) continue;
            entry.Property("TenantId").CurrentValue = tenantContext.TenantId;
            logger.LogInformation("TenantId {TenantId} set for {EntityType}", tenantContext.TenantId, entry.Entity.GetType().Name);
        }
    }
}
