using System.Linq.Expressions;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ExamCenterService(AppDbContext context) : IExamCenterService
{
    public async Task<(List<ExamCenter> Items, int TotalCount)> GetExamCentersAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = BuildQuery(search);
        var totalCount = await query.CountAsync();

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<ExamCenter>> GetFilteredItemsAsync(string? search, string sort, string sortDir)
    {
        var query = BuildQuery(search);
        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));
        return await query.ToListAsync();
    }

    public async Task<ExamCenter?> GetExamCenterByIdAsync(int id)
    {
        return await context.ExamCenters
            .AsNoTracking()
            .Include(ec => ec.ExamSchedule)
            .Include(ec => ec.College)
            .FirstOrDefaultAsync(ec => ec.Id == id);
    }

    public async Task CreateExamCenterAsync(ExamCenter examCenter)
    {
        context.ExamCenters.Add(examCenter);
        await context.SaveChangesAsync();
    }

    public async Task UpdateExamCenterAsync(ExamCenter examCenter)
    {
        var existing = await context.ExamCenters.FindAsync(examCenter.Id);
        if (existing != null)
        {
            examCenter.TenantId = existing.TenantId;
            context.Entry(existing).CurrentValues.SetValues(examCenter);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteExamCenterAsync(int id)
    {
        var examCenter = await context.ExamCenters.FindAsync(id);
        if (examCenter != null)
        {
            examCenter.IsActive = false;
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExamCenterExistsAsync(int id)
    {
        return await context.ExamCenters.AnyAsync(ec => ec.Id == id);
    }

    private IQueryable<ExamCenter> BuildQuery(string? search)
    {
        IQueryable<ExamCenter> query = context.ExamCenters
            .AsNoTracking()
            .Include(ec => ec.ExamSchedule)
            .Include(ec => ec.College);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(ec =>
                (ec.Code != null && ec.Code.Contains(search)) ||
                (ec.College != null && ec.College.Name != null && ec.College.Name.Contains(search)) ||
                (ec.College != null && ec.College.Code != null && ec.College.Code.Contains(search)) ||
                (ec.ExamSchedule != null && ec.ExamSchedule.ExamScheduleName != null && ec.ExamSchedule.ExamScheduleName.Contains(search)) ||
                (ec.Remark != null && ec.Remark.Contains(search)));
        }

        return query;
    }

    private static Expression<Func<ExamCenter, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "code" => ec => ec.Code ?? string.Empty,
            "college" => ec => ec.College != null ? ec.College.Name : string.Empty,
            "schedule" => ec => ec.ExamSchedule != null ? ec.ExamSchedule.ExamScheduleName : string.Empty,
            "isactive" => ec => ec.IsActive,
            _ => ec => ec.Id
        };
    }
}
