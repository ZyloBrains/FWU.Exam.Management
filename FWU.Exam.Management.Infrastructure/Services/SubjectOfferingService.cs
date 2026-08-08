using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class SubjectOfferingService : ISubjectOfferingService
{
    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;

    public SubjectOfferingService(AppDbContext context, IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<(List<SubjectOffering> Items, int TotalProgramCount)> GetSubjectOfferingsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.SubjectOfferings
            .Include(s => s.SubjectCatalog)
            .Include(s => s.Program)
            .Include(s => s.Semester)
            .AsNoTracking();
        query = query.ApplyScope(_userContext);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                (s.SubjectCatalog != null && s.SubjectCatalog.SubjectName != null && s.SubjectCatalog.SubjectName.Contains(search)) ||
                (s.SubjectCatalog != null && s.SubjectCatalog.SubjectCode != null && s.SubjectCatalog.SubjectCode.Contains(search)) ||
                (s.Program != null && s.Program.ProgramName != null && s.Program.ProgramName.Contains(search)));
        }

        var matchingProgramIds = await query
            .Select(s => s.ProgramId)
            .Distinct()
            .ToListAsync();

        var programs = await _context.Programs
            .Where(p => matchingProgramIds.Contains(p.Id))
            .Select(p => new { p.Id, p.ProgramName })
            .ToListAsync();

        var sortedPrograms = sortDir.ToLower() == "desc"
            ? programs.OrderByDescending(p => p.ProgramName).ToList()
            : programs.OrderBy(p => p.ProgramName).ToList();

        var totalProgramCount = sortedPrograms.Count;
        var pagedProgramIds = sortedPrograms
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => p.Id)
            .ToList();

        var items = await query
            .Where(s => pagedProgramIds.Contains(s.ProgramId))
            .OrderBy(s => s.Program!.ProgramName)
            .ThenBy(s => s.Semester!.Number)
            .ThenBy(s => s.DisplayOrder)
            .ToListAsync();

        return (items, totalProgramCount);
    }

    public async Task<List<SubjectOffering>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.SubjectOfferings
            .Include(s => s.SubjectCatalog)
            .Include(s => s.Program)
            .Include(s => s.Semester)
            .AsNoTracking();
        query = query.ApplyScope(_userContext);

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

    public async Task<List<int>> GetExistingSubjectCatalogIdsAsync(int programId, int semesterId, int? curriculumVersionId = null)
    {
        return await _context.SubjectOfferings
            .Where(so => so.ProgramId == programId
                         && so.SemesterId == semesterId
                         && (curriculumVersionId == null || so.CurriculumVersionId == curriculumVersionId))
            .Select(so => so.SubjectCatalogId)
            .ToListAsync();
    }

    public async Task<Dictionary<int, List<int>>> GetExistingSubjectCatalogIdsBySemesterAsync(int programId, int curriculumVersionId, int academicYearId)
    {
        return await _context.SubjectOfferings
            .AsNoTracking()
            .Where(so => so.ProgramId == programId
                         && so.CurriculumVersionId == curriculumVersionId
                         && so.Semester != null
                         && so.Semester.AcademicYearId == academicYearId)
            .GroupBy(so => so.SemesterId)
            .Select(g => new
            {
                SemesterId = g.Key,
                SubjectCatalogIds = g.Select(so => so.SubjectCatalogId).Distinct().ToList()
            })
            .ToDictionaryAsync(x => x.SemesterId, x => x.SubjectCatalogIds);
    }

    public async Task<List<SelectOption>> GetAcademicYearsAsync()
    {
        return await _context.AcademicYears
            .Where(ay => ay.IsActive)
            .OrderByDescending(ay => ay.Id)
            .AsNoTracking()
            .Select(ay => new SelectOption { Id = ay.Id, Name = ay.AcademicYearName })
            .ToListAsync();
    }

    public async Task<List<SelectOption>> GetSemestersByAcademicYearAsync(int academicYearId, int? programId = null)
    {
        var cap = 8;
        if (programId is > 0)
        {
            var duration = await _context.Programs.AsNoTracking()
                .Where(p => p.Id == programId.Value)
                .Select(p => (int?)p.Duration)
                .FirstOrDefaultAsync();
            if (duration is > 0) cap = duration.Value;
        }

        return await _context.Semesters
            .AsNoTracking()
            .ApplyScope(_userContext)
            .Where(s => s.AcademicYearId == academicYearId && s.Number <= cap)
            .OrderBy(s => s.Number)
            .Select(s => new SelectOption
            {
                Id = s.Id,
                Name = s.Name + " (" + s.Code + " - " + s.AcademicYear!.AcademicYearName + ")"
            })
            .ToListAsync();
    }

    public async Task<List<ProgramOfferingSummary>> GetProgramsByAcademicYearAsync(int academicYearId)
    {
        var groups = await _context.SubjectOfferings
            .AsNoTracking()
            .ApplyScope(_userContext)
            .Where(so => so.Semester != null && so.Semester.AcademicYearId == academicYearId)
            .GroupBy(so => so.ProgramId)
            .Select(g => new
            {
                ProgramId = g.Key,
                SemesterCount = g.Select(x => x.SemesterId).Distinct().Count(),
                SubjectCount = g.Count()
            })
            .ToListAsync();

        var programIds = groups.Select(g => g.ProgramId).ToList();
        var programs = await _context.Programs
            .AsNoTracking()
            .Where(p => programIds.Contains(p.Id))
            .Select(p => new { p.Id, p.ProgramName })
            .ToListAsync();

        return groups
            .Select(g => new ProgramOfferingSummary
            {
                ProgramId = g.ProgramId,
                ProgramName = programs.FirstOrDefault(p => p.Id == g.ProgramId)?.ProgramName ?? "Program",
                SemesterCount = g.SemesterCount,
                SubjectCount = g.SubjectCount
            })
            .OrderBy(p => p.ProgramName)
            .ToList();
    }

    public async Task<List<SemesterOfferingSummary>> GetSemestersByProgramAsync(int programId, int academicYearId)
    {
        var assignedSemesterIds = _context.ProgramSemesters
            .AsNoTracking()
            .Where(ps => ps.ProgramId == programId && ps.IsActive)
            .Select(ps => ps.SemesterId);

        return await _context.Semesters
            .AsNoTracking()
            .ApplyScope(_userContext)
            .Where(s => assignedSemesterIds.Contains(s.Id) && s.AcademicYearId == academicYearId)
            .OrderBy(s => s.Number)
            .Select(s => new SemesterOfferingSummary
            {
                SemesterId = s.Id,
                SemesterNumber = s.Number,
                SemesterName = s.Name!,
                SubjectCount = _context.SubjectOfferings.Count(so => so.ProgramId == programId && so.SemesterId == s.Id)
            })
            .ToListAsync();
    }

    public async Task<List<SemesterOfferingSummary>> GetSemestersForOfferingAsync(int programId, int academicYearId)
    {
        var assigned = await GetSemestersByProgramAsync(programId, academicYearId);
        if (assigned.Count > 0) return assigned;

        var duration = await _context.Programs.AsNoTracking()
            .Where(p => p.Id == programId)
            .Select(p => (int?)p.Duration)
            .FirstOrDefaultAsync();
        var cap = duration is > 0 ? duration.Value : 8;

        return await _context.Semesters
            .AsNoTracking()
            .ApplyScope(_userContext)
            .Where(s => s.AcademicYearId == academicYearId && s.Number <= cap)
            .OrderBy(s => s.Number)
            .Select(s => new SemesterOfferingSummary
            {
                SemesterId = s.Id,
                SemesterNumber = s.Number,
                SemesterName = s.Name!,
                SubjectCount = _context.SubjectOfferings.Count(so => so.ProgramId == programId && so.SemesterId == s.Id)
            })
            .ToListAsync();
    }

    public async Task EnsureSemesterAssignedToProgramAsync(int programId, int semesterId)
    {
        var existing = await _context.ProgramSemesters
            .FirstOrDefaultAsync(ps => ps.ProgramId == programId && ps.SemesterId == semesterId);

        if (existing == null)
        {
            _context.ProgramSemesters.Add(new ProgramSemester
            {
                ProgramId = programId,
                SemesterId = semesterId,
                IsActive = true,
                DisplayOrder = 0
            });
        }
        else if (!existing.IsActive)
        {
            existing.IsActive = true;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<CurriculumVersion?> GetOrCreateDefaultCurriculumVersionAsync(int programId, int academicYearId)
    {
        var existing = await _context.CurriculumVersions
            .AsNoTracking()
            .Where(cv => cv.ProgramId == programId && cv.EffectiveAcademicYearId == academicYearId)
            .OrderByDescending(cv => cv.IsActive)
            .ThenByDescending(cv => cv.Id)
            .FirstOrDefaultAsync();
        if (existing != null) return existing;

        var program = await _context.Programs.AsNoTracking().FirstOrDefaultAsync(p => p.Id == programId);
        var year = await _context.AcademicYears.AsNoTracking().FirstOrDefaultAsync(a => a.Id == academicYearId);
        if (program == null || year == null) return null;

        var version = new CurriculumVersion
        {
            Name = $"Default - {program.ProgramName} ({year.AcademicYearName})",
            ProgramId = programId,
            EffectiveAcademicYearId = academicYearId,
            Description = "Auto-created curriculum version for subject offerings.",
            IsActive = true
        };
        _context.CurriculumVersions.Add(version);
        await _context.SaveChangesAsync();
        return version;
    }

    public async Task<List<SubjectOffering>> GetSubjectOfferingsForDeletionAsync(List<int> ids)
    {
        if (ids == null || ids.Count == 0) return new List<SubjectOffering>();
        return await _context.SubjectOfferings
            .Where(so => ids.Contains(so.Id))
            .ToListAsync();
    }

    public async Task<List<SubjectOffering>> GetSubjectOfferingsAsync(int programId, int? semesterId = null)
    {
        var query = _context.SubjectOfferings
            .Include(so => so.SubjectCatalog)
            .Include(so => so.Program)
            .Include(so => so.Semester)
            .AsNoTracking()
            .ApplyScope(_userContext)
            .Where(so => so.ProgramId == programId);

        if (semesterId.HasValue)
            query = query.Where(so => so.SemesterId == semesterId.Value);

        return await query
            .OrderBy(so => so.Semester != null ? so.Semester.Number : 0)
            .ThenBy(so => so.DisplayOrder)
            .ThenBy(so => so.SubjectCatalog != null ? so.SubjectCatalog.SubjectName : "")
            .ToListAsync();
    }

    public async Task<(List<SubjectCatalog> SubjectCatalogs, List<Program> Programs, List<Semester> Semesters)> GetSelectListsAsync(int? subjectCatalogId = null, int? programId = null, int? semesterId = null)
    {
        var subjectCatalogs = await _context.SubjectCatalogs
            .Where(s => s.IsActive)
            .OrderBy(s => s.SubjectCode)
            .AsNoTracking()
            .ToListAsync();

        var programs = await _context.Programs
            .Where(p => p.IsActive)
            .ApplyScope(_userContext)
            .OrderBy(p => p.ProgramName)
            .AsNoTracking()
            .ToListAsync();

        var semesters = await _context.Semesters
            .Include(s => s.AcademicYear)
            .OrderByDescending(s => s.Id)
            .AsNoTracking()
            .ToListAsync();

        return (subjectCatalogs, programs, semesters);
    }

    public async Task<bool> IsSemesterAssignedToProgramAsync(int programId, int semesterId)
    {
        return await _context.ProgramSemesters
            .AsNoTracking()
            .AnyAsync(ps => ps.ProgramId == programId && ps.SemesterId == semesterId && ps.IsActive);
    }

    public async Task<List<SelectOption>> GetCurriculumVersionsAsync(int? programId = null, int? academicYearId = null)
    {
        var query = _context.CurriculumVersions
            .AsNoTracking()
            .ApplyScope(_userContext)
            .Where(cv => cv.IsActive);

        if (programId.HasValue && programId.Value > 0)
            query = query.Where(cv => cv.ProgramId == programId.Value);

        if (academicYearId.HasValue && academicYearId.Value > 0)
            query = query.Where(cv => cv.EffectiveAcademicYearId == academicYearId.Value);

        return await query
            .OrderBy(cv => cv.Program != null ? cv.Program.ProgramName : "")
            .ThenByDescending(cv => cv.EffectiveAcademicYearId)
            .Select(cv => new SelectOption
            {
                Id = cv.Id,
                Name = cv.Name + (cv.EffectiveAcademicYear != null ? " (" + cv.EffectiveAcademicYear.AcademicYearName + ")" : "")
            })
            .ToListAsync();
    }

    public async Task<CurriculumVersion?> GetCurriculumVersionByIdAsync(int id)
    {
        return await _context.CurriculumVersions
            .AsNoTracking()
            .ApplyScope(_userContext)
            .Include(cv => cv.Program)
            .Include(cv => cv.EffectiveAcademicYear)
            .FirstOrDefaultAsync(cv => cv.Id == id);
    }

    public async Task<List<SubjectOffering>> GetSubjectOfferingsByCurriculumVersionAsync(int curriculumVersionId)
    {
        return await _context.SubjectOfferings
            .Include(so => so.SubjectCatalog)
            .ThenInclude(sc => sc!.SubjectType)
            .Include(so => so.Semester)
            .AsNoTracking()
            .ApplyScope(_userContext)
            .Where(so => so.CurriculumVersionId == curriculumVersionId)
            .OrderBy(so => so.Semester != null ? so.Semester.Number : 0)
            .ThenBy(so => so.DisplayOrder)
            .ThenBy(so => so.SubjectCatalog != null ? so.SubjectCatalog.SubjectName : "")
            .ToListAsync();
    }

    public async Task<bool> IsCurriculumVersionForProgramAsync(int curriculumVersionId, int programId)
    {
        return await _context.CurriculumVersions
            .AsNoTracking()
            .AnyAsync(cv => cv.Id == curriculumVersionId && cv.ProgramId == programId);
    }

    public async Task<List<SubjectOffering>> GetSearchResultsAsync(int? academicYearId, int? programId, int? semesterId)
    {
        var query = _context.SubjectOfferings
            .Include(so => so.SubjectCatalog)
            .Include(so => so.Program)
            .Include(so => so.Semester)
            .AsNoTracking()
            .ApplyScope(_userContext);

        if (academicYearId is > 0)
            query = query.Where(so => so.Semester != null && so.Semester.AcademicYearId == academicYearId.Value);
        if (programId is > 0)
            query = query.Where(so => so.ProgramId == programId.Value);
        if (semesterId is > 0)
            query = query.Where(so => so.SemesterId == semesterId.Value);

        return await query
            .OrderBy(so => so.Program != null ? so.Program.ProgramName : "")
            .ThenBy(so => so.Semester != null ? so.Semester.Number : 0)
            .ThenBy(so => so.DisplayOrder)
            .ThenBy(so => so.SubjectCatalog != null ? so.SubjectCatalog.SubjectName : "")
            .ToListAsync();
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
