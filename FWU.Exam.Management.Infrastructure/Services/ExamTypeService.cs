using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ExamTypeService(AppDbContext context) : IExamTypeService
{
    public async Task<(List<ExamType> Items, int TotalCount)> GetExamTypesAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.ExamTypes.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e =>
                e.Name.Contains(search) ||
                (e.Remarks ?? "").Contains(search));
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

    public async Task<List<ExamType>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.ExamTypes.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e =>
                e.Name.Contains(search) ||
                (e.Remarks ?? "").Contains(search));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<ExamType?> GetExamTypeByIdAsync(int id)
    {
        return await context.ExamTypes.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateExamTypeAsync(ExamType examType)
    {
        context.ExamTypes.Add(examType);
        await context.SaveChangesAsync();
    }

    public async Task UpdateExamTypeAsync(ExamType examType)
    {
        context.ExamTypes.Update(examType);
        await context.SaveChangesAsync();
    }

    public async Task DeleteExamTypeAsync(int id)
    {
        var examType = await context.ExamTypes.FindAsync(id);
        if (examType != null)
        {
            context.ExamTypes.Remove(examType);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExamTypeExistsAsync(int id)
    {
        return await context.ExamTypes.AnyAsync(e => e.Id == id);
    }

    private static Expression<Func<ExamType, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "code" => e => e.Code,
            "name" => e => e.Name,
            "remarks" => e => e.Remarks ?? "",
            "isactive" => e => e.IsActive,
            _ => e => e.Name
        };
    }
}
