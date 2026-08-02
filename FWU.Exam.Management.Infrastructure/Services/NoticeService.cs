using System.Linq.Expressions;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class NoticeService(AppDbContext context) : INoticeService
{
    public async Task<(List<Notice> Items, int TotalCount)> GetNoticesAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.Notices.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(n =>
                n.NoticeTitle.Contains(search) ||
                n.NoticePreview.Contains(search));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Notice>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.Notices.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(n =>
                n.NoticeTitle.Contains(search) ||
                n.NoticePreview.Contains(search));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<Notice?> GetNoticeByIdAsync(int id)
    {
        return await context.Notices.FindAsync(id);
    }

    public async Task CreateNoticeAsync(Notice notice)
    {
        context.Notices.Add(notice);
        await context.SaveChangesAsync();
    }

    public async Task UpdateNoticeAsync(Notice notice)
    {
        context.Notices.Update(notice);
        await context.SaveChangesAsync();
    }

    public async Task DeleteNoticeAsync(int id)
    {
        var notice = await context.Notices.FindAsync(id);
        if (notice != null)
        {
            context.Notices.Remove(notice);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> NoticeExistsAsync(int id)
    {
        return await context.Notices.AnyAsync(n => n.Id == id);
    }

    public async Task<List<Notice>> GetLatestNoticesAsync(int count)
    {
        return await context.Notices
            .AsNoTracking()
            .OrderByDescending(n => n.PublishedDate)
            .Take(count)
            .ToListAsync();
    }

    private static Expression<Func<Notice, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "noticetitle" => n => n.NoticeTitle,
            _ => n => n.PublishedDate ?? DateTime.MinValue
        };
    }
}
