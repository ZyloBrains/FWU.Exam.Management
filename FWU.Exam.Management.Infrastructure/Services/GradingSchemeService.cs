using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class GradingSchemeService(AppDbContext context, IUserContext userContext) : IGradingSchemeService
{
    public async Task<(List<GradingScheme> Items, int TotalCount)> GetGradingSchemesAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = BuildQuery(search, sort, sortDir);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new GradingScheme
            {
                Id = e.Id,
                Name = e.Name,
                ProgramId = e.ProgramId,
                AcademicYearId = e.AcademicYearId,
                Description = e.Description,
                IsActive = e.IsActive,
                Program = e.Program,
                AcademicYear = e.AcademicYear,
                GradeDefinitions = e.GradeDefinitions
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<GradingScheme>> GetFilteredItemsAsync(string? search)
    {
        var query = BuildQuery(search, "Id", "asc");
        return await query
            .Select(e => new GradingScheme
            {
                Id = e.Id,
                Name = e.Name,
                ProgramId = e.ProgramId,
                AcademicYearId = e.AcademicYearId,
                Description = e.Description,
                IsActive = e.IsActive,
                Program = e.Program,
                AcademicYear = e.AcademicYear,
                GradeDefinitions = e.GradeDefinitions
            })
            .ToListAsync();
    }

    public async Task<GradingScheme?> GetGradingSchemeByIdAsync(int id)
    {
        return await context.GradingSchemes
            .AsNoTracking()
            .Include(e => e.Program)
            .Include(e => e.AcademicYear)
            .Include(e => e.GradeDefinitions)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task CreateGradingSchemeAsync(GradingScheme gradingScheme)
    {
        context.GradingSchemes.Add(gradingScheme);
        await context.SaveChangesAsync();
    }

    public async Task UpdateGradingSchemeAsync(GradingScheme gradingScheme)
    {
        var existing = await context.GradingSchemes
            .Include(e => e.GradeDefinitions)
            .FirstOrDefaultAsync(e => e.Id == gradingScheme.Id);

        if (existing == null) return;

        existing.Name = gradingScheme.Name;
        existing.ProgramId = gradingScheme.ProgramId;
        existing.AcademicYearId = gradingScheme.AcademicYearId;
        existing.Description = gradingScheme.Description;
        existing.IsActive = gradingScheme.IsActive;

        if (gradingScheme.GradeDefinitions != null)
        {
            var incomingIds = gradingScheme.GradeDefinitions
                .Where(g => g.Id > 0)
                .Select(g => g.Id)
                .ToHashSet();

            var toRemove = existing.GradeDefinitions?
                .Where(g => !incomingIds.Contains(g.Id))
                .ToList() ?? [];

            foreach (var item in toRemove)
                context.GradeDefinitions.Remove(item);

            foreach (var gd in gradingScheme.GradeDefinitions)
            {
                if (gd.Id > 0 && existing.GradeDefinitions != null)
                {
                    var existingGd = existing.GradeDefinitions.FirstOrDefault(g => g.Id == gd.Id);
                    if (existingGd != null)
                    {
                        existingGd.GradeLetter = gd.GradeLetter;
                        existingGd.MinPercentage = gd.MinPercentage;
                        existingGd.MaxPercentage = gd.MaxPercentage;
                        existingGd.GradePoint = gd.GradePoint;
                        existingGd.Remark = gd.Remark;
                        existingGd.IsPass = gd.IsPass;
                        existingGd.DisplayOrder = gd.DisplayOrder;
                    }
                }
                else
                {
                    gd.GradingSchemeId = existing.Id;
                    context.GradeDefinitions.Add(gd);
                }
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteGradingSchemeAsync(int id)
    {
        var gradingScheme = await context.GradingSchemes
            .Include(e => e.GradeDefinitions)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (gradingScheme != null)
        {
            context.GradingSchemes.Remove(gradingScheme);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> GradingSchemeExistsAsync(int id)
    {
        return await context.GradingSchemes.AnyAsync(e => e.Id == id);
    }

    public GradingSchemeSelectListsDto GetSelectListData(GradingScheme? gradingScheme = null)
    {
        var programsQuery = context.Programs.AsNoTracking();
        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
                programsQuery = programsQuery.Where(p => p.FacultyId == userContext.FacultyId.Value);
        }
        var programs = programsQuery.ToList();
        var academicYears = context.AcademicYears.AsNoTracking().ToList();

        return new GradingSchemeSelectListsDto
        {
            Programs = programs.Select(p => new SelectOption { Id = p.Id, Name = p.ProgramName }).ToList(),
            AcademicYears = academicYears.Select(ay => new SelectOption { Id = ay.Id, Name = ay.AcademicYearName }).ToList()
        };
    }

    private IQueryable<GradingScheme> BuildQuery(string? search, string sort, string sortDir)
    {
        var query = context.GradingSchemes.AsNoTracking();

        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
                query = query.Where(e => e.Program != null && e.Program.FacultyId == userContext.FacultyId.Value);
        }

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e =>
                (e.Name != null && e.Name.Contains(search)) ||
                (e.Description != null && e.Description.Contains(search)) ||
                (e.Program != null && e.Program.ProgramName != null && e.Program.ProgramName.Contains(search)));
        }

        var descending = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sort.ToLower() switch
        {
            "name" => descending ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name),
            "program" => descending
                ? query.OrderByDescending(e => e.Program != null ? e.Program.ProgramName : string.Empty)
                : query.OrderBy(e => e.Program != null ? e.Program.ProgramName : string.Empty),
            "isactive" => descending ? query.OrderByDescending(e => e.IsActive) : query.OrderBy(e => e.IsActive),
            _ => descending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id)
        };
    }
}
