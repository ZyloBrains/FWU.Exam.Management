using System.Collections.Concurrent;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;

namespace FWU.Exam.Management.Infrastructure.Tests;

public sealed record AuditLogEntry(string ActivityType, string? Description, string? DetailsJson, string Severity, string? EntityName, string? EntityId, string? ActorUserId);

/// <summary>
/// In-memory implementation of <see cref="IAuditLogWriter"/> for tests.
/// Records every written activity so tests can assert on it.
/// </summary>
public sealed class TestAuditLogWriter : IAuditLogWriter
{
    private readonly ConcurrentQueue<AuditLogEntry> _entries = new();

    public IReadOnlyList<AuditLogEntry> Entries => _entries.ToList();

    public Task LogAsync(
        string activityType,
        string? description = null,
        object? details = null,
        string severity = AuditSeverity.Info,
        string? entityName = null,
        string? entityId = null,
        string? actorUserId = null)
    {
        _entries.Enqueue(new AuditLogEntry(
            activityType,
            description,
            details == null ? null : System.Text.Json.JsonSerializer.Serialize(details),
            severity,
            entityName,
            entityId,
            actorUserId));
        return Task.CompletedTask;
    }
}
