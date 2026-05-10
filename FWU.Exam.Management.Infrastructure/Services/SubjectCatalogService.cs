using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class SubjectCatalogService : ISubjectCatalogService
{
    private readonly AppDbContext _context;

    public SubjectCatalogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<SubjectCatalog> Items, int TotalCount)> GetSubjectCatalogsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.SubjectCatalogs
            .Include(s => s.SubjectType)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                (s.SubjectCode != null && s.SubjectCode.Contains(search)) ||
                (s.SubjectName != null && s.SubjectName.Contains(search)) ||
                (s.ShortName != null && s.ShortName.Contains(search)) ||
                (s.Description != null && s.Description.Contains(search)));
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

    public async Task<List<SubjectCatalog>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.SubjectCatalogs
            .Include(s => s.SubjectType)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                (s.SubjectCode != null && s.SubjectCode.Contains(search)) ||
                (s.SubjectName != null && s.SubjectName.Contains(search)) ||
                (s.ShortName != null && s.ShortName.Contains(search)) ||
                (s.Description != null && s.Description.Contains(search)));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<SubjectCatalog?> GetSubjectCatalogByIdAsync(int id)
    {
        return await _context.SubjectCatalogs
            .Include(s => s.SubjectType)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateSubjectCatalogAsync(SubjectCatalog subjectCatalog)
    {
        _context.SubjectCatalogs.Add(subjectCatalog);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSubjectCatalogAsync(SubjectCatalog subjectCatalog)
    {
        _context.SubjectCatalogs.Update(subjectCatalog);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSubjectCatalogAsync(int id)
    {
        var subjectCatalog = await _context.SubjectCatalogs.FindAsync(id);
        if (subjectCatalog != null)
        {
            _context.SubjectCatalogs.Remove(subjectCatalog);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> SubjectCatalogExistsAsync(int id)
    {
        return await _context.SubjectCatalogs.AnyAsync(e => e.Id == id);
    }

    public async Task<List<SubjectType>> GetSelectListsAsync(int? subjectTypeId = null)
    {
        return await _context.SubjectTypes
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    private static Expression<Func<SubjectCatalog, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "code" => s => s.SubjectCode ?? "",
            "subjectcode" => s => s.SubjectCode ?? "",
            "name" => s => s.SubjectName ?? "",
            "subjectname" => s => s.SubjectName ?? "",
            "shortname" => s => s.ShortName ?? "",
            "credithours" => s => s.CreditHours ?? 0,
            "subjecttype" => s => s.SubjectType != null ? s.SubjectType.Name : "",
            "isactive" => s => s.IsActive,
            _ => s => s.SubjectName ?? ""
        };
    }
}
