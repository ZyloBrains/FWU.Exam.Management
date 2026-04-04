using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace fwu_examination_management_system.Data.Auditing;

public class AuditableSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IAuditUserProvider _userProvider;
    private readonly ILogger<AuditableSaveChangesInterceptor> _logger;

    public AuditableSaveChangesInterceptor(IAuditUserProvider userProvider, ILogger<AuditableSaveChangesInterceptor> logger)
    {
        _userProvider = userProvider;
        _logger = logger;
    }

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
        var user = _userProvider.GetCurrentUserName() ?? "System";
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedBy").CurrentValue = user;
                entry.Property("CreatedDate").CurrentValue = DateTime.UtcNow;
            }
            entry.Property("UpdatedBy").CurrentValue = user;
            entry.Property("UpdatedDate").CurrentValue = DateTime.UtcNow;
            _logger.LogInformation("Audit fields set for {EntityType} by {User}", entry.Entity.GetType().Name, user);
        }
    }
}

public interface IAuditUserProvider
{
    string? GetCurrentUserName();
}
