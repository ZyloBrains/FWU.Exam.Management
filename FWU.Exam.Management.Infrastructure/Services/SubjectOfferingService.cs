using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class SubjectOfferingService : ISubjectOfferingService
{
    private readonly AppDbContext _context;

    public SubjectOfferingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<SubjectOffering> Items, int TotalCount)> GetSubjectOfferingsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? facultyId = null)
    {
        var query = _context.SubjectOfferings
            .Include(s => s.SubjectCatalog)
            .Include(s => s.Program)
            .Include(s => s.Semester)
            .AsNoTracking();

        if (facultyId.HasValue)
        {
            query = query.Where(s => s.Program != null && s.Program.Department != null && s.Program.Department.FacultyId == facultyId.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                (s.SubjectCatalog != null && s.SubjectCatalog.SubjectName != null && s.SubjectCatalog.SubjectName.Contains(search)) ||
                (s.SubjectCatalog != null && s.SubjectCatalog.SubjectCode != null && s.SubjectCatalog.SubjectCode.Contains(search)) ||
                (s.Program != null && s.Program.ProgramName != null && s.Program.ProgramName.Contains(search)));
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

    public async Task<List<SubjectOffering>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? facultyId = null)
    {
        var query = _context.SubjectOfferings
            .Include(s => s.SubjectCatalog)
            .Include(s => s.Program)
            .Include(s => s.Semester)
            .AsNoTracking();

        if (facultyId.HasValue)
        {
            query = query.Where(s => s.Program != null && s.Program.Department != null && s.Program.Department.FacultyId == facultyId.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                (s.SubjectCatalog != null && s.SubjectCatalog.SubjectName != null && s.SubjectCatalog.SubjectName.Contains(search)) ||
                (s.SubjectCatalog != null && s.SubjectCatalog.SubjectCode != null && s.SubjectCatalog.SubjectCode.Contains(search)) ||
                (s.Program != null && s.Program.ProgramName != null && s.Program.ProgramName.Contains(search)));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<SubjectOffering?> GetSubjectOfferingByIdAsync(int id)
    {
        return await _context.SubjectOfferings
            .Include(s => s.SubjectCatalog)
            .Include(s => s.Program)
            .Include(s => s.Semester)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateSubjectOfferingAsync(SubjectOffering subjectOffering)
    {
        _context.SubjectOfferings.Add(subjectOffering);
        await _context.SaveChangesAsync();
    }

    public async Task CreateSubjectOfferingsAsync(List<SubjectOffering> subjectOfferings)
    {
        _context.SubjectOfferings.AddRange(subjectOfferings);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSubjectOfferingAsync(SubjectOffering subjectOffering)
    {
        _context.SubjectOfferings.Update(subjectOffering);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSubjectOfferingAsync(int id)
    {
        var subjectOffering = await _context.SubjectOfferings.FindAsync(id);
        if (subjectOffering != null)
        {
            _context.SubjectOfferings.Remove(subjectOffering);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> SubjectOfferingExistsAsync(int id)
    {
        return await _context.SubjectOfferings.AnyAsync(e => e.Id == id);
    }

    public async Task<List<int>> GetExistingSubjectCatalogIdsAsync(int programId)
    {
        return await _context.SubjectOfferings
            .Where(so => so.ProgramId == programId)
            .Select(so => so.SubjectCatalogId)
            .ToListAsync();
    }

    public async Task<(List<SubjectCatalog> SubjectCatalogs, List<Program> Programs, List<Semester> Semesters)> GetSelectListsAsync(int? subjectCatalogId = null, int? programId = null, int? semesterId = null, int? facultyId = null)
    {
        var subjectCatalogs = await _context.SubjectCatalogs
            .Where(s => s.IsActive)
            .OrderBy(s => s.SubjectCode)
            .AsNoTracking()
            .ToListAsync();

        var programsQuery = _context.Programs
            .Where(p => p.IsActive);

        if (facultyId.HasValue)
        {
            programsQuery = programsQuery.Where(p => p.Department != null && p.Department.FacultyId == facultyId.Value);
        }

        var programs = await programsQuery
            .OrderBy(p => p.ProgramName)
            .AsNoTracking()
            .ToListAsync();

        var semesters = await _context.Semesters
            .OrderByDescending(s => s.Id)
            .AsNoTracking()
            .ToListAsync();

        return (subjectCatalogs, programs, semesters);
    }

    private static Expression<Func<SubjectOffering, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "subject" => s => s.SubjectCatalog != null ? s.SubjectCatalog.SubjectName : "",
            "subjectcatalog" => s => s.SubjectCatalog != null ? s.SubjectCatalog.SubjectName : "",
            "program" => s => s.Program != null ? s.Program.ProgramName : "",
            "semester" => s => s.Semester != null ? s.Semester.Name : "",
            "displayorder" => s => s.DisplayOrder,
            "iscompulsory" => s => s.IsCompulsory,
            _ => s => s.SubjectCatalog != null ? s.SubjectCatalog.SubjectCode : ""
        };
    }
}
