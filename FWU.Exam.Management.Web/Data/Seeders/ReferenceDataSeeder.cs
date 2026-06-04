using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class ReferenceDataSeeder
{
    public static async Task SeedPaymentTypesAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (await context.Set<PaymentType>().AnyAsync())
            return;

        var paymentTypes = new[]
        {
            new PaymentType { PaymentTypeName = "eSewa", IsActive = true },
            new PaymentType { PaymentTypeName = "Khalti", IsActive = true },
            new PaymentType { PaymentTypeName = "ConnectIPS", IsActive = true },
        };
        await context.Set<PaymentType>().AddRangeAsync(paymentTypes);
        await context.SaveChangesAsync();
    }

    public static async Task SeedReferenceDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (await context.Genders.AnyAsync())
            return;

        // Genders
        var genders = new[]
        {
            new Gender { GenderName = "Male", IsActive = true },
            new Gender { GenderName = "Female", IsActive = true },
            new Gender { GenderName = "Other", IsActive = true },
        };
        await context.Genders.AddRangeAsync(genders);

        // Previous Levels
        var previousLevels = new[]
        {
            new PreviousLevel { PreviousLevelName = "SEE (Grade 10)", IsActive = true },
            new PreviousLevel { PreviousLevelName = "+2 / 10+2", IsActive = true },
            new PreviousLevel { PreviousLevelName = "Bachelor", IsActive = true },
            new PreviousLevel { PreviousLevelName = "Master", IsActive = true },
        };
        await context.PreviousLevels.AddRangeAsync(previousLevels);

        // Levels
        var levels = new[]
        {
            new Level { LevelCode = "BL", LevelName = "Bachelor", IsActive = true },
            new Level { LevelCode = "MA", LevelName = "Master", IsActive = true },
        };
        await context.Levels.AddRangeAsync(levels);
        await context.SaveChangesAsync();

        // Departments
        var departments = new[]
        {
            new Department { DepartmentCode = "MGMT", DepartmentName = "Management", ShortName = "MGT", IsActive = true },
            new Department { DepartmentCode = "SCI", DepartmentName = "Science", ShortName = "SCI", IsActive = true },
            new Department { DepartmentCode = "EDU", DepartmentName = "Education", ShortName = "EDU", IsActive = true },
            new Department { DepartmentCode = "HUM", DepartmentName = "Humanities", ShortName = "HUM", IsActive = true },
        };
        await context.Departments.AddRangeAsync(departments);
        await context.SaveChangesAsync();

        // Programs
        var programs = new[]
        {
            new Program
            {
                ProgramCode = "BBA",
                ProgramName = "Bachelor of Business Administration",
                ShortName = "BBA",
                LevelId = levels[0].Id,
                DepartmentId = departments[0].Id,
                Duration = 4,
                IsActive = true,
            },
            new Program
            {
                ProgramCode = "BBS",
                ProgramName = "Bachelor of Business Studies",
                ShortName = "BBS",
                LevelId = levels[0].Id,
                DepartmentId = departments[0].Id,
                Duration = 4,
                IsActive = true,
            },
            new Program
            {
                ProgramCode = "BCA",
                ProgramName = "Bachelor of Computer Application",
                ShortName = "BCA",
                LevelId = levels[0].Id,
                DepartmentId = departments[1].Id,
                Duration = 4,
                IsActive = true,
            },
        };
        await context.Programs.AddRangeAsync(programs);
        await context.SaveChangesAsync();

        // Faculties
        if (!await context.Faculties.AnyAsync())
        {
            var engTenant = await context.Tenants.FirstOrDefaultAsync(t => t.OfficeCode == "ENG");
            var faculties = new[]
            {
                new Faculty
                {
                    Name = "School of Engineering",
                    OfficeCode = "SOE",
                    ContactNumber = "021-123456",
                    Address = "Mahendranagar, Kanchanpur",
                    Email = "soe@fwu.edu.np",
                    TenantId = engTenant?.Id,
                },
            };
            await context.Faculties.AddRangeAsync(faculties);
            await context.SaveChangesAsync();
        }

        // Colleges
        var org = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "SOE");
        var colleges = new[]
        {
            new College
            {
                Code = "COC",
                Name = "College of Commerce",
                TenantId = 1,
                IsActive = true,
                FacultyId = org?.Id,
            },
            new College
            {
                Code = "SOM",
                Name = "School of Management",
                TenantId = 1,
                IsActive = true,
                FacultyId = org?.Id,
            },
        };
        await context.Colleges.AddRangeAsync(colleges);
        await context.SaveChangesAsync();
    }

    public static async Task SeedTenantsAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (!await context.Tenants.AnyAsync())
        {
            await context.Tenants.AddRangeAsync(new[]
            {
                new Tenant
                {
                    Name = "Office of Controller of Examinations",
                    OfficeCode = "OCE",
                    ContactNumber = "01-2345678",
                    Address = "Kathmandu, Nepal",
                    Email = "info@oce.gov.np",
                    TenantType = TenantType.Central,
                    IsActive = true,
                },
                new Tenant
                {
                    Name = "Agriculture",
                    OfficeCode = "AGR",
                    ContactNumber = "01-1234567",
                    Address = "Kathmandu, Nepal",
                    Email = "info@agriculture.fwu.edu.np",
                    TenantType = TenantType.Standard,
                    IsActive = true,
                },
                new Tenant
                {
                    Name = "Engineering",
                    OfficeCode = "ENG",
                    ContactNumber = "01-7654321",
                    Address = "Kathmandu, Nepal",
                    Email = "info@engineering.fwu.edu.np",
                    TenantType = TenantType.Standard,
                    IsActive = true,
                },
            });
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedAdditionalReferenceDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        Faculty? foe, fst;
        var engTenant = await context.Tenants.FirstOrDefaultAsync(t => t.OfficeCode == "ENG");
        var agrTenant = await context.Tenants.FirstOrDefaultAsync(t => t.OfficeCode == "AGR");

        var facultySeedData = new[]
        {
            new Faculty { Name = "Faculty of Engineering", OfficeCode = "FOE", ContactNumber = "099-520296", Address = "Mahendranagar, Kanchanpur", Email = "dean.engineering@fwu.edu.np", TenantId = engTenant?.Id },
            new Faculty { Name = "Faculty of Science & Technology", OfficeCode = "FST", ContactNumber = "099-524000", Address = "Mahendranagar, Kanchanpur", Email = "faculty.science@fwu.edu.np" },
            new Faculty { Name = "Faculty of Law", OfficeCode = "LAW", ContactNumber = "099-520729", Address = "Mahendranagar, Kanchanpur", Email = "law@fwu.edu.np" },
            new Faculty { Name = "Faculty of Humanities", OfficeCode = "HUM", ContactNumber = "099-520729", Address = "Mahendranagar, Kanchanpur", Email = "humanities@fwu.edu.np" },
            new Faculty { Name = "Faculty of Education", OfficeCode = "EDU", ContactNumber = "099-520729", Address = "Mahendranagar, Kanchanpur", Email = "education@fwu.edu.np" },
            new Faculty { Name = "Faculty of Management", OfficeCode = "MGT", ContactNumber = "099-520729", Address = "Mahendranagar, Kanchanpur", Email = "management@fwu.edu.np" },
            new Faculty { Name = "Faculty of Agriculture", OfficeCode = "AGR", ContactNumber = "099-520729", Address = "Mahendranagar, Kanchanpur", Email = "agriculture@fwu.edu.np", TenantId = agrTenant?.Id },
            new Faculty { Name = "Faculty of Health Sciences", OfficeCode = "HSC", ContactNumber = "099-520729", Address = "Mahendranagar, Kanchanpur", Email = "health@fwu.edu.np" },
            new Faculty { Name = "Faculty of Natural Resource Management", OfficeCode = "NRM", ContactNumber = "099-520729", Address = "Mahendranagar, Kanchanpur", Email = "nrm@fwu.edu.np" },
        };

        foreach (var faculty in facultySeedData)
        {
            var existing = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == faculty.OfficeCode);
            if (existing == null)
            {
                context.Faculties.Add(faculty);
            }
        }
        await context.SaveChangesAsync();

        foe = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "FOE");
        fst = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "FST");

        Department? enggDept;
        if (!await context.Departments.AnyAsync(d => d.DepartmentCode == "ENGG"))
        {
            enggDept = new Department { DepartmentCode = "ENGG", DepartmentName = "Engineering", ShortName = "ENG", IsActive = true };
            context.Departments.Add(enggDept);
            await context.SaveChangesAsync();
        }
        else
        {
            enggDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "ENGG");
        }

        var bachelorLevel = await context.Levels.FirstOrDefaultAsync(l => l.LevelCode == "BL");

        if (!await context.Programs.AnyAsync(p => p.ProgramCode == "BECT") && enggDept != null && bachelorLevel != null)
        {
            context.Programs.AddRange(new[]
            {
                new Program { ProgramCode = "BECT", ProgramName = "Bachelor of Engineering in Civil", ShortName = "BE Civil", LevelId = bachelorLevel.Id, DepartmentId = enggDept.Id, Duration = 4, IsActive = true },
                new Program { ProgramCode = "BECP", ProgramName = "Bachelor of Engineering in Computer", ShortName = "BE Computer", LevelId = bachelorLevel.Id, DepartmentId = enggDept.Id, Duration = 4, IsActive = true },
                new Program { ProgramCode = "BARC", ProgramName = "Bachelor of Architecture", ShortName = "B.Arch", LevelId = bachelorLevel.Id, DepartmentId = enggDept.Id, Duration = 5, IsActive = true },
            });
            await context.SaveChangesAsync();
        }

        var sciDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "SCI");
        if (!await context.Programs.AnyAsync(p => p.ProgramCode == "BSCSIT") && sciDept != null && bachelorLevel != null)
        {
            context.Programs.AddRange(new[]
            {
                new Program { ProgramCode = "BSCSIT", ProgramName = "Bachelor of Science in Computer Science and Information Technology", ShortName = "B.Sc. CSIT", LevelId = bachelorLevel.Id, DepartmentId = sciDept.Id, Duration = 4, IsActive = true },
                new Program { ProgramCode = "BIT", ProgramName = "Bachelor of Information Technology", ShortName = "BIT", LevelId = bachelorLevel.Id, DepartmentId = sciDept.Id, Duration = 4, IsActive = true },
            });
            await context.SaveChangesAsync();
        }

        College? engCollege, csitCollege;
        if (!await context.Colleges.AnyAsync(c => c.Code == "ENG-SOE"))
        {
            engCollege = new College { Code = "ENG-SOE", Name = "School of Engineering", TenantId = 3, IsActive = true, FacultyId = foe?.Id };
            context.Colleges.Add(engCollege);
            await context.SaveChangesAsync();
        }
        else
        {
            engCollege = await context.Colleges.FirstOrDefaultAsync(c => c.Code == "ENG-SOE");
        }

        if (!await context.Colleges.AnyAsync(c => c.Code == "CDC-CSIT"))
        {
            csitCollege = new College { Code = "CDC-CSIT", Name = "Central Department of Computer Science & IT", TenantId = 1, IsActive = true, FacultyId = fst?.Id };
            context.Colleges.Add(csitCollege);
            await context.SaveChangesAsync();
        }
        else
        {
            csitCollege = await context.Colleges.FirstOrDefaultAsync(c => c.Code == "CDC-CSIT");
        }

        if (engCollege != null)
        {
            var engProgramCodes = new[] { "BECT", "BECP", "BARC" };
            var engPrograms = await context.Programs.Where(p => engProgramCodes.Contains(p.ProgramCode)).ToListAsync();
            foreach (var program in engPrograms)
            {
                if (!await context.CollegePrograms.AnyAsync(cp => cp.CollegeId == engCollege.Id && cp.ProgramId == program.Id))
                {
                    context.CollegePrograms.Add(new CollegeProgram { CollegeId = engCollege.Id, ProgramId = program.Id, TenantId = 3, IsActive = true });
                }
            }
            await context.SaveChangesAsync();
        }

        if (csitCollege != null)
        {
            var csitProgramCodes = new[] { "BSCSIT", "BIT" };
            var csitPrograms = await context.Programs.Where(p => csitProgramCodes.Contains(p.ProgramCode)).ToListAsync();
            foreach (var program in csitPrograms)
            {
                if (!await context.CollegePrograms.AnyAsync(cp => cp.CollegeId == csitCollege.Id && cp.ProgramId == program.Id))
                {
                    context.CollegePrograms.Add(new CollegeProgram { CollegeId = csitCollege.Id, ProgramId = program.Id, TenantId = 1, IsActive = true });
                }
            }
            await context.SaveChangesAsync();
        }

        var oceTenantId = 1;

        var mgtFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "MGT");
        var eduFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "EDU");
        var humFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "HUM");
        var lawFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "LAW");
        var agrFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "AGR");

        var centralDepartments = new[]
        {
            new College { Code = "CDC-CSIT", Name = "Central Department of Computer Science and Information Technology", TenantId = oceTenantId, IsActive = true, Website = "http://cdcsit.fwu.edu.np", FacultyId = fst?.Id },
            new College { Code = "DGS", Name = "Central Department of General Science", TenantId = oceTenantId, IsActive = true, Website = "http://science.fwu.edu.np", FacultyId = fst?.Id },
            new College { Code = "CDM", Name = "Central Department of Management", TenantId = oceTenantId, IsActive = true, Website = "http://management.fwu.edu.np", FacultyId = mgtFaculty?.Id },
            new College { Code = "CDE", Name = "Central Department of Education", TenantId = oceTenantId, IsActive = true, Website = "http://education.fwu.edu.np", FacultyId = eduFaculty?.Id },
            new College { Code = "CDH", Name = "Central Department of Humanities", TenantId = oceTenantId, IsActive = true, Website = "http://humanities.fwu.edu.np", FacultyId = humFaculty?.Id },
            new College { Code = "CDL", Name = "Central Department of Law", TenantId = oceTenantId, IsActive = true, Website = "http://agriculture.fwu.edu.np", FacultyId = lawFaculty?.Id },
            new College { Code = "CDA", Name = "Central Department of Agriculture", TenantId = oceTenantId, IsActive = true, Website = "http://law.fwu.edu.np", FacultyId = agrFaculty?.Id },
        };

        foreach (var dept in centralDepartments)
        {
            if (!await context.Colleges.AnyAsync(c => c.Code == dept.Code))
            {
                context.Colleges.Add(dept);
            }
        }
        await context.SaveChangesAsync();

        var campuses = new[]
        {
            new College { Code = "CC", Name = "Central Campus", TenantId = oceTenantId, IsActive = true, Website = "http://principal.fwu.edu.np" },
            new College { Code = "TMC", Name = "Tikapur Multiple Campus", TenantId = oceTenantId, IsActive = true, Website = "http://tikapur.fwu.edu.np" },
            new College { Code = "DMC", Name = "Darchula Multiple Campus", TenantId = oceTenantId, IsActive = true, Website = "http://darchula.fwu.edu.np" },
            new College { Code = "BJC", Name = "Bajura Campus", TenantId = oceTenantId, IsActive = true, Website = "http://bajura.fwu.edu.np" },
            new College { Code = "TVC", Name = "Triveni Multiple Campus", TenantId = oceTenantId, IsActive = true, Website = "http://treveni.fwu.edu.np" },
            new College { Code = "GSMC", Name = "Ghanteshwar Seti Mahakali Multiple Campus", TenantId = oceTenantId, IsActive = true, Website = "http://ghanteshwar.fwu.edu.np" },
            new College { Code = "SRC", Name = "Sitaram Multiple Campus", TenantId = oceTenantId, IsActive = true, Website = "http://sitaram.fwu.edu.np" },
            new College { Code = "JNC", Name = "Janata Multiple Campus", TenantId = oceTenantId, IsActive = true, Website = "http://janata.fwu.edu.np" },
            new College { Code = "JPC", Name = "Jayaprithivi Multiple Campus", TenantId = oceTenantId, IsActive = true, Website = "http://jayaprithivi.fwu.edu.np" },
            new College { Code = "BMC", Name = "Badimalika Campus", TenantId = oceTenantId, IsActive = true, Website = "http://badimalika.fwu.edu.np" },
            new College { Code = "MLC", Name = "Manilek Multiple Campus", TenantId = oceTenantId, IsActive = true, Website = "http://manilek.fwu.edu.np" },
            new College { Code = "PMC", Name = "Patan Multiple Campus", TenantId = oceTenantId, IsActive = true, Website = "http://patan.fwu.edu.np" },
            new College { Code = "JGC", Name = "Jagannath Multiple Campus", TenantId = oceTenantId, IsActive = true, Website = "http://jagannath.fwu.edu.np" },
            new College { Code = "GWC", Name = "Gokuleshwor Multiple Campus", TenantId = oceTenantId, IsActive = true, Website = "http://gokuleshwar.fwu.edu.np" },
            new College { Code = "KLC", Name = "Kailali Multiple Campus", TenantId = oceTenantId, IsActive = true, Website = "http://kailali.fwu.edu.np" },
        };

        foreach (var campus in campuses)
        {
            if (!await context.Colleges.AnyAsync(c => c.Code == campus.Code))
            {
                context.Colleges.Add(campus);
            }
        }
        await context.SaveChangesAsync();
    }

    public static async Task SeedESewaConfigurationAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (await context.ESewaConfigurations.AnyAsync())
            return;

        var eSewaConfig = new ESewaConfiguration
        {
            PostUrl = "https://rc-epay.esewa.com.np/api/epay/main/v2/form",
            ProductCode = "EPAYTEST",
            SecretKey = "8gBm/:&EnhH.1/q",
            SuccessUrl = "https://localhost:44333/Payment/Success",
            VerifyUrl = "https://rc-epay.esewa.com.np/api/epay/transaction/status/",
            ServiceChargeAmount = 0m,
        };
        context.ESewaConfigurations.Add(eSewaConfig);
        await context.SaveChangesAsync();
    }
}
