using System.Text.Json;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Interceptor;

public class AuditLogInterceptor(
    IAuditUserProvider userProvider,
    ITenantContext tenantContext,
    IUserContext userContext,
    ILogger<AuditLogInterceptor> logger) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        CaptureChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        CaptureChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void CaptureChanges(DbContext? context)
    {
        if (context == null) return;

        var tenantId = tenantContext.TenantId;
        if (tenantId <= 0) return;

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog && e.Entity is not Tenant && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

        var userName = userProvider.GetCurrentUserName() ?? "System";
        var userId = userContext.UserId;
        var now = DateTime.UtcNow;
        var auditLogs = new List<AuditLog>();

        foreach (var entry in entries)
        {
            var entityName = entry.Metadata.ShortName();
            var entityId = GetPrimaryKeyValue(entry);
            var action = entry.State switch
            {
                EntityState.Added => "Created",
                EntityState.Modified => "Updated",
                EntityState.Deleted => "Deleted",
                _ => "Unknown"
            };

            var changes = GetChanges(entry);
            if (entry.State == EntityState.Added && changes.Count == 0)
                continue;

            var changesJson = changes.Count > 0
                ? JsonSerializer.Serialize(changes, new JsonSerializerOptions { WriteIndented = false })
                : null;

            auditLogs.Add(new AuditLog
            {
                TenantId = tenantId,
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                UserName = userName,
                UserId = userId,
                Timestamp = now,
                ChangesJson = changesJson
            });
        }

        if (auditLogs.Count > 0)
        {
            context.Set<AuditLog>().AddRange(auditLogs);
            logger.LogInformation("Captured {Count} audit log entries", auditLogs.Count);
        }
    }

    private static List<Dictionary<string, object?>> GetChanges(EntityEntry entry)
    {
        var changes = new List<Dictionary<string, object?>>();

        var properties = entry.Properties.Where(p =>
            p.Metadata is IProperty prop &&
            prop.Name != "TenantId" &&
            !IsShadowProperty(p.Metadata));

        switch (entry.State)
        {
            case EntityState.Added:
                foreach (var prop in properties)
                {
                    if (prop.CurrentValue == null && prop.Metadata.IsNullable) continue;
                    if (prop.CurrentValue is string s && string.IsNullOrEmpty(s)) continue;
                    changes.Add(new Dictionary<string, object?>
                    {
                        ["field"] = prop.Metadata.Name,
                        ["old"] = null,
                        ["new"] = FormatValue(prop.CurrentValue)
                    });
                }
                break;

            case EntityState.Modified:
                foreach (var prop in properties)
                {
                    if (Equals(prop.OriginalValue, prop.CurrentValue)) continue;
                    changes.Add(new Dictionary<string, object?>
                    {
                        ["field"] = prop.Metadata.Name,
                        ["old"] = FormatValue(prop.OriginalValue),
                        ["new"] = FormatValue(prop.CurrentValue)
                    });
                }
                break;

            case EntityState.Deleted:
                foreach (var prop in properties)
                {
                    if (prop.OriginalValue == null) continue;
                    changes.Add(new Dictionary<string, object?>
                    {
                        ["field"] = prop.Metadata.Name,
                        ["old"] = FormatValue(prop.OriginalValue),
                        ["new"] = null
                    });
                }
                break;
        }

        return changes;
    }

    private static string GetPrimaryKeyValue(EntityEntry entry)
    {
        var key = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
        return key?.CurrentValue?.ToString() ?? "";
    }

    private static bool IsShadowProperty(IPropertyBase property)
    {
        return string.IsNullOrEmpty(property.GetFieldName());
    }

    private static object? FormatValue(object? value)
    {
        return value switch
        {
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
            DateOnly d => d.ToString("yyyy-MM-dd"),
            TimeOnly t => t.ToString("HH:mm:ss"),
            byte[] bytes => $"[bytes:{bytes.Length}]",
            _ => value
        };
    }
}
