using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class EthnicityServiceTests : TestBase
{
    [Fact]
    public async Task CreateEthnicity_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new EthnicityService(context);

        var ethnicity = new Ethnicity
        {
            EthnicityName = "Tharu",
            IsDefault = false,
            IsActive = true
        };

        await service.CreateEthnicityAsync(ethnicity);

        var result = await service.GetEthnicityByIdAsync(ethnicity.Id);
        result.Should().NotBeNull();
        result!.EthnicityName.Should().Be("Tharu");
    }

    [Fact]
    public async Task GetEthnicities_ShouldReturnPaged()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<Ethnicity>().Add(new Ethnicity { EthnicityName = "Tharu", IsDefault = true, IsActive = true });
        context.Set<Ethnicity>().Add(new Ethnicity { EthnicityName = "Brahmin", IsDefault = false, IsActive = true });
        context.Set<Ethnicity>().Add(new Ethnicity { EthnicityName = "Chhetri", IsDefault = false, IsActive = false });
        await context.SaveChangesAsync();

        var service = new EthnicityService(context);

        var (items, totalCount) = await service.GetEthnicitiesAsync(1, 2, null, "EthnicityName", "asc");

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEthnicities_WithSearch_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<Ethnicity>().Add(new Ethnicity { EthnicityName = "Tharu", IsDefault = false, IsActive = true });
        context.Set<Ethnicity>().Add(new Ethnicity { EthnicityName = "Brahmin", IsDefault = false, IsActive = true });
        await context.SaveChangesAsync();

        var service = new EthnicityService(context);

        var (items, totalCount) = await service.GetEthnicitiesAsync(1, 10, "Brahmin", "EthnicityName", "asc");

        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
        items[0].EthnicityName.Should().Be("Brahmin");
    }

    [Fact]
    public async Task UpdateEthnicity_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<Ethnicity>().Add(new Ethnicity { EthnicityName = "Original", IsDefault = false, IsActive = true });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new EthnicityService(context);

        var entity = await service.GetEthnicityByIdAsync(1);
        entity.Should().NotBeNull();

        entity!.EthnicityName = "Updated";
        await service.UpdateEthnicityAsync(entity);

        context.ChangeTracker.Clear();
        var updated = await service.GetEthnicityByIdAsync(1);
        updated!.EthnicityName.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteEthnicity_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<Ethnicity>().Add(new Ethnicity { EthnicityName = "ToDelete", IsDefault = false, IsActive = true });
        await context.SaveChangesAsync();

        var service = new EthnicityService(context);
        await service.DeleteEthnicityAsync(1);

        var exists = await service.EthnicityExistsAsync(1);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task EthnicityExists_ShouldReturnTrue_WhenExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<Ethnicity>().Add(new Ethnicity { EthnicityName = "Tharu", IsDefault = false, IsActive = true });
        await context.SaveChangesAsync();

        var service = new EthnicityService(context);

        var exists = await service.EthnicityExistsAsync(1);
        exists.Should().BeTrue();
    }
}
