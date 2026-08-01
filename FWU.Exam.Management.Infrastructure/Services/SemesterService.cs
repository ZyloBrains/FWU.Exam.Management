using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class SemesterService(AppDbContext context) : ISemesterService
{
    public async Task<(List<Semester> Items, int TotalCount)> GetSemestersAsync(int page, int pageSize, string? search, string sort, string sortDir, IUserContext userContext)
    {
        var query = context.Semesters.AsNoTracking().ApplyScope(userContext);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.Name!.Contains(search) ||
                s.Code!.Contains(search) ||
                s.Remark!.Contains(search));
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

    public async Task<List<Semester>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir, IUserContext userContext)
    {
        var query = context.Semesters.AsNoTracking().ApplyScope(userContext);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.Name!.Contains(search) ||
                s.Code!.Contains(search) ||
                s.Remark!.Contains(search));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<List<Semester>> GetSemestersByFacultyAsync(int? facultyId)
    {
        if (facultyId == null) return new List<Semester>();
        return await context.Semesters
            .AsNoTracking()
            .Where(s => s.FacultyId == facultyId.Value)
            .OrderBy(s => s.Number)
            .ToListAsync();
    }

    public async Task<List<Semester>> GetSemestersByProgramAsync(int programId)
    {
        return await context.ProgramSemesters
            .AsNoTracking()
            .Where(ps => ps.ProgramId == programId && ps.IsActive)
            .Select(ps => ps.Semester!)
            .OrderBy(s => s.Number)
            .ToListAsync();
    }

    public async Task<List<int>> GetAssignedSemesterIdsAsync(int programId)
    {
        return await context.ProgramSemesters
            .AsNoTracking()
            .Where(ps => ps.ProgramId == programId)
            .Select(ps => ps.SemesterId)
            .ToListAsync();
    }

    public async Task<List<Semester>> GetAssignableSemestersAsync(IUserContext userContext)
    {
        return await context.Semesters
            .AsNoTracking()
            .Include(s => s.AcademicYear)
            .Include(s => s.Faculty)
            .ApplyScope(userContext)
            .OrderBy(s => s.AcademicYearId)
            .ThenBy(s => s.Number)
            .ToListAsync();
    }

    public async Task SetProgramSemestersAsync(int programId, IEnumerable<int> semesterIds)
    {
        var ids = (semesterIds ?? Enumerable.Empty<int>()).Distinct().ToList();
        var existing = await context.ProgramSemesters
            .Where(ps => ps.ProgramId == programId)
            .ToListAsync();

        var existingIds = existing.Select(ps => ps.SemesterId).ToHashSet();
        var toAdd = ids.Where(id => !existingIds.Contains(id)).ToList();
        var toRemove = existing.Where(ps => !ids.Contains(ps.SemesterId)).ToList();

        if (toRemove.Count > 0)
            context.ProgramSemesters.RemoveRange(toRemove);

        foreach (var semesterId in toAdd)
        {
            context.ProgramSemesters.Add(new ProgramSemester
            {
                ProgramId = programId,
                SemesterId = semesterId,
                IsActive = true,
                DisplayOrder = 0
            });
        }

        if (toAdd.Count > 0 || toRemove.Count > 0)
            await context.SaveChangesAsync();
    }

    public async Task<bool> IsSemesterAssignedToProgramAsync(int programId, int semesterId)
    {
        return await context.ProgramSemesters
            .AsNoTracking()
            .AnyAsync(ps => ps.ProgramId == programId && ps.SemesterId == semesterId && ps.IsActive);
    }

    public async Task<bool> IsSemesterAssignedToAnyProgramAsync(int semesterId)
    {
        return await context.ProgramSemesters
            .AsNoTracking()
            .AnyAsync(ps => ps.SemesterId == semesterId);
    }

    public async Task AutoLinkProgramSemestersAsync()
    {
        var programs = await context.Programs
            .AsNoTracking()
            .Where(p => p.FacultyId.HasValue && p.IsActive)
            .Select(p => new { p.Id, p.FacultyId })
            .ToListAsync();

        var existing = await context.ProgramSemesters
            .AsNoTracking()
            .Select(ps => new { ps.ProgramId, ps.SemesterId })
            .ToListAsync();
        var existingSet = existing
            .Select(x => (x.ProgramId, x.SemesterId))
            .ToHashSet();

        var semestersByFaculty = await context.Semesters
            .AsNoTracking()
            .Where(s => s.FacultyId.HasValue)
            .GroupBy(s => s.FacultyId!.Value)
            .ToDictionaryAsync(g => g.Key, g => g.Select(s => s.Id).ToList());

        var toAdd = new List<ProgramSemester>();
        foreach (var program in programs)
        {
            if (!program.FacultyId.HasValue) continue;
            if (!semestersByFaculty.TryGetValue(program.FacultyId.Value, out var semesterIds)) continue;
            foreach (var semesterId in semesterIds)
            {
                if (existingSet.Contains((program.Id, semesterId))) continue;
                toAdd.Add(new ProgramSemester
                {
                    ProgramId = program.Id,
                    SemesterId = semesterId,
                    IsActive = true,
                    DisplayOrder = 0
                });
                existingSet.Add((program.Id, semesterId));
            }
        }

        if (toAdd.Count > 0)
        {
            await context.ProgramSemesters.AddRangeAsync(toAdd);
            await context.SaveChangesAsync();
        }
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
