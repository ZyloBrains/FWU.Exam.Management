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
            .Include(e => e.ProgramAssignments)
                .ThenInclude(gsp => gsp.Program)
            .Include(e => e.ProgramAssignments)
                .ThenInclude(gsp => gsp.AcademicYear)
            .Include(e => e.GradeDefinitions)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<GradingScheme>> GetFilteredItemsAsync(string? search)
    {
        var query = BuildQuery(search, "Id", "asc");
        return await query
            .Include(e => e.ProgramAssignments)
                .ThenInclude(gsp => gsp.Program)
            .Include(e => e.ProgramAssignments)
                .ThenInclude(gsp => gsp.AcademicYear)
            .Include(e => e.GradeDefinitions)
            .ToListAsync();
    }

    public async Task<GradingScheme?> GetGradingSchemeByIdAsync(int id)
    {
        return await context.GradingSchemes
            .AsNoTracking()
            .Include(e => e.ProgramAssignments)
                .ThenInclude(gsp => gsp.Program)
            .Include(e => e.ProgramAssignments)
                .ThenInclude(gsp => gsp.AcademicYear)
            .Include(e => e.GradeDefinitions)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task CreateGradingSchemeAsync(GradingScheme gradingScheme, List<int> programIds, Dictionary<int, int?> programAcademicYears)
    {
        context.GradingSchemes.Add(gradingScheme);
        await context.SaveChangesAsync();

        foreach (var programId in programIds)
        {
            context.GradingSchemePrograms.Add(new GradingSchemeProgram
            {
                GradingSchemeId = gradingScheme.Id,
                ProgramId = programId,
                AcademicYearId = programAcademicYears.TryGetValue(programId, out var ayId) ? ayId : null,
                IsActive = true
            });
        }
        await context.SaveChangesAsync();
    }

    public async Task UpdateGradingSchemeAsync(GradingScheme gradingScheme, List<int> programIds, Dictionary<int, int?> programAcademicYears)
    {
        var existing = await context.GradingSchemes
            .Include(e => e.GradeDefinitions)
            .Include(e => e.ProgramAssignments)
            .FirstOrDefaultAsync(e => e.Id == gradingScheme.Id);

        if (existing == null) return;

        existing.Name = gradingScheme.Name;
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

        var existingAssignments = existing.ProgramAssignments?.ToList() ?? [];
        var incomingProgramIds = programIds.ToHashSet();

        var assignmentsToRemove = existingAssignments
            .Where(a => !incomingProgramIds.Contains(a.ProgramId))
            .ToList();
        foreach (var a in assignmentsToRemove)
            context.GradingSchemePrograms.Remove(a);

        foreach (var programId in programIds)
        {
            var existingAssignment = existingAssignments.FirstOrDefault(a => a.ProgramId == programId);
            if (existingAssignment != null)
            {
                existingAssignment.AcademicYearId = programAcademicYears.TryGetValue(programId, out var ayId) ? ayId : null;
                existingAssignment.IsActive = true;
            }
            else
            {
                context.GradingSchemePrograms.Add(new GradingSchemeProgram
                {
                    GradingSchemeId = existing.Id,
                    ProgramId = programId,
                    AcademicYearId = programAcademicYears.TryGetValue(programId, out var newAyId) ? newAyId : null,
                    IsActive = true
                });
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteGradingSchemeAsync(int id)
    {
        var gradingScheme = await context.GradingSchemes
            .Include(e => e.GradeDefinitions)
            .Include(e => e.ProgramAssignments)
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

    public async Task<GradingSchemeSelectListsDto> GetSelectListDataAsync(GradingScheme? gradingScheme = null)
    {
        var programsQuery = context.Programs.AsNoTracking();
        if (!userContext.IsSuperAdmin)
        {
            if (userContext.IsFacultyAdmin && userContext.FacultyId.HasValue)
                programsQuery = programsQuery.Where(p => p.FacultyId == userContext.FacultyId.Value);
        }
        var programs = await programsQuery.ToListAsync();
        var academicYears = await context.AcademicYears.AsNoTracking().ToListAsync();

        return new GradingSchemeSelectListsDto
        {
            Programs = programs.Select(p => new SelectOption { Id = p.Id, Name = p.ProgramName }).ToList(),
            AcademicYears = academicYears.Select(ay => new SelectOption { Id = ay.Id, Name = ay.AcademicYearName }).ToList()
        };
    }

    private IQueryable<GradingScheme> BuildQuery(string? search, string sort, string sortDir)
    {
        var query = context.GradingSchemes.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e =>
                (e.Name != null && e.Name.Contains(search)) ||
                (e.Description != null && e.Description.Contains(search)));
        }

        var descending = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sort.ToLower() switch
        {
            "name" => descending ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name),
            "isactive" => descending ? query.OrderByDescending(e => e.IsActive) : query.OrderBy(e => e.IsActive),
            _ => descending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id)
        };
    }
}
