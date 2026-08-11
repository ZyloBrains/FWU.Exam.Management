using FWU.Exam.Management.Domain.Constants;

namespace FWU.Exam.Management.Application.Interfaces;

/// <summary>
/// Records business-level activity events (logins, approvals, payment verifications, ...)
/// into the persistent audit trail. Distinct from Serilog: rows are tenant-scoped, attributed
/// to the acting user, and queryable through the Audit Log UI.
/// </summary>
public interface IAuditLogWriter
{
    /// <summary>
    /// Writes a business activity audit record. Never throws.
    /// </summary>
    /// <param name="activityType">A fixed key from <see cref="ActivityTypes"/>.</param>
    /// <param name="description">Optional human-readable summary.</param>
    /// <param name="details">Optional structured payload (serialized to JSON).</param>
    /// <param name="severity">One of <see cref="AuditSeverity"/>.</param>
    /// <param name="entityName">Optional related entity type name.</param>
    /// <param name="entityId">Optional related entity primary key.</param>
    /// <param name="actorUserId">Explicit acting user id for service-layer calls (falls back to the current HTTP user).</param>
    Task LogAsync(
        string activityType,
        string? description = null,
        object? details = null,
        string severity = AuditSeverity.Info,
        string? entityName = null,
        string? entityId = null,
        string? actorUserId = null);
}
