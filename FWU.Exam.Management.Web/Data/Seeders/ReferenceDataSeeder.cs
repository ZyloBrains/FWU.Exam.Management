using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Payments;
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
            var faculties = new[]
            {
                new Faculty
                {
                    Name = "School of Engineering",
                    OfficeCode = "SOE",
                    ContactNumber = "021-123456",
                    Address = "Mahendranagar, Kanchanpur",
                    Email = "soe@fwu.edu.np",
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
                IsActive = true,
                FacultyId = org?.Id,
            },
            new College
            {
                Code = "SOM",
                Name = "School of Management",
                IsActive = true,
                FacultyId = org?.Id,
            },
        };
        await context.Colleges.AddRangeAsync(colleges);
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
