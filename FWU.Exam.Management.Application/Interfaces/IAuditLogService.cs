using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IAuditLogService
{
    Task<(List<AuditLog> Items, int TotalCount)> GetAuditLogsAsync(
        int page, int pageSize, string? entityName, string? action,
        string? userName, DateTime? from, DateTime? to, string? search);

    Task<List<string>> GetEntityNamesAsync();
}
