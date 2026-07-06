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

        var existingTypes = await context.Set<PaymentType>().ToListAsync();
        var seedTypes = new Dictionary<string, (string logoUrl, bool isActive)>
        {
            ["eSewa"] = ("https://upload.wikimedia.org/wikipedia/commons/5/5c/ESewa_Logo.png", true),
            ["Khalti"] = ("https://khalti.com/static/images/khalti-icon.png", true),
            ["ConnectIPS"] = ("https://www.connectips.com/wp-content/uploads/2021/07/connect-ips-logo.png", true),
        };

        foreach (var (name, (logoUrl, isActive)) in seedTypes)
        {
            var existing = existingTypes.FirstOrDefault(pt => string.Equals(pt.PaymentTypeName, name, StringComparison.OrdinalIgnoreCase));
            if (existing == null)
            {
                context.Set<PaymentType>().Add(new PaymentType
                {
                    PaymentTypeName = name,
                    LogoUrl = logoUrl,
                    IsActive = isActive
                });
            }
            else if (string.IsNullOrEmpty(existing.LogoUrl))
            {
                existing.LogoUrl = logoUrl;
            }
        }
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
            new PreviousLevel { PreviousLevelName = "Diploma / PCL", IsActive = true },
            new PreviousLevel { PreviousLevelName = "Bachelor", IsActive = true },
            new PreviousLevel { PreviousLevelName = "Master", IsActive = true },
            new PreviousLevel { PreviousLevelName = "MPhil", IsActive = true },
            new PreviousLevel { PreviousLevelName = "PhD", IsActive = true },
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
        var collegeCoc = new College
        {
            Code = "COC",
            Name = "College of Commerce",
            TenantId = 1,
            IsActive = true,
        };
        var collegeSom = new College
        {
            Code = "SOM",
            Name = "School of Management",
            TenantId = 1,
            IsActive = true,
        };
        context.Colleges.AddRange(collegeCoc, collegeSom);
        await context.SaveChangesAsync();

        if (org != null)
        {
            collegeCoc.Faculties = new List<Faculty> { org };
            collegeSom.Faculties = new List<Faculty> { org };
            await context.SaveChangesAsync();
        }
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
            new Faculty { Name = "Faculty of Engineering", OfficeCode = "ENG", ContactNumber = "099-520296", Address = "Mahendranagar, Kanchanpur", Email = "dean.engineering@fwu.edu.np", TenantId = engTenant?.Id },
            new Faculty { Name = "Faculty of Science and Technology", OfficeCode = "FST", ContactNumber = "099-524000", Address = "Mahendranagar, Kanchanpur", Email = "faculty.science@fwu.edu.np" },
            new Faculty { Name = "Faculty of Law", OfficeCode = "FOL", ContactNumber = "099-520729", Address = "Mahendranagar, Kanchanpur", Email = "law@fwu.edu.np" },
            new Faculty { Name = "Faculty of Humanities", OfficeCode = "FO-HSS", ContactNumber = "099-520729", Address = "Mahendranagar, Kanchanpur", Email = "humanities@fwu.edu.np" },
            new Faculty { Name = "Faculty of Education", OfficeCode = "EDU", ContactNumber = "099-520729", Address = "Mahendranagar, Kanchanpur", Email = "education@fwu.edu.np" },
            new Faculty { Name = "Faculty of Management", OfficeCode = "FO-MGT", ContactNumber = "099-520729", Address = "Mahendranagar, Kanchanpur", Email = "management@fwu.edu.np" },
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
            else
            {
                existing.Name = faculty.Name;
                existing.ContactNumber = faculty.ContactNumber;
                existing.Address = faculty.Address;
                existing.Email = faculty.Email;
                existing.TenantId = faculty.TenantId;
            }
        }
        await context.SaveChangesAsync();

        foe = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "ENG");
        fst = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "FST");
        var foeMgt = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "FO-MGT");
        var foeEdu = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "EDU");
        var foeHss = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "FO-HSS");

        // Link departments to their faculties
        var deptMgt = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "MGMT");
        if (deptMgt != null && foeMgt != null) deptMgt.FacultyId = foeMgt.Id;

        var deptSci = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "SCI");
        if (deptSci != null && fst != null) deptSci.FacultyId = fst.Id;

        var deptEdu = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "EDU");
        if (deptEdu != null && foeEdu != null) deptEdu.FacultyId = foeEdu.Id;

        var deptHum = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "HUM");
        if (deptHum != null && foeHss != null) deptHum.FacultyId = foeHss.Id;

        await context.SaveChangesAsync();

        Department? enggDept;
        if (!await context.Departments.AnyAsync(d => d.DepartmentCode == "ENGG"))
        {
            enggDept = new Department { DepartmentCode = "ENGG", DepartmentName = "Engineering", ShortName = "ENG", IsActive = true, FacultyId = foe?.Id };
            context.Departments.Add(enggDept);
            await context.SaveChangesAsync();
        }
        else
        {
            enggDept = await context.Departments.FirstOrDefaultAsync(d => d.DepartmentCode == "ENGG");
            if (enggDept != null && foe != null && enggDept.FacultyId == null)
                enggDept.FacultyId = foe.Id;
            await context.SaveChangesAsync();
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
        if (!await context.Colleges.IgnoreQueryFilters().AnyAsync(c => c.Code == "ENG-SOE"))
        {
            engCollege = new College { Code = "ENG-SOE", Name = "School of Engineering", TenantId = 1, IsActive = true };
            context.Colleges.Add(engCollege);
            await context.SaveChangesAsync();
            if (foe != null)
            {
                engCollege.Faculties = new List<Faculty> { foe };
                await context.SaveChangesAsync();
            }
        }
        else
        {
            engCollege = await context.Colleges.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Code == "ENG-SOE");
        }

        if (!await context.Colleges.IgnoreQueryFilters().AnyAsync(c => c.Code == "CDC-CSIT"))
        {
            csitCollege = new College { Code = "CDC-CSIT", Name = "Central Department of Computer Science & IT", TenantId = 1, IsActive = true };
            context.Colleges.Add(csitCollege);
            await context.SaveChangesAsync();
            if (fst != null)
            {
                csitCollege.Faculties = new List<Faculty> { fst };
                await context.SaveChangesAsync();
            }
        }
        else
        {
            csitCollege = await context.Colleges.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Code == "CDC-CSIT");
        }

        if (engCollege != null)
        {
            var engProgramCodes = new[] { "BECT", "BECP", "BARC" };
            var engPrograms = await context.Programs.Where(p => engProgramCodes.Contains(p.ProgramCode)).ToListAsync();
            foreach (var program in engPrograms)
            {
                if (!await context.CollegePrograms.IgnoreQueryFilters().AnyAsync(cp => cp.CollegeId == engCollege.Id && cp.ProgramId == program.Id))
                {
                    context.CollegePrograms.Add(new CollegeProgram { CollegeId = engCollege.Id, ProgramId = program.Id, TenantId = 1, IsActive = true });
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
                if (!await context.CollegePrograms.IgnoreQueryFilters().AnyAsync(cp => cp.CollegeId == csitCollege.Id && cp.ProgramId == program.Id))
                {
                    context.CollegePrograms.Add(new CollegeProgram { CollegeId = csitCollege.Id, ProgramId = program.Id, TenantId = 1, IsActive = true });
                }
            }
            await context.SaveChangesAsync();
        }

        var oceTenantId = 1;

        var mgtFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "FO-MGT");
        var eduFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "EDU");
        var humFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "FO-HSS");
        var lawFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "FOL");
        var agrFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "AGR");

        var centralDepartmentDefs = new[]
        {
            new { Code = "CDC-CSIT", Name = "Central Department of Computer Science and Information Technology", Website = "http://cdcsit.fwu.edu.np", FacultyRef = fst },
            new { Code = "DGS", Name = "Central Department of General Science", Website = "http://science.fwu.edu.np", FacultyRef = fst },
            new { Code = "CDM", Name = "Central Department of Management", Website = "http://management.fwu.edu.np", FacultyRef = mgtFaculty },
            new { Code = "CDE", Name = "Central Department of Education", Website = "http://education.fwu.edu.np", FacultyRef = eduFaculty },
            new { Code = "CDH", Name = "Central Department of Humanities", Website = "http://humanities.fwu.edu.np", FacultyRef = humFaculty },
            new { Code = "CDL", Name = "Central Department of Law", Website = "http://agriculture.fwu.edu.np", FacultyRef = lawFaculty },
            new { Code = "CDA", Name = "Central Department of Agriculture", Website = "http://law.fwu.edu.np", FacultyRef = agrFaculty },
        };

        foreach (var def in centralDepartmentDefs)
        {
            if (!await context.Colleges.IgnoreQueryFilters().AnyAsync(c => c.Code == def.Code))
            {
                var dept = new College { Code = def.Code, Name = def.Name, TenantId = oceTenantId, IsActive = true, Website = def.Website };
                context.Colleges.Add(dept);
                await context.SaveChangesAsync();
                if (def.FacultyRef != null)
                {
                    dept.Faculties = new List<Faculty> { def.FacultyRef };
                    await context.SaveChangesAsync();
                }
            }
        }

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
            if (!await context.Colleges.IgnoreQueryFilters().AnyAsync(c => c.Code == campus.Code))
            {
                context.Colleges.Add(campus);
            }
        }
        await context.SaveChangesAsync();

        // Previous Levels (upsert)
        var previousLevels = new[]
        {
            "SEE (Grade 10)",
            "+2 / 10+2",
            "Diploma / PCL",
            "Bachelor",
            "Master",
            "MPhil",
            "PhD",
        };
        foreach (var name in previousLevels)
        {
            if (!await context.PreviousLevels.AnyAsync(pl => pl.PreviousLevelName == name))
            {
                context.PreviousLevels.Add(new PreviousLevel { PreviousLevelName = name, IsActive = true });
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

    public static async Task SeedKhaltiConfigurationAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (await context.KhaltiConfigurations.AnyAsync())
            return;

        var khaltiConfig = new KhaltiConfiguration
        {
            ReturnUrl = "https://localhost:44333/Payment/KhaltiCallback",
            WebsiteUrl = "https://example.com",
            Amount = 0m,
            ProductName = "Exam Fee",
            AuthorizationKey = "test_secret_key",
            ServiceCharge = 0,
            PostUrl = "https://rc-epay.khalti.com/api/v2/epayment/initiate/",
            VerifyUrl = "https://rc-epay.khalti.com/api/v2/epayment/lookup/",
        };
        context.KhaltiConfigurations.Add(khaltiConfig);
        await context.SaveChangesAsync();
    }

    public static async Task SeedConnectIPSConfigurationAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (await context.ConnectIpsPaymentConfigurations.AnyAsync())
            return;

        var connectIpsConfig = new ConnectIpsPaymentConfiguration
        {
            GatewayUrl = "https://connectips.example.com/gateway",
            MerchantId = "TEST_MERCHANT",
            AppId = "TEST_APP_ID",
            AppName = "Exam Management",
            ValidationApiUrl = "https://connectips.example.com/api/validate",
            UsernameForValidationApi = "test_user",
            PasswordForValidationApi = "test_pass",
            PasswordForCreditorPfx = "test_pfx_pass",
            TransactionCurrency = "NPR",
        };
        context.ConnectIpsPaymentConfigurations.Add(connectIpsConfig);
        await context.SaveChangesAsync();
    }
}
