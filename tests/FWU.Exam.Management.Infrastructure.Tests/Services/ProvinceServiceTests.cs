using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class ProvinceServiceTests : TestBase
{
    [Fact]
    public async Task CreateProvince_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new ProvinceService(context);

        var province = new Province
        {
            ProvinceName = "Sudurpashchim",
            ProvinceCode = "SP",
            IsActive = true
        };

        await service.CreateProvinceAsync(province);

        var result = await service.GetProvinceByIdAsync(province.Id);
        result.Should().NotBeNull();
        result!.ProvinceName.Should().Be("Sudurpashchim");
    }

    [Fact]
    public async Task GetProvincesAsync_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new ProvinceService(context);

        context.Set<Province>().Add(new Province { ProvinceName = "Province 1", ProvinceCode = "P1", IsActive = true });
        context.Set<Province>().Add(new Province { ProvinceName = "Bagmati", ProvinceCode = "BA", IsActive = true });
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetProvincesAsync(1, 10, null, "provincename", "asc");

        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateProvince_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var province = new Province { ProvinceName = "Old Name", ProvinceCode = "ON", IsActive = true };
        context.Set<Province>().Add(province);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var service = new ProvinceService(context);

        province.ProvinceName = "Updated Name";
        await service.UpdateProvinceAsync(province);

        context.ChangeTracker.Clear();
        var result = await service.GetProvinceByIdAsync(province.Id);
        result!.ProvinceName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task DeleteProvince_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var province = new Province { ProvinceName = "To Delete", ProvinceCode = "TD", IsActive = true };
        context.Set<Province>().Add(province);
        await context.SaveChangesAsync();

        var service = new ProvinceService(context);
        await service.DeleteProvinceAsync(province.Id);

        var exists = await service.ProvinceExistsAsync(province.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ProvinceExists_ShouldReturnFalse_WhenNotExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new ProvinceService(context);

        var exists = await service.ProvinceExistsAsync(999);

        exists.Should().BeFalse();
    }
}
