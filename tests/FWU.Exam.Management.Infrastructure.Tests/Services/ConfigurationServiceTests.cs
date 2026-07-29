using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class SmsConfigurationServiceTests : TestBase
{
    [Fact]
    public async Task CreateAsync_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new SmsConfigurationService(context);

        var config = new SmsConfiguration
        {
            ApiUrl = "https://api.gumpnow.com/sms",
            ApiKey = "test-key",
            Mode = "prod",
            Tags = "entrance",
            IsActive = true
        };

        await service.CreateAsync(config);

        var result = await service.GetByIdAsync(config.Id);
        result.Should().NotBeNull();
        result!.ApiUrl.Should().Be("https://api.gumpnow.com/sms");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAll()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new SmsConfigurationService(context);

        context.Set<SmsConfiguration>().Add(new SmsConfiguration { ApiUrl = "url1", ApiKey = "key1", IsActive = true });
        context.Set<SmsConfiguration>().Add(new SmsConfiguration { ApiUrl = "url2", ApiKey = "key2", IsActive = false });
        await context.SaveChangesAsync();

        var items = await service.GetAllAsync();

        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new SmsConfigurationService(context);

        var config = new SmsConfiguration { ApiUrl = "https://old.url", ApiKey = "old-key", IsActive = true };
        context.Set<SmsConfiguration>().Add(config);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        config.ApiUrl = "https://new.url";
        await service.UpdateAsync(config);

        var updated = await service.GetByIdAsync(config.Id);
        updated!.ApiUrl.Should().Be("https://new.url");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new SmsConfigurationService(context);

        var config = new SmsConfiguration { ApiUrl = "https://delete.url", ApiKey = "del-key", IsActive = true };
        context.Set<SmsConfiguration>().Add(config);
        await context.SaveChangesAsync();

        await service.DeleteAsync(config.Id);

        var result = await service.GetByIdAsync(config.Id);
        result.Should().BeNull();
    }
}

public class SmtpConfigurationServiceTests : TestBase
{
    [Fact]
    public async Task CreateSmtpConfigurationAsync_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new SmtpConfigurationService(context);

        var config = new SmtpConfiguration
        {
            Host = "smtp.test.com",
            From = "noreply@test.com",
            Port = 587,
            UserName = "user",
            Password = "pass",
            EnableSsl = true,
            IsActive = true
        };

        await service.CreateSmtpConfigurationAsync(config);

        var result = await service.GetSmtpConfigurationByIdAsync(config.Id);
        result.Should().NotBeNull();
        result!.Host.Should().Be("smtp.test.com");
    }

    [Fact]
    public async Task GetSmtpConfigurationsAsync_ShouldReturnPaged()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new SmtpConfigurationService(context);

        for (int i = 1; i <= 3; i++)
        {
            context.Set<SmtpConfiguration>().Add(new SmtpConfiguration
            {
                Host = $"smtp{i}.test.com", From = $"noreply{i}@test.com", Port = 587,
                UserName = $"user{i}", Password = "pass", IsActive = true
            });
        }
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetSmtpConfigurationsAsync(1, 2, null, "host", "asc");

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateSmtpConfigurationAsync_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new SmtpConfigurationService(context);

        var config = new SmtpConfiguration { Host = "old.test.com", From = "old@test.com", Port = 587, UserName = "old", Password = "pass", IsActive = true };
        context.Set<SmtpConfiguration>().Add(config);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        config.Host = "new.test.com";
        await service.UpdateSmtpConfigurationAsync(config);

        var updated = await service.GetSmtpConfigurationByIdAsync(config.Id);
        updated!.Host.Should().Be("new.test.com");
    }

    [Fact]
    public async Task DeleteSmtpConfigurationAsync_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new SmtpConfigurationService(context);

        var config = new SmtpConfiguration { Host = "del.test.com", From = "del@test.com", Port = 587, UserName = "del", Password = "pass", IsActive = true };
        context.Set<SmtpConfiguration>().Add(config);
        await context.SaveChangesAsync();

        await service.DeleteSmtpConfigurationAsync(config.Id);

        var exists = await service.SmtpConfigurationExistsAsync(config.Id);
        exists.Should().BeFalse();
    }
}
