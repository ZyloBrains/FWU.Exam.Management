using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class SemesterSeeder
{
    private static readonly (int Number, string Name, string Code)[] SemesterTemplates =
    [
        (1,  "First Semester",   "SEM01"),
        (2,  "Second Semester",  "SEM02"),
        (3,  "Third Semester",   "SEM03"),
        (4,  "Fourth Semester",  "SEM04"),
        (5,  "Fifth Semester",   "SEM05"),
        (6,  "Sixth Semester",   "SEM06"),
        (7,  "Seventh Semester", "SEM07"),
        (8,  "Eighth Semester",  "SEM08"),
        (9,  "Ninth Semester",   "SEM09"),
        (10, "Tenth Semester",   "SEM10"),
    ];

    public static async Task SeedSemestersAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (await context.Semesters.AnyAsync())
            return;

        var semesters = SemesterTemplates.Select(t => new Semester
        {
            Number = t.Number,
            Name = t.Name,
            Code = t.Code,
            Remark = $"S{t.Number:D2}",
        }).ToList();

        await context.Semesters.AddRangeAsync(semesters);
        await context.SaveChangesAsync();
    }

    public static async Task SeedProgramSemestersAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (await context.ProgramSemesters.AnyAsync())
            return;

        var programs = await context.Programs.ToListAsync();
        var semesters = await context.Semesters.OrderBy(s => s.Number).ToListAsync();

        if (programs.Count == 0 || semesters.Count == 0)
            return;

        var programSemesters = new List<ProgramSemester>();
        var displayOrder = 1;

        foreach (var program in programs)
        {
            foreach (var semester in semesters)
            {
                programSemesters.Add(new ProgramSemester
                {
                    ProgramId = program.Id,
                    SemesterId = semester.Id,
                    IsActive = true,
                    DisplayOrder = displayOrder++,
                });
            }
        }

        await context.ProgramSemesters.AddRangeAsync(programSemesters);
        await context.SaveChangesAsync();
    }

    public static async Task SeedSemesterInstancesAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (await context.SemesterInstances.IgnoreQueryFilters().AnyAsync())
            return;

        var tenantContext = serviceProvider.GetRequiredService<ITenantContext>();
        var tenantId = tenantContext.TenantId;

        var runningYears = await context.AcademicYears.IgnoreQueryFilters()
            .Where(ay => ay.TenantId == tenantId && ay.IsRunning)
            .ToListAsync();

        var programs = await context.Programs.ToListAsync();
        var semesters = await context.Semesters.OrderBy(s => s.Number).ToListAsync();

        if (runningYears.Count == 0 || programs.Count == 0 || semesters.Count == 0)
            return;

        var instances = new List<SemesterInstance>();

        foreach (var year in runningYears)
        {
            var yearStart = year.StartDate
                ?? new DateTime(int.Parse(year.AcademicYearCode), 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var yearEnd = year.EndDate
                ?? new DateTime(int.Parse(year.AcademicYearCode), 12, 31, 0, 0, 0, DateTimeKind.Utc);
            var totalDays = (yearEnd - yearStart).Days;

            foreach (var program in programs)
            {
                for (var i = 0; i < semesters.Count; i++)
                {
                    var semester = semesters[i];
                    var segDays = totalDays / semesters.Count;
                    var startDate = yearStart.AddDays(i * segDays);
                    var endDate = (i == semesters.Count - 1)
                        ? yearEnd
                        : startDate.AddDays(segDays);

                    instances.Add(new SemesterInstance
                    {
                        TenantId = tenantId,
                        SemesterId = semester.Id,
                        AcademicYearId = year.Id,
                        ProgramId = program.Id,
                        StartDate = startDate,
                        EndDate = endDate,
                    });
                }
            }
        }

        await context.SemesterInstances.AddRangeAsync(instances);
        await context.SaveChangesAsync();
    }
}
