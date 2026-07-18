using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class AcademicStructureSeeder
{
    public static async Task SeedAcademicStructureAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        // ----- Colleges (fill missing ones by checking individual codes) -----
        await AddCollegeIfMissing(context, "COC", "College of Commerce", 1);
        await AddCollegeIfMissing(context, "SOM", "School of Management", 1);
        await AddCollegeIfMissing(context, "CDHSC", "Central Department of Health Sciences", 1);

        // ----- CollegeProgram links (fill all missing combinations) -----
        var allColleges = await context.Colleges.IgnoreQueryFilters().ToListAsync();
        var allPrograms = await context.Programs.ToListAsync();
        var existingLinks = await context.CollegePrograms.IgnoreQueryFilters()
            .Select(cp => new { cp.CollegeId, cp.ProgramId })
            .ToListAsync();
        var existingLinkSet = new HashSet<(int, int)>(existingLinks.Select(l => (l.CollegeId, l.ProgramId)));

        foreach (var college in allColleges)
        {
            foreach (var program in allPrograms)
            {
                if (!existingLinkSet.Contains((college.Id, program.Id)))
                {
                    context.CollegePrograms.Add(new CollegeProgram
                    {
                        CollegeId = college.Id,
                        ProgramId = program.Id,
                        TenantId = college.TenantId,
                        IsActive = true,
                    });
                }
            }
        }
        await context.SaveChangesAsync();
    }

    private static async Task AddCollegeIfMissing(AppDbContext context, string code, string name, int tenantId)
    {
        var exists = await context.Colleges.IgnoreQueryFilters().AnyAsync(c => c.Code == code);
        if (!exists)
        {
            context.Colleges.Add(new College
            {
                Code = code,
                Name = name,
                TenantId = tenantId,
                IsActive = true,
            });
        }
    }
}
