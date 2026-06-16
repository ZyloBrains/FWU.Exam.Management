using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class NaturalResourceManagementSeeder
{
    public static async Task SeedNaturalResourceManagementAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        var nrmFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "NRM");
        var bachelorLevel = await context.Levels.FirstOrDefaultAsync(l => l.LevelCode == "BL");
        var masterLevel = await context.Levels.FirstOrDefaultAsync(l => l.LevelCode == "MA");

        // Department
        Department? nrmDept;
        if (!await context.Departments.AnyAsync(d => d.DepartmentCode == "NRM"))
        {
            nrmDept = new Department
            {
                DepartmentCode = "NRM",
                DepartmentName = "Natural Resource Management",
                ShortName = "NRM",
                IsActive = true,
            };
            context.Departments.Add(nrmDept);
            await context.SaveChangesAsync();
        }
        else
        {
            nrmDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "NRM");
        }

        // Programs
        if (bachelorLevel != null && nrmDept != null)
        {
            await AddProgramIfMissing(context, "BScNRM", "Bachelor of Science in Natural Resource Management", "B.Sc. NRM", bachelorLevel.Id, nrmDept.Id, 4, "NRM");
        }
        if (masterLevel != null && nrmDept != null)
        {
            await AddProgramIfMissing(context, "MScNRM", "Master of Science in Natural Resource Management", "M.Sc. NRM", masterLevel.Id, nrmDept.Id, 2, "MNRM");
        }
        await context.SaveChangesAsync();

        // College
        College? nrmCollege;
        if (!await context.Colleges.IgnoreQueryFilters().AnyAsync(c => c.Code == "CDNRM"))
        {
            nrmCollege = new College
            {
                Code = "CDNRM",
                Name = "Central Department of Natural Resource Management",
                TenantId = 1,
                IsActive = true,
            };
            context.Colleges.Add(nrmCollege);
            await context.SaveChangesAsync();
            if (nrmFaculty != null)
            {
                nrmCollege.Faculties = new List<Faculty> { nrmFaculty };
                await context.SaveChangesAsync();
            }
        }
        else
        {
            nrmCollege = await context.Colleges.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Code == "CDNRM");
        }

        // CollegeProgram links
        if (nrmCollege != null)
        {
            var nrmProgramCodes = new[] { "BScNRM", "MScNRM" };
            var nrmPrograms = await context.Programs.Where(p => nrmProgramCodes.Contains(p.ProgramCode)).ToListAsync();
            foreach (var program in nrmPrograms)
            {
                if (!await context.CollegePrograms.IgnoreQueryFilters().AnyAsync(cp => cp.CollegeId == nrmCollege.Id && cp.ProgramId == program.Id))
                {
                    context.CollegePrograms.Add(new CollegeProgram
                    {
                        CollegeId = nrmCollege.Id,
                        ProgramId = program.Id,
                        TenantId = 1,
                        IsActive = true,
                    });
                }
            }
            await context.SaveChangesAsync();
        }
    }

    private static async Task AddProgramIfMissing(AppDbContext context, string code, string name, string shortName, int levelId, int departmentId, int duration, string prefix)
    {
        var exists = await context.Programs.AnyAsync(p => p.ProgramCode == code);
        if (!exists)
        {
            context.Programs.Add(new Program
            {
                ProgramCode = code,
                ProgramName = name,
                ShortName = shortName,
                LevelId = levelId,
                DepartmentId = departmentId,
                Duration = duration,
                IsActive = true,
                RollNumberPrefix = prefix,
            });
        }
    }
}
