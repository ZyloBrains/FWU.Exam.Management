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
            new PreviousLevel { PreviousLevelName = "Bachelor", IsActive = true },
            new PreviousLevel { PreviousLevelName = "Master", IsActive = true },
        };
        await context.PreviousLevels.AddRangeAsync(previousLevels);

        // Levels
        var levels = new[]
        {
            new Level { LevelCode = "1", LevelName = "Undergraduate", LevelDisplayOrder = 1, IsActive = true },
            new Level { LevelCode = "2", LevelName = "Graduate", LevelDisplayOrder = 2, IsActive = true },
            new Level { LevelCode = "3", LevelName = "MPhil Leading to Ph.D", LevelDisplayOrder = 3, IsActive = true },
            new Level { LevelCode = "4", LevelName = "Ph.D.", LevelDisplayOrder = 4, IsActive = true },
        };
        await context.Levels.AddRangeAsync(levels);
        await context.SaveChangesAsync();

        // Colleges
        var org = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "L091");
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

        foe = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "L091");
        fst = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "L002");
        var foeMgt = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "L001");
        var foeEdu = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "L006");
        var foeHss = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "L003");

        var bachelorLevel = await context.Levels.FirstOrDefaultAsync(l => l.LevelCode == "1");

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
            var engProgramCodes = new[] { "L092", "L117", "L118" };
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
            var csitProgramCodes = new[] { "L008", "L016" };
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

        var mgtFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "L001");
        var eduFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "L006");
        var humFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "L003");
        var lawFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "L140");
        var agrFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "L103");

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

    public static async Task SeedSmsConfigurationAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (await context.SmsConfigurations.AnyAsync())
            return;

        var smsConfig = new SmsConfiguration
        {
            ApiUrl = "https://message.gumpnow.com/api/v1/sms/send/",
            ApiKey = "change-me",
            Mode = "prod",
            Tags = "entrance",
            IsActive = true
        };
        context.SmsConfigurations.Add(smsConfig);
        await context.SaveChangesAsync();
    }
}
