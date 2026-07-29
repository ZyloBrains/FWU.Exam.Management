using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class AcademicYearServiceTests : TestBase
{
    [Fact]
    public async Task CreateAcademicYear_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new AcademicYearService(context);

        var year = new AcademicYear
        {
            AcademicYearCode = "2082/083",
            AcademicYearName = "2082/083",
            AcademicYearCodeNepali = "२०८२/०८३",
            AcademicYearNameNepali = "२०८२/०८३",
            IsRunning = true,
            IsActive = true
        };

        await service.CreateAcademicYearAsync(year);

        var result = await service.GetAcademicYearByIdAsync(year.Id);
        result.Should().NotBeNull();
        result!.AcademicYearCode.Should().Be("2082/083");
        result.IsRunning.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAcademicYears_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedAcademicYearAsync(context);
        var service = new AcademicYearService(context);

        var (items, totalCount) = await service.GetAllAcademicYearsAsync(1, 10, null);

        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
        items[0].AcademicYearCode.Should().Be("2081/082");
    }

    [Fact]
    public async Task GetAllAcademicYears_WithSearch_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedAcademicYearAsync(context);

        context.Set<AcademicYear>().Add(new AcademicYear
        {
            AcademicYearCode = "2080/081",
            AcademicYearName = "2080/081",
            AcademicYearCodeNepali = "२०८०/०८१",
            AcademicYearNameNepali = "२०८०/०८१",
            IsRunning = false,
            IsActive = false
        });
        await context.SaveChangesAsync();

        var service = new AcademicYearService(context);

        var (items, totalCount) = await service.GetAllAcademicYearsAsync(1, 10, "2081");

        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
        items[0].AcademicYearCode.Should().Be("2081/082");
    }

    [Fact]
    public async Task UpdateAcademicYear_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedAcademicYearAsync(context);
        context.ChangeTracker.Clear();
        var service = new AcademicYearService(context);

        var year = await service.GetAcademicYearByIdAsync(1);
        year.Should().NotBeNull();

        year!.IsRunning = false;
        await service.UpdateAcademicYearAsync(year);

        context.ChangeTracker.Clear();
        var updated = await service.GetAcademicYearByIdAsync(1);
        updated!.IsRunning.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAcademicYear_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedAcademicYearAsync(context);
        var service = new AcademicYearService(context);

        await service.DeleteAcademicYearAsync(1);

        var exists = await service.AcademicYearExistsAsync(1);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task AcademicYearExists_ShouldReturnTrue_WhenExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedAcademicYearAsync(context);
        var service = new AcademicYearService(context);

        var exists = await service.AcademicYearExistsAsync(1);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task AcademicYearExists_ShouldReturnFalse_WhenNotExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new AcademicYearService(context);

        var exists = await service.AcademicYearExistsAsync(999);

        exists.Should().BeFalse();
    }
}
