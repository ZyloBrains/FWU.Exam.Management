using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class AcademicYearService(AppDbContext context, ITenantContext tenantContext) : IAcademicYearService
{
    public async Task<(List<AcademicYear> Items, int TotalCount)> GetAllAcademicYearsAsync(int page, int pageSize, string? search)
    {
        var query = context.AcademicYears.AsNoTracking();
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(a => a.AcademicYearName.Contains(search) ||
                                     a.AcademicYearCode.ToString().Contains(search) ||
                                     a.AcademicYearNameNepali.Contains(search) ||
                                     (a.AcademicYearCodeNepali ?? "").Contains(search) ||
                                     (a.Remark ?? "").Contains(search));
        }
        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();


        return (items, totalCount);
    }

    public async Task<AcademicYear?> GetAcademicYearByIdAsync(int id)
    {
        return await context.AcademicYears
            .AsNoTracking()
            .FirstOrDefaultAsync(ay => ay.Id == id);
    }

    public async Task CreateAcademicYearAsync(AcademicYear academicYear)
    {
        context.AcademicYears.Add(academicYear);
        await context.SaveChangesAsync();

        await CreateSemesterInstancesAsync(academicYear);
    }

    public async Task UpdateAcademicYearAsync(AcademicYear academicYear)
    {
        context.AcademicYears.Update(academicYear);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAcademicYearAsync(int id)
    {
        var academicYear = await context.AcademicYears.FindAsync(id);
        if (academicYear != null)
        {
            context.AcademicYears.Remove(academicYear);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> AcademicYearExistsAsync(int id)
    {
        return await context.AcademicYears.AnyAsync(ay => ay.Id == id);
    }

    private async Task CreateSemesterInstancesAsync(AcademicYear academicYear)
    {
        if (academicYear.StartDate == null || academicYear.EndDate == null)
            return;

        var tenantId = tenantContext.TenantId;

        var tenantFacultyIds = await context.CollegeFaculties
            .Where(cf => cf.TenantId == tenantId)
            .Select(cf => cf.FacultyId)
            .Distinct()
            .ToListAsync();

        var programSemesters = await context.ProgramSemesters
            .Where(ps => ps.IsActive
                && ps.Program != null
                && ps.Program.FacultyId.HasValue
                && tenantFacultyIds.Contains(ps.Program.FacultyId.Value))
            .Include(ps => ps.Semester)
            .ToListAsync();

        if (programSemesters.Count == 0)
            return;

        var programsGrouped = programSemesters
            .GroupBy(ps => ps.ProgramId);

        var semesterInstances = new List<SemesterInstance>();

        foreach (var programGroup in programsGrouped)
        {
            var programId = programGroup.Key;
            var semesters = programGroup
                .Where(ps => ps.Semester != null)
                .OrderBy(ps => ps.Semester!.Number)
                .ToList();

            if (semesters.Count == 0)
                continue;

            var totalDays = (academicYear.EndDate.Value - academicYear.StartDate.Value).TotalDays;
            var daysPerSemester = totalDays / semesters.Count;

            for (int i = 0; i < semesters.Count; i++)
            {
                var sem = semesters[i];
                var startDate = academicYear.StartDate.Value.AddDays(i * daysPerSemester);
                var endDate = (i == semesters.Count - 1)
                    ? academicYear.EndDate.Value
                    : academicYear.StartDate.Value.AddDays((i + 1) * daysPerSemester).AddDays(-1);

                semesterInstances.Add(new SemesterInstance
                {
                    TenantId = tenantId,
                    SemesterId = sem.SemesterId,
                    AcademicYearId = academicYear.Id,
                    ProgramId = programId,
                    StartDate = startDate,
                    EndDate = endDate
                });
            }
        }

        if (semesterInstances.Count > 0)
        {
            context.SemesterInstances.AddRange(semesterInstances);
            await context.SaveChangesAsync();
        }
    }
}
