using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class ProgramSemesterSeeder
{
    public static async Task SeedProgramSemestersAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

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
        foreach (var program in await context.Programs.AsNoTracking().Where(p => p.FacultyId.HasValue && p.IsActive).ToListAsync())
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
}
