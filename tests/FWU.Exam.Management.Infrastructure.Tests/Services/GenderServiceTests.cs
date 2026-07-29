using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class GenderServiceTests : TestBase
{
    [Fact]
    public async Task CreateGender_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new GenderService(context);

        var gender = new Gender { GenderName = "Male", IsActive = true };

        await service.CreateGenderAsync(gender);

        var result = await service.GetGenderByIdAsync(gender.Id);
        result.Should().NotBeNull();
        result!.GenderName.Should().Be("Male");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetGendersAsync_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new GenderService(context);

        context.Set<Gender>().Add(new Gender { GenderName = "Male", IsActive = true });
        context.Set<Gender>().Add(new Gender { GenderName = "Female", IsActive = true });
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetGendersAsync(1, 10, null, "gendername", "asc");

        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateGender_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var gender = new Gender { GenderName = "Old", IsActive = true };
        context.Set<Gender>().Add(gender);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var service = new GenderService(context);

        gender.GenderName = "Updated";
        await service.UpdateGenderAsync(gender);

        context.ChangeTracker.Clear();
        var result = await service.GetGenderByIdAsync(gender.Id);
        result!.GenderName.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteGender_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var gender = new Gender { GenderName = "Other", IsActive = true };
        context.Set<Gender>().Add(gender);
        await context.SaveChangesAsync();

        var service = new GenderService(context);
        await service.DeleteGenderAsync(gender.Id);

        var exists = await service.GenderExistsAsync(gender.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GenderExists_ShouldReturnFalse_WhenNotExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new GenderService(context);

        var exists = await service.GenderExistsAsync(999);

        exists.Should().BeFalse();
    }
}
