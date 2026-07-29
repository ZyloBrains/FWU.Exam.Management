using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class CollegeTypeServiceTests : TestBase
{
    [Fact]
    public async Task CreateCollegeType_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new CollegeTypeService(context);

        var ct = new CollegeType { Code = "UT", Name = "University", IsActive = true };
        await service.CreateCollegeTypeAsync(ct);

        var result = await service.GetCollegeTypeByIdAsync(ct.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("University");
    }

    [Fact]
    public async Task GetAllCollegeTypes_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<CollegeType>().AddRange(
            new CollegeType { Code = "UT", Name = "University", IsActive = true },
            new CollegeType { Code = "CT", Name = "Community", IsActive = true }
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new CollegeTypeService(context);
        var (items, totalCount) = await service.GetCollegeTypesAsync(1, 10, null, "name", "asc");

        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCollegeTypes_WithSearch_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<CollegeType>().AddRange(
            new CollegeType { Code = "UNI", Name = "University", IsActive = true },
            new CollegeType { Code = "COM", Name = "Community", IsActive = true }
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new CollegeTypeService(context);
        var (items, totalCount) = await service.GetCollegeTypesAsync(1, 10, "Uni", "name", "asc");

        totalCount.Should().Be(1);
        items[0].Name.Should().Be("University");
    }

    [Fact]
    public async Task UpdateCollegeType_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var ct = new CollegeType { Code = "OR", Name = "Original", IsActive = true };
        context.Set<CollegeType>().Add(ct);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new CollegeTypeService(context);

        var existing = await service.GetCollegeTypeByIdAsync(ct.Id);
        existing!.Name = "Updated";
        await service.UpdateCollegeTypeAsync(existing);

        context.ChangeTracker.Clear();
        var updated = await service.GetCollegeTypeByIdAsync(ct.Id);
        updated!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteCollegeType_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var ct = new CollegeType { Code = "DM", Name = "Delete Me", IsActive = true };
        context.Set<CollegeType>().Add(ct);
        await context.SaveChangesAsync();

        var service = new CollegeTypeService(context);
        await service.DeleteCollegeTypeAsync(ct.Id);

        var exists = await service.CollegeTypeExistsAsync(ct.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CreateCollegeType_WhenDefault_ShouldUnsetPreviousDefault()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var service = new CollegeTypeService(context);

        var first = new CollegeType { Code = "DF1", Name = "Default First", IsDefault = true, IsActive = true };
        await service.CreateCollegeTypeAsync(first);

        var second = new CollegeType { Code = "DF2", Name = "Default Second", IsDefault = true, IsActive = true };
        await service.CreateCollegeTypeAsync(second);

        context.ChangeTracker.Clear();
        var firstReload = await service.GetCollegeTypeByIdAsync(first.Id);
        firstReload!.IsDefault.Should().BeFalse();

        var secondReload = await service.GetCollegeTypeByIdAsync(second.Id);
        secondReload!.IsDefault.Should().BeTrue();
    }
}
