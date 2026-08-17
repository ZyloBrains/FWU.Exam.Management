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
        if (curriculumVersion.IsActive)
            await DeactivateOtherVersionsAsync(curriculumVersion.ProgramId, null);
        _context.CurriculumVersions.Add(curriculumVersion);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCurriculumVersionAsync(CurriculumVersion curriculumVersion)
    {
        var existing = await _context.CurriculumVersions.FindAsync(curriculumVersion.Id)
            ?? throw new InvalidOperationException("Curriculum version not found.");

        existing.Name = curriculumVersion.Name;
        existing.ProgramId = curriculumVersion.ProgramId;
        existing.EffectiveAcademicYearId = curriculumVersion.EffectiveAcademicYearId;
        existing.Description = curriculumVersion.Description;
        existing.IsActive = curriculumVersion.IsActive;

        if (existing.IsActive)
            await DeactivateOtherVersionsAsync(existing.ProgramId, existing.Id);
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Deleted, int SkippedOfferings)> DeleteCurriculumVersionAsync(int id)
    {
        var curriculumVersion = await _context.CurriculumVersions
            .Include(cv => cv.SubjectOfferings)
            .FirstOrDefaultAsync(cv => cv.Id == id);

        if (curriculumVersion == null)
            return (false, 0);

        var offerings = curriculumVersion.SubjectOfferings?.ToList() ?? new List<SubjectOffering>();

        var referencedIds = new HashSet<int>();
        if (offerings.Count > 0)
        {
            var ids = offerings.Select(o => o.Id).ToList();
            var examSlotIds = await _context.ExamSlots
                .Where(x => ids.Contains(x.SubjectOfferingId))
                .Select(x => x.SubjectOfferingId)
                .ToListAsync();
            var examResultIds = await _context.ExamSubjectResults
                .Where(x => ids.Contains(x.SubjectOfferingId))
                .Select(x => x.SubjectOfferingId)
                .ToListAsync();
            var assignmentIds = await _context.CollegeAdminSubjectAssignments
                .Where(x => ids.Contains(x.SubjectOfferingId))
                .Select(x => x.SubjectOfferingId)
                .ToListAsync();

            foreach (var referencedId in examSlotIds.Concat(examResultIds).Concat(assignmentIds))
                referencedIds.Add(referencedId);
        }

        var deletable = offerings.Where(o => !referencedIds.Contains(o.Id)).ToList();
        _context.SubjectOfferings.RemoveRange(deletable);

        var skipped = offerings.Count - deletable.Count;
        if (skipped == 0)
            _context.CurriculumVersions.Remove(curriculumVersion);

        await _context.SaveChangesAsync();
        return (skipped == 0, skipped);
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

    public async Task<List<CurriculumVersion>> GetCurriculumVersionsByProgramAsync(int programId)
    {
        return await _context.CurriculumVersions
            .Include(c => c.Program)
            .Include(c => c.EffectiveAcademicYear)
            .AsNoTracking()
            .Where(c => c.ProgramId == programId)
            .OrderByDescending(c => c.EffectiveAcademicYearId)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<CurriculumVersion?> CopyCurriculumVersionAsync(int sourceVersionId, int targetAcademicYearId, string name)
    {
        var source = await _context.CurriculumVersions
            .Include(c => c.Program)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == sourceVersionId);

        if (source == null) return null;

        var targetYear = await _context.AcademicYears
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == targetAcademicYearId);

        if (targetYear == null) return null;

        var sourceOfferings = await _context.SubjectOfferings
            .AsNoTracking()
            .Where(so => so.CurriculumVersionId == sourceVersionId)
            .ToListAsync();

        var targetSemesters = await _context.Semesters
            .AsNoTracking()
            .ToListAsync();

        var semesterMap = targetSemesters.ToDictionary(s => s.Number);

        var sourceSemesterMap = await _context.Semesters
            .AsNoTracking()
            .Where(s => sourceOfferings.Select(o => o.SemesterId).Contains(s.Id))
            .ToDictionaryAsync(s => s.Id);

        var newVersion = new CurriculumVersion
        {
            TenantId = source.TenantId,
            Name = name,
            ProgramId = source.ProgramId,
            EffectiveAcademicYearId = targetAcademicYearId,
            Description = source.Description,
            IsActive = true
        };

        await DeactivateOtherVersionsAsync(source.ProgramId, null);
        _context.CurriculumVersions.Add(newVersion);
        await _context.SaveChangesAsync();

        var skipped = 0;
        foreach (var offering in sourceOfferings.OrderBy(o => o.SemesterId))
        {
            var sourceSemesterNumber = sourceSemesterMap.TryGetValue(offering.SemesterId, out var sourceSemester)
                ? sourceSemester.Number
                : -1;
            var targetSemester = sourceSemesterNumber >= 0 && semesterMap.TryGetValue(sourceSemesterNumber, out var t)
                ? t
                : null;
            if (targetSemester == null)
            {
                skipped++;
                continue;
            }

            _context.SubjectOfferings.Add(new SubjectOffering
            {
                TenantId = source.TenantId,
                SubjectCatalogId = offering.SubjectCatalogId,
                ProgramId = source.ProgramId,
                SemesterId = targetSemester.Id,
                CurriculumVersionId = newVersion.Id,
                IsCompulsory = offering.IsCompulsory,
                DisplayOrder = offering.DisplayOrder,
                HasTheory = offering.HasTheory,
                HasPractical = offering.HasPractical,
                HasInternal = offering.HasInternal,
                TheoryFullMarks = offering.TheoryFullMarks,
                TheoryPassMarks = offering.TheoryPassMarks,
                PracticalFullMarks = offering.PracticalFullMarks,
                PracticalPassMarks = offering.PracticalPassMarks,
                InternalTheoryFullMarks = offering.InternalTheoryFullMarks,
                InternalTheoryPassMarks = offering.InternalTheoryPassMarks
            });
        }

        await _context.SaveChangesAsync();
        return newVersion;
    }

    private async Task DeactivateOtherVersionsAsync(int programId, int? exceptId)
    {
        var others = await _context.CurriculumVersions
            .Where(c => c.ProgramId == programId && c.IsActive && (!exceptId.HasValue || c.Id != exceptId.Value))
            .ToListAsync();
        foreach (var v in others)
            v.IsActive = false;
    }

    private static Expression<Func<CurriculumVersion, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "id" => c => c.Id,
            "name" => c => c.Name ?? "",
            "program" => c => c.Program != null ? c.Program.ProgramName : "",
            "academicyear" => c => c.EffectiveAcademicYear != null ? c.EffectiveAcademicYear.AcademicYearName : "",
            "isactive" => c => c.IsActive,
            _ => c => c.Name ?? ""
        };
    }
}
