using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class CollegeProgramService(AppDbContext context, IUserContext userContext) : ICollegeProgramService
{
    /// <summary>
    /// Paged over colleges rather than CollegeProgram rows. A college is included when
    /// any one of its programs matches the search term, and when included it carries all
    /// of its programs so a group is never broken up by a page boundary.
    /// </summary>
    public async Task<(List<CollegeProgramGroupDto> Colleges, int TotalCollegeCount, int TotalProgramCount)> GetPagedCollegesWithProgramsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var matching = BuildQuery(search).ApplyScope(userContext);

        // One row per college, carrying the college fields needed for sorting.
        var collegeGroups = matching
            .Where(cp => cp.College != null)
            .GroupBy(cp => new { cp.CollegeId, cp.College!.Name, cp.College!.Code })
            .Select(g => new
            {
                g.Key.CollegeId,
                g.Key.Name,
                g.Key.Code,
                ProgramCount = g.Count(),
                ActiveProgramCount = g.Count(cp => cp.IsActive),
                TotalStudents = g.Sum(cp => cp.NumberOfStudents)
            });

        var totalCollegeCount = await collegeGroups.CountAsync();
        var totalProgramCount = await matching.CountAsync();

        var descending = sortDir.ToLower() == "desc";
        var ordered = sort.ToLower() switch
        {
            "collegecode" => descending
                ? collegeGroups.OrderByDescending(c => c.Code)
                : collegeGroups.OrderBy(c => c.Code),
            "programcount" => descending
                ? collegeGroups.OrderByDescending(c => c.ProgramCount).ThenBy(c => c.Name)
                : collegeGroups.OrderBy(c => c.ProgramCount).ThenBy(c => c.Name),
            "activemembers" => descending
                ? collegeGroups.OrderByDescending(c => c.ActiveProgramCount).ThenBy(c => c.Name)
                : collegeGroups.OrderBy(c => c.ActiveProgramCount).ThenBy(c => c.Name),
            "studentcount" => descending
                ? collegeGroups.OrderByDescending(c => c.TotalStudents).ThenBy(c => c.Name)
                : collegeGroups.OrderBy(c => c.TotalStudents).ThenBy(c => c.Name),
            _ => descending
                ? collegeGroups.OrderByDescending(c => c.Name)
                : collegeGroups.OrderBy(c => c.Name)
        };

        var page_ = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (page_.Count == 0)
        {
            return ([], totalCollegeCount, totalProgramCount);
        }

        var pageIds = page_.Select(c => c.CollegeId).ToList();

        // Deliberately unfiltered: a matched college shows its complete program list.
        var rows = await BuildQuery(search: null)
            .ApplyScope(userContext)
            .Where(cp => pageIds.Contains(cp.CollegeId))
            .OrderBy(cp => cp.Program!.ProgramName)
            .ToListAsync();

        var byCollege = rows
            .GroupBy(cp => cp.CollegeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = page_.Select(c =>
        {
            var programs = byCollege.TryGetValue(c.CollegeId, out var list) ? list : [];

            return new CollegeProgramGroupDto
            {
                CollegeId = c.CollegeId,
                CollegeName = c.Name,
                CollegeCode = c.Code,
                // Counts describe every program on the page for this college, which is
                // what the user actually sees under the header.
                ProgramCount = programs.Count,
                ActiveProgramCount = programs.Count(p => p.IsActive),
                TotalStudents = programs.Sum(p => p.NumberOfStudents),
                Programs = programs
            };
        }).ToList();

        return (result, totalCollegeCount, totalProgramCount);
    }

    /// <summary>
    /// Every row matching the search, ignoring pagination, so exports match what the
    /// user is filtering on rather than only the page currently on screen.
    /// </summary>
    public async Task<(List<CollegeProgram> Items, int TotalCount)> GetFilteredItemsForExportAsync(string? search, string sort, string sortDir)
    {
        var query = BuildQuery(search).ApplyScope(userContext);

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        var items = await query.ToListAsync();

        return (items, items.Count);
    }

    public async Task<CollegeProgram?> GetCollegeProgramByIdAsync(int id)
    {
        return await context.CollegePrograms
            .Include(cp => cp.College)
            .Include(cp => cp.Program)
            .AsNoTracking()
            .FirstOrDefaultAsync(cp => cp.Id == id);
    }

    public async Task CreateCollegeProgramAsync(CollegeProgram collegeProgram)
    {
        collegeProgram.TenantId = AppDbContext.GetCurrentTenantId();
        context.CollegePrograms.Add(collegeProgram);
        await context.SaveChangesAsync();
    }

    public async Task CreateCollegeProgramsAsync(List<CollegeProgram> collegePrograms)
    {
        var tenantId = AppDbContext.GetCurrentTenantId();

        foreach (var collegeProgram in collegePrograms)
        {
            collegeProgram.TenantId = tenantId;
        }

        context.CollegePrograms.AddRange(collegePrograms);
        await context.SaveChangesAsync();
    }

    public async Task<List<int>> GetExistingProgramIdsAsync(int collegeId)
    {
        return await context.CollegePrograms
            .Where(cp => cp.CollegeId == collegeId)
            .Select(cp => cp.ProgramId)
            .ToListAsync();
    }

    public async Task UpdateCollegeProgramAsync(CollegeProgram collegeProgram)
    {
        // The edit form does not post TenantId, so preserve it from the stored row.
        // Without this the update writes TenantId = 0 and fails the tenant foreign key.
        var existing = await context.CollegePrograms
            .AsNoTracking()
            .Where(cp => cp.Id == collegeProgram.Id)
            .Select(cp => new { cp.TenantId })
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            collegeProgram.TenantId = existing.TenantId;
        }

        context.CollegePrograms.Update(collegeProgram);
        await context.SaveChangesAsync();
    }

    public async Task DeleteCollegeProgramAsync(int id)
    {
        var collegeProgram = await context.CollegePrograms.FindAsync(id);
        if (collegeProgram != null)
        {
            context.CollegePrograms.Remove(collegeProgram);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> CollegeProgramExistsAsync(int id)
    {
        return await context.CollegePrograms.AnyAsync(cp => cp.Id == id);
    }

    public async Task<(List<College> Colleges, List<Program> Programs)> GetSelectListsAsync()
    {
        var colleges = await context.Colleges.ApplyScope(userContext).AsNoTracking().ToListAsync();
        var programs = await context.Programs.ApplyScope(userContext).AsNoTracking().ToListAsync();

        return (colleges, programs);
    }

    public async Task<List<College>> GetCollegesWithoutProgramsAsync()
    {
        var collegesWithPrograms = context.CollegePrograms
            .Select(cp => cp.CollegeId)
            .Distinct();

        return await context.Colleges
            .ApplyScope(userContext)
            .Where(c => !collegesWithPrograms.Contains(c.Id))
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    private IQueryable<CollegeProgram> BuildQuery(string? search)
    {
        var query = context.CollegePrograms
            .Include(cp => cp.College)
            .Include(cp => cp.Program)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(cp =>
                cp.College!.Code.ToString().Contains(search) ||
                cp.College!.Name.Contains(search) ||
                cp.Program!.ProgramCode.Contains(search) ||
                cp.Program!.ProgramName.Contains(search) ||
                (cp.Remarks ?? "").Contains(search) ||
                cp.NumberOfStudents.ToString().Contains(search));
        }

        return query;
    }

    private static Expression<Func<CollegeProgram, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "collegecode" => cp => cp.College != null ? cp.College.Code : "",
            "collegename" => cp => cp.College != null ? cp.College.Name : "",
            "programcode" => cp => cp.Program != null ? cp.Program.ProgramCode : "",
            "programname" => cp => cp.Program != null ? cp.Program.ProgramName : "",
            "affiliationdate" => cp => cp.AffiliationDate ?? DateTime.MinValue,
            "numberofstudents" => cp => cp.NumberOfStudents,
            "isactive" => cp => cp.IsActive,
            "remarks" => cp => cp.Remarks ?? "",
            _ => cp => cp.Id
        };
    }
}
