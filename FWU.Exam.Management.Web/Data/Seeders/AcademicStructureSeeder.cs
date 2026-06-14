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

        // ----- Departments (fill missing ones not created by existing seeders) -----
        if (!await context.Departments.AnyAsync(d => d.DepartmentCode == "LAW"))
        {
            context.Departments.Add(new Department { DepartmentCode = "LAW", DepartmentName = "Law", ShortName = "LAW", IsActive = true });
        }
        if (!await context.Departments.AnyAsync(d => d.DepartmentCode == "AGR"))
        {
            context.Departments.Add(new Department { DepartmentCode = "AGR", DepartmentName = "Agriculture", ShortName = "AGR", IsActive = true });
        }
        if (!await context.Departments.AnyAsync(d => d.DepartmentCode == "HSC"))
        {
            context.Departments.Add(new Department { DepartmentCode = "HSC", DepartmentName = "Health Sciences", ShortName = "HSC", IsActive = true });
        }
        await context.SaveChangesAsync();

        // ----- Programs (fill missing ones) -----
        var bachelorLevel = await context.Levels.FirstOrDefaultAsync(l => l.LevelCode == "BL");
        var masterLevel = await context.Levels.FirstOrDefaultAsync(l => l.LevelCode == "MA");
        var deptMgmt = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "MGMT");
        var deptSci = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "SCI");
        var deptEngg = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "ENGG");
        var deptEdu = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "EDU");
        var deptHum = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "HUM");
        var deptLaw = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "LAW");
        var deptAgr = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "AGR");
        var deptHsc = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "HSC");

        if (bachelorLevel != null)
        {
            // Management programs
            if (deptMgmt != null)
            {
                await AddProgramIfMissing(context, "BPA", "Bachelor of Public Administration", "BPA", bachelorLevel.Id, deptMgmt.Id, 4, "BPA");
            }

            // Science & Technology programs
            if (deptSci != null)
            {
                await AddProgramIfMissing(context, "BSC", "Bachelor of Science (General)", "B.Sc.", bachelorLevel.Id, deptSci.Id, 4, "BSC");
                await AddProgramIfMissing(context, "BMT", "Bachelor of Medical Lab Technology", "BMLT", bachelorLevel.Id, deptSci.Id, 4, "BMLT");
            }

            // Engineering programs
            if (deptEngg != null)
            {
                await AddProgramIfMissing(context, "BEEE", "Bachelor of Engineering in Electrical & Electronics", "BE Electrical", bachelorLevel.Id, deptEngg.Id, 4, "BEEE");
            }

            // Education programs
            if (deptEdu != null)
            {
                await AddProgramIfMissing(context, "BED", "Bachelor of Education", "B.Ed.", bachelorLevel.Id, deptEdu.Id, 4, "BED");
            }

            // Humanities programs
            if (deptHum != null)
            {
                await AddProgramIfMissing(context, "BA", "Bachelor of Arts", "BA", bachelorLevel.Id, deptHum.Id, 4, "BA");
                await AddProgramIfMissing(context, "BSW", "Bachelor of Social Work", "BSW", bachelorLevel.Id, deptHum.Id, 4, "BSW");
            }

            // Law programs
            if (deptLaw != null)
            {
                await AddProgramIfMissing(context, "LLB", "Bachelor of Laws", "LLB", bachelorLevel.Id, deptLaw.Id, 5, "LLB");
            }

            // Agriculture programs
            if (deptAgr != null)
            {
                await AddProgramIfMissing(context, "BSCAG", "Bachelor of Science in Agriculture", "B.Sc. Ag.", bachelorLevel.Id, deptAgr.Id, 4, "AG");
            }

            // Health Sciences programs
            if (deptHsc != null)
            {
                await AddProgramIfMissing(context, "BPH", "Bachelor of Public Health", "BPH", bachelorLevel.Id, deptHsc.Id, 4, "BPH");
                await AddProgramIfMissing(context, "BN", "Bachelor of Nursing", "BN", bachelorLevel.Id, deptHsc.Id, 4, "BN");
            }
        }

        // Master level programs
        if (masterLevel != null)
        {
            if (deptMgmt != null)
            {
                await AddProgramIfMissing(context, "MBA", "Master of Business Administration", "MBA", masterLevel.Id, deptMgmt.Id, 2, "MBA");
            }
            if (deptSci != null)
            {
                await AddProgramIfMissing(context, "MSCSIT", "Master of Science in Computer Science and Information Technology", "M.Sc. CSIT", masterLevel.Id, deptSci.Id, 2, "MSCSIT");
            }
            if (deptEdu != null)
            {
                await AddProgramIfMissing(context, "MED", "Master of Education", "M.Ed.", masterLevel.Id, deptEdu.Id, 2, "MED");
            }
        }

        await context.SaveChangesAsync();

        // ----- Faculties (fill missing ones not created by existing seeders) -----
        await AddFacultyIfMissing(context, "FO-MGT", "Faculty of Management", "099-520729", "Mahendranagar, Kanchanpur", "management@fwu.edu.np");
        await AddFacultyIfMissing(context, "FO-HSS", "Faculty of Humanities and Social Sciences", "099-520729", "Mahendranagar, Kanchanpur", "humanities@fwu.edu.np");
        await AddFacultyIfMissing(context, "FOL", "Faculty of Law", "099-520729", "Mahendranagar, Kanchanpur", "law@fwu.edu.np");
        await AddFacultyIfMissing(context, "NRM", "Faculty of Natural Resource Management", "099-520729", "Mahendranagar, Kanchanpur", "nrm@fwu.edu.np");
        var hscFacultyExists = await context.Faculties.AnyAsync(f => f.OfficeCode == "HSC");
        if (!hscFacultyExists)
        {
            context.Faculties.Add(new Faculty { Name = "Faculty of Health Sciences", OfficeCode = "HSC", ContactNumber = "099-520729", Address = "Mahendranagar, Kanchanpur", Email = "health@fwu.edu.np" });
        }
        await context.SaveChangesAsync();

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

    private static async Task AddFacultyIfMissing(AppDbContext context, string officeCode, string name, string contact, string address, string email)
    {
        var exists = await context.Faculties.AnyAsync(f => f.OfficeCode == officeCode);
        if (!exists)
        {
            context.Faculties.Add(new Faculty
            {
                Name = name,
                OfficeCode = officeCode,
                ContactNumber = contact,
                Address = address,
                Email = email,
            });
        }
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
