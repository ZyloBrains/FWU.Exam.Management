using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class DistrictServiceTests : TestBase
{
    private async Task SeedProvinceAsync(AppDbContext context)
    {
        context.Set<Province>().Add(new Province { ProvinceName = "Sudurpashchim", ProvinceCode = "SUD", IsActive = true });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateDistrict_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedProvinceAsync(context);
        var service = new DistrictService(context);

        var district = new District
        {
            ProvinceId = 1,
            DistrictCode = "KAN",
            DistrictName = "Kanchanpur",
            IsActive = true
        };

        await service.CreateDistrictAsync(district);

        var result = await service.GetDistrictByIdAsync(district.Id);
        result.Should().NotBeNull();
        result!.DistrictName.Should().Be("Kanchanpur");
    }

    [Fact]
    public async Task GetDistricts_ShouldReturnPaged()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedProvinceAsync(context);

        context.Set<District>().Add(new District { ProvinceId = 1, DistrictCode = "KAN", DistrictName = "Kanchanpur", IsActive = true });
        context.Set<District>().Add(new District { ProvinceId = 1, DistrictCode = "KAI", DistrictName = "Kailali", IsActive = true });
        context.Set<District>().Add(new District { ProvinceId = 1, DistrictCode = "DOT", DistrictName = "Doti", IsActive = true });
        await context.SaveChangesAsync();

        var service = new DistrictService(context);

        var (items, totalCount) = await service.GetDistrictsAsync(1, 2, null, "DistrictName", "asc");

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDistricts_WithSearch_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedProvinceAsync(context);

        context.Set<District>().Add(new District { ProvinceId = 1, DistrictCode = "KAN", DistrictName = "Kanchanpur", IsActive = true });
        context.Set<District>().Add(new District { ProvinceId = 1, DistrictCode = "KAI", DistrictName = "Kailali", IsActive = true });
        await context.SaveChangesAsync();

        var service = new DistrictService(context);

        var (items, totalCount) = await service.GetDistrictsAsync(1, 10, "Kailali", "DistrictName", "asc");

        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
        items[0].DistrictName.Should().Be("Kailali");
    }

    [Fact]
    public async Task UpdateDistrict_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedProvinceAsync(context);

        context.Set<District>().Add(new District { ProvinceId = 1, DistrictCode = "KAN", DistrictName = "Kanchanpur", IsActive = true });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new DistrictService(context);

        var entity = await service.GetDistrictByIdAsync(1);
        entity.Should().NotBeNull();

        entity!.DistrictName = "Updated";
        await service.UpdateDistrictAsync(entity);

        context.ChangeTracker.Clear();
        var updated = await service.GetDistrictByIdAsync(1);
        updated!.DistrictName.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteDistrict_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedProvinceAsync(context);

        context.Set<District>().Add(new District { ProvinceId = 1, DistrictCode = "KAN", DistrictName = "Kanchanpur", IsActive = true });
        await context.SaveChangesAsync();

        var service = new DistrictService(context);
        await service.DeleteDistrictAsync(1);

        var exists = await service.DistrictExistsAsync(1);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetActiveProvinces_ShouldReturnActiveOnly()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<Province>().Add(new Province { ProvinceName = "Province 1", ProvinceCode = "P1", IsActive = true });
        context.Set<Province>().Add(new Province { ProvinceName = "Province 2", ProvinceCode = "P2", IsActive = false });
        await context.SaveChangesAsync();

        var service = new DistrictService(context);

        var provinces = await service.GetActiveProvincesAsync();

        provinces.Should().HaveCount(1);
        provinces[0].ProvinceCode.Should().Be("P1");
    }
}
