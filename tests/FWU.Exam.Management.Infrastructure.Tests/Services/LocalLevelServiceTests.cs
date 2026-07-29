using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class LocalLevelServiceTests : TestBase
{
    [Fact]
    public async Task CreateLocalLevel_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new LocalLevelService(context);

        var province = new Province { ProvinceName = "Sudurpashchim", ProvinceCode = "SP", IsActive = true };
        context.Set<Province>().Add(province);
        await context.SaveChangesAsync();

        var district = new District { DistrictName = "Kailali", ProvinceId = province.Id, IsActive = true };
        context.Set<District>().Add(district);
        await context.SaveChangesAsync();

        var localLevel = new LocalLevel
        {
            DistrictId = district.Id,
            LocalLevelName = "Dhangadhi Sub-Metropolitan",
            LocalLevelType = LocalLevelType.SubMetropolitan,
            IsActive = true
        };

        await service.CreateLocalLevelAsync(localLevel);

        var result = await service.GetLocalLevelByIdAsync(localLevel.Id);
        result.Should().NotBeNull();
        result!.LocalLevelName.Should().Be("Dhangadhi Sub-Metropolitan");
        result.District.Should().NotBeNull();
        result.District!.DistrictName.Should().Be("Kailali");
    }

    [Fact]
    public async Task GetLocalLevelsAsync_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new LocalLevelService(context);

        var province = new Province { ProvinceName = "Province 1", ProvinceCode = "P1", IsActive = true };
        context.Set<Province>().Add(province);
        await context.SaveChangesAsync();

        var district = new District { DistrictName = "Morang", ProvinceId = province.Id, IsActive = true };
        context.Set<District>().Add(district);
        await context.SaveChangesAsync();

        context.Set<LocalLevel>().Add(new LocalLevel { DistrictId = district.Id, LocalLevelName = "Biratnagar Mahanagarpalika", LocalLevelType = LocalLevelType.Metropolitan, IsActive = true });
        context.Set<LocalLevel>().Add(new LocalLevel { DistrictId = district.Id, LocalLevelName = "Sundar Haraicha", LocalLevelType = LocalLevelType.Municipality, IsActive = true });
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetLocalLevelsAsync(1, 10, null, "locallevelname", "asc");

        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateLocalLevel_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var provinceId = await SeedProvinceAsync(context);

        var district = new District { DistrictName = "Kanchanpur", ProvinceId = provinceId, IsActive = true };
        context.Set<District>().Add(district);
        await context.SaveChangesAsync();

        var localLevel = new LocalLevel { DistrictId = district.Id, LocalLevelName = "Mahendranagar", LocalLevelType = LocalLevelType.Municipality, IsActive = true };
        context.Set<LocalLevel>().Add(localLevel);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var service = new LocalLevelService(context);

        localLevel.LocalLevelName = "Bhimdatta";
        await service.UpdateLocalLevelAsync(localLevel);

        context.ChangeTracker.Clear();
        var result = await service.GetLocalLevelByIdAsync(localLevel.Id);
        result!.LocalLevelName.Should().Be("Bhimdatta");
    }

    [Fact]
    public async Task DeleteLocalLevel_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var provinceId = await SeedProvinceAsync(context);

        var district = new District { DistrictName = "Doti", ProvinceId = provinceId, IsActive = true };
        context.Set<District>().Add(district);
        await context.SaveChangesAsync();

        var localLevel = new LocalLevel { DistrictId = district.Id, LocalLevelName = "Dipayal Silgadhi", LocalLevelType = LocalLevelType.Municipality, IsActive = true };
        context.Set<LocalLevel>().Add(localLevel);
        await context.SaveChangesAsync();

        var service = new LocalLevelService(context);
        await service.DeleteLocalLevelAsync(localLevel.Id);

        var exists = await service.LocalLevelExistsAsync(localLevel.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetActiveDistrictsAsync_ShouldReturnActiveOnly()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var provinceId = await SeedProvinceAsync(context);

        context.Set<District>().Add(new District { DistrictName = "Kailali", ProvinceId = provinceId, IsActive = true });
        context.Set<District>().Add(new District { DistrictName = "Kanchanpur", ProvinceId = provinceId, IsActive = true });
        context.Set<District>().Add(new District { DistrictName = "Inactive", ProvinceId = provinceId, IsActive = false });
        await context.SaveChangesAsync();

        var service = new LocalLevelService(context);
        var districts = await service.GetActiveDistrictsAsync();

        districts.Should().HaveCount(2);
        districts.Should().NotContain(d => d.DistrictName == "Inactive");
    }
}
