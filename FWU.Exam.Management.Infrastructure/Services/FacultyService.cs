using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class FacultyService : IFacultyService
{
    private readonly AppDbContext _context;

    public FacultyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Faculty> Items, int TotalCount)> GetFacultiesAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.Faculties.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(f =>
                f.FacultyCode.Contains(search) ||
                f.FacultyName.Contains(search) ||
                (f.ShortName != null && f.ShortName.Contains(search)) ||
                (f.Remarks != null && f.Remarks.Contains(search)));
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

    public async Task<List<Faculty>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.Faculties.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(f =>
                f.FacultyCode.Contains(search) ||
                f.FacultyName.Contains(search) ||
                (f.ShortName != null && f.ShortName.Contains(search)) ||
                (f.Remarks != null && f.Remarks.Contains(search)));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<Faculty?> GetFacultyByIdAsync(int id)
    {
        return await _context.Faculties.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateFacultyAsync(Faculty faculty)
    {
        _context.Faculties.Add(faculty);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateFacultyAsync(Faculty faculty)
    {
        _context.Faculties.Update(faculty);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteFacultyAsync(int id)
    {
        var faculty = await _context.Faculties.FindAsync(id);
        if (faculty != null)
        {
            _context.Faculties.Remove(faculty);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> FacultyExistsAsync(int id)
    {
        return await _context.Faculties.AnyAsync(e => e.Id == id);
    }

    private static Expression<Func<Faculty, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "facultycode" => f => f.FacultyCode,
            "facultyname" => f => f.FacultyName,
            "shortname" => f => f.ShortName ?? "",
            "remarks" => f => f.Remarks ?? "",
            "isactive" => f => f.IsActive,
            _ => f => f.FacultyName
        };
    }
}
