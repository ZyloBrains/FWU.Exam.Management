using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IAuditLogService
{
    Task<(List<AuditLog> Items, int TotalCount)> GetAuditLogsAsync(
        int page, int pageSize, string? entityName, string? actionType,
        string? userName, DateTime? from, DateTime? to, string? search,
        string? kind = null, string? activityType = null, string? severity = null);

    Task<List<string>> GetEntityNamesAsync();

    Task<List<string>> GetActivityTypesAsync();

    Task<AuditLog?> GetByIdAsync(int id);
}
