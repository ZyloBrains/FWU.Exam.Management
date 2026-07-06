using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Semesters;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class SemesterService(AppDbContext context) : ISemesterService
{
    public async Task<(List<Semester> Items, int TotalCount)> GetSemestersAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.Semesters.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.Name.Contains(search) ||
                s.Code.Contains(search) ||
                s.Remark.Contains(search));
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

    public async Task<List<Semester>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.Semesters.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.Name.Contains(search) ||
                s.Code.Contains(search) ||
                s.Remark.Contains(search));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<Semester?> GetSemesterByIdAsync(int id)
    {
        return await context.Semesters.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateSemesterAsync(Semester semester)
    {
        context.Semesters.Add(semester);
        await context.SaveChangesAsync();
    }

    public async Task UpdateSemesterAsync(Semester semester)
    {
        context.Semesters.Update(semester);
        await context.SaveChangesAsync();
    }

    public async Task DeleteSemesterAsync(int id)
    {
        var semester = await context.Semesters.FindAsync(id);
        if (semester != null)
        {
            context.Semesters.Remove(semester);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> SemesterExistsAsync(int id)
    {
        return await context.Semesters.AnyAsync(s => s.Id == id);
    }

    private static Expression<Func<Semester, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "code" => s => s.Code ?? "",
            "name" => s => s.Name ?? "",
            "number" => s => s.Number,
            "year" => s => s.Year,
            "startdate" => s => s.StartDate,
            "enddate" => s => s.EndDate,
            "remark" => s => s.Remark ?? "",
            _ => s => s.Name ?? ""
        };
    }
}
