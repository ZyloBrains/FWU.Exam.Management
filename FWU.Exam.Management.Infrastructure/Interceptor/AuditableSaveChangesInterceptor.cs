using FWU.Exam.Management.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Interceptor;

public class AuditableSaveChangesInterceptor(IAuditUserProvider userProvider, ILogger<AuditableSaveChangesInterceptor> logger) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SetAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        SetAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void SetAuditFields(DbContext? context)
    {
        if (context == null) return;
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditable && (e.State == EntityState.Added || e.State == EntityState.Modified));
        var user = userProvider.GetCurrentUserName() ?? "System";
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedBy").CurrentValue = user;
                entry.Property("CreatedDate").CurrentValue = DateTime.UtcNow;
            }
            entry.Property("UpdatedBy").CurrentValue = user;
            entry.Property("UpdatedDate").CurrentValue = DateTime.UtcNow;
            logger.LogInformation("Audit fields set for {EntityType} by {User}", entry.Entity.GetType().Name, user);
        }
    }
}

public interface IAuditUserProvider
{
    string? GetCurrentUserName();
}
