using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class SubjectTypeService : ISubjectTypeService
{
    private readonly AppDbContext _context;

    public SubjectTypeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<SubjectType> Items, int TotalCount)> GetSubjectTypesAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.SubjectTypes.AsNoTracking();

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
        var query = _context.SubjectTypes.AsNoTracking();

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
        return await _context.SubjectTypes.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateSubjectTypeAsync(SubjectType subjectType)
    {
        if (subjectType.IsDefault)
        {
            var existingDefault = await _context.SubjectTypes
                .FirstOrDefaultAsync(s => s.IsDefault);
            if (existingDefault != null)
            {
                existingDefault.IsDefault = false;
                _context.Update(existingDefault);
            }
        }

        _context.SubjectTypes.Add(subjectType);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSubjectTypeAsync(SubjectType subjectType)
    {
        if (subjectType.IsDefault)
        {
            var existingDefault = await _context.SubjectTypes
                .FirstOrDefaultAsync(s => s.IsDefault && s.Id != subjectType.Id);
            if (existingDefault != null)
            {
                existingDefault.IsDefault = false;
                _context.Update(existingDefault);
            }
        }

        _context.SubjectTypes.Update(subjectType);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSubjectTypeAsync(int id)
    {
        var subjectType = await _context.SubjectTypes.FindAsync(id);
        if (subjectType != null)
        {
            _context.SubjectTypes.Remove(subjectType);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> SubjectTypeExistsAsync(int id)
    {
        return await _context.SubjectTypes.AnyAsync(e => e.Id == id);
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
