using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class SubjectTypeService(AppDbContext context) : ISubjectTypeService
{
    public async Task<(List<SubjectType> Items, int TotalCount)> GetSubjectTypesAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.SubjectTypes.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.Name.Contains(search) ||
                (s.Code != null && s.Code.Contains(search)));
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

    public async Task<List<SubjectType>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.SubjectTypes.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.Name.Contains(search) ||
                (s.Code != null && s.Code.Contains(search)));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<SubjectType?> GetSubjectTypeByIdAsync(int id)
    {
        return await context.SubjectTypes.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateSubjectTypeAsync(SubjectType subjectType)
    {
        if (subjectType.IsDefault)
        {
            var existingDefault = await context.SubjectTypes
                .FirstOrDefaultAsync(s => s.IsDefault);
            if (existingDefault != null)
            {
                existingDefault.IsDefault = false;
                context.Update(existingDefault);
            }
        }

        context.SubjectTypes.Add(subjectType);
        await context.SaveChangesAsync();
    }

    public async Task UpdateSubjectTypeAsync(SubjectType subjectType)
    {
        if (subjectType.IsDefault)
        {
            var existingDefault = await context.SubjectTypes
                .FirstOrDefaultAsync(s => s.IsDefault && s.Id != subjectType.Id);
            if (existingDefault != null)
            {
                existingDefault.IsDefault = false;
                context.Update(existingDefault);
            }
        }

        context.SubjectTypes.Update(subjectType);
        await context.SaveChangesAsync();
    }

    public async Task DeleteSubjectTypeAsync(int id)
    {
        var subjectType = await context.SubjectTypes.FindAsync(id);
        if (subjectType != null)
        {
            context.SubjectTypes.Remove(subjectType);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> SubjectTypeExistsAsync(int id)
    {
        return await context.SubjectTypes.AnyAsync(e => e.Id == id);
    }

    private static Expression<Func<SubjectType, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "name" => s => s.Name,
            "code" => s => s.Code ?? "",
            "maxallowedsubjects" => s => s.MaxAllowedSubjects,
            "isdefault" => s => s.IsDefault,
            "isactive" => s => s.IsActive,
            _ => s => s.Name
        };
    }
}
