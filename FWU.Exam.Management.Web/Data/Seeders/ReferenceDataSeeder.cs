using FWU.Exam.Management.Domain.Entities;
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
                    Name = "Engineering Office",
                    OfficeCode = "ENG",
                    ContactNumber = "01-2345670",
                    Address = "Mahendranagar,Kanchanpur, Nepal",
                    Email = "eng@fwu.edu.np",
                    TenantType = TenantType.Central,
                    IsActive = true,
                },
            });
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedAdditionalReferenceDataAsync(IServiceProvider serviceProvider)
    {
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
