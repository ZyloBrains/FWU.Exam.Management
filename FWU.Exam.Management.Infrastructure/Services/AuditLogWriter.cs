using System.Text.Json;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Interceptor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;

public class AuditLogWriter(
    IServiceScopeFactory scopeFactory,
    IUserContext userContext,
    ITenantContext tenantContext,
    IAuditUserProvider userProvider,
    ILogger<AuditLogWriter> logger) : IAuditLogWriter
{
    private const int MaxDetailsJsonLength = 10000;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    public async Task LogAsync(
        string activityType,
        string? description = null,
        object? details = null,
        string severity = AuditSeverity.Info,
        string? entityName = null,
        string? entityId = null,
        string? actorUserId = null)
    {
        try
        {
            var resolvedUserId = actorUserId ?? userContext.UserId;

            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userName = userProvider.GetCurrentUserName();
            if (!string.IsNullOrEmpty(actorUserId) && actorUserId != userContext.UserId)
            {
                userName = await context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == actorUserId)
                    .Select(u => u.UserName)
                    .FirstOrDefaultAsync() ?? userName;
            }

            var row = new AuditLog
            {
                TenantId = tenantContext.TenantId > 0 ? tenantContext.TenantId : null,
                Kind = AuditLogKinds.Activity,
                ActivityType = Truncate(activityType, 128),
                Description = Truncate(description, 500),
                Severity = Truncate(severity, 32),
                EntityName = Truncate(entityName, 128),
                EntityId = Truncate(entityId, 128),
                UserName = Truncate(userName ?? "System", 256),
                UserId = Truncate(resolvedUserId, 128),
                Timestamp = DateTime.UtcNow,
                DetailsJson = SerializeDetails(details)
            };

            context.Set<AuditLog>().Add(row);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to write audit log activity {ActivityType}", activityType);
        }
    }

    private static string? SerializeDetails(object? details)
    {
        if (details == null) return null;
        var json = JsonSerializer.Serialize(details, JsonOptions);
        return json.Length <= MaxDetailsJsonLength ? json : json[..MaxDetailsJsonLength];
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
