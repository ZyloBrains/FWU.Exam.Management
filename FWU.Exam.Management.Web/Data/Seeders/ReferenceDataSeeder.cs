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

        if (!await context.Faculties.AnyAsync(f => f.OfficeCode == "FOE"))
        {
            foe = new Faculty { Name = "Faculty of Engineering", OfficeCode = "FOE", ContactNumber = "099-520296", Address = "Mahendranagar, Kanchanpur", Email = "dean.engineering@fwu.edu.np", TenantId = engTenant?.Id };
            context.Faculties.Add(foe);
            await context.SaveChangesAsync();
        }
        else
        {
            foe = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "FOE");
        }

        if (!await context.Faculties.AnyAsync(f => f.OfficeCode == "FST"))
        {
            fst = new Faculty { Name = "Faculty of Science & Technology", OfficeCode = "FST", ContactNumber = "099-524000", Address = "Mahendranagar, Kanchanpur", Email = "faculty.science@fwu.edu.np" };
            context.Faculties.Add(fst);
            await context.SaveChangesAsync();
        }
        else
        {
            fst = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "FST");
        }

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
