using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class ExamTypeServiceTests : TestBase
{
    [Fact]
    public async Task CreateExamType_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new ExamTypeService(context);

        var examType = new ExamType
        {
            Name = "Regular",
            Code = "REG",
            IsActive = true
        };

        await service.CreateExamTypeAsync(examType);

        var result = await service.GetExamTypeByIdAsync(examType.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Regular");
    }

    [Fact]
    public async Task GetExamTypes_ShouldReturnPaged()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<ExamType>().Add(new ExamType { Name = "Regular", Code = "REG", IsActive = true });
        context.Set<ExamType>().Add(new ExamType { Name = "Back", Code = "BCK", IsActive = true });
        context.Set<ExamType>().Add(new ExamType { Name = "Final", Code = "FIN", IsActive = false });
        await context.SaveChangesAsync();

        var service = new ExamTypeService(context);

        var (items, totalCount) = await service.GetExamTypesAsync(1, 2, null, "Name", "asc");

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetExamTypes_WithSearch_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<ExamType>().Add(new ExamType { Name = "Regular", Code = "REG", IsActive = true });
        context.Set<ExamType>().Add(new ExamType { Name = "Back", Code = "BCK", IsActive = true });
        await context.SaveChangesAsync();

        var service = new ExamTypeService(context);

        var (items, totalCount) = await service.GetExamTypesAsync(1, 10, "Regular", "Name", "asc");

        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
        items[0].Name.Should().Be("Regular");
    }

    [Fact]
    public async Task UpdateExamType_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<ExamType>().Add(new ExamType { Name = "Original", Code = "ORG", IsActive = true });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new ExamTypeService(context);

        var entity = await service.GetExamTypeByIdAsync(1);
        entity.Should().NotBeNull();

        entity!.Name = "Updated";
        await service.UpdateExamTypeAsync(entity);

        context.ChangeTracker.Clear();
        var updated = await service.GetExamTypeByIdAsync(1);
        updated!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteExamType_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<ExamType>().Add(new ExamType { Name = "ToDelete", Code = "DEL", IsActive = true });
        await context.SaveChangesAsync();

        var service = new ExamTypeService(context);
        await service.DeleteExamTypeAsync(1);

        var exists = await service.ExamTypeExistsAsync(1);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExamTypeExists_ShouldReturnTrue_WhenExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<ExamType>().Add(new ExamType { Name = "Exists", Code = "EXT", IsActive = true });
        await context.SaveChangesAsync();

        var service = new ExamTypeService(context);

        var exists = await service.ExamTypeExistsAsync(1);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetFilteredItems_ShouldReturnAll()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<ExamType>().Add(new ExamType { Name = "Regular", Code = "REG", IsActive = true });
        context.Set<ExamType>().Add(new ExamType { Name = "Back", Code = "BCK", IsActive = false });
        await context.SaveChangesAsync();

        var service = new ExamTypeService(context);

        var items = await service.GetFilteredItemsAsync(1, 10, null, "Name", "asc");

        items.Should().HaveCount(2);
    }
}
