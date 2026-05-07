using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class CurriculumVersionService : ICurriculumVersionService
{
    private readonly AppDbContext _context;

    public CurriculumVersionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<CurriculumVersion> Items, int TotalCount)> GetCurriculumVersionsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.CurriculumVersions
            .Include(c => c.Program)
            .Include(c => c.EffectiveAcademicYear)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c =>
                (c.Name != null && c.Name.Contains(search)) ||
                (c.Description != null && c.Description.Contains(search)));
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

    public async Task<List<CurriculumVersion>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.CurriculumVersions
            .Include(c => c.Program)
            .Include(c => c.EffectiveAcademicYear)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c =>
                (c.Name != null && c.Name.Contains(search)) ||
                (c.Description != null && c.Description.Contains(search)));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<CurriculumVersion?> GetCurriculumVersionByIdAsync(int id)
    {
        return await _context.CurriculumVersions
            .Include(c => c.Program)
            .Include(c => c.EffectiveAcademicYear)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateCurriculumVersionAsync(CurriculumVersion curriculumVersion)
    {
        _context.CurriculumVersions.Add(curriculumVersion);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCurriculumVersionAsync(CurriculumVersion curriculumVersion)
    {
        _context.CurriculumVersions.Update(curriculumVersion);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCurriculumVersionAsync(int id)
    {
        var curriculumVersion = await _context.CurriculumVersions.FindAsync(id);
        if (curriculumVersion != null)
        {
            _context.CurriculumVersions.Remove(curriculumVersion);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> CurriculumVersionExistsAsync(int id)
    {
        return await _context.CurriculumVersions.AnyAsync(e => e.Id == id);
    }

    public async Task<(List<Program> Programs, List<AcademicYear> AcademicYears)> GetSelectListsAsync(int? programId = null, int? academicYearId = null)
    {
        var programs = await _context.Programs
            .Where(p => p.IsActive)
            .OrderBy(p => p.ProgramName)
            .AsNoTracking()
            .ToListAsync();

        var academicYears = await _context.AcademicYears
            .Where(a => a.IsActive)
            .OrderByDescending(a => a.Id)
            .AsNoTracking()
            .ToListAsync();

        return (programs, academicYears);
    }

    private static Expression<Func<CurriculumVersion, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "name" => c => c.Name ?? "",
            "program" => c => c.Program != null ? c.Program.ProgramName : "",
            "academicyear" => c => c.EffectiveAcademicYear != null ? c.EffectiveAcademicYear.AcademicYearName : "",
            "isactive" => c => c.IsActive,
            _ => c => c.Name ?? ""
        };
    }
}
