using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class AuditLogService(AppDbContext context) : IAuditLogService
{
    public async Task<(List<AuditLog> Items, int TotalCount)> GetAuditLogsAsync(
        int page, int pageSize, string? entityName, string? action,
        string? userName, DateTime? from, DateTime? to, string? search)
    {
        var query = context.AuditLogs!.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(a => a.EntityName == entityName);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);

        if (!string.IsNullOrWhiteSpace(userName))
            query = query.Where(a => a.UserName != null && a.UserName.Contains(userName));

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a =>
                (a.EntityName != null && a.EntityName.Contains(search)) ||
                (a.EntityId != null && a.EntityId.Contains(search)) ||
                (a.UserName != null && a.UserName.Contains(search)));

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<string>> GetEntityNamesAsync()
    {
        return await context.AuditLogs!
            .AsNoTracking()
            .Where(a => a.EntityName != null)
            .Select(a => a.EntityName!)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync();
    }
}
