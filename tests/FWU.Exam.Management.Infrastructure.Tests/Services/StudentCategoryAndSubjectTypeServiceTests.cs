using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class StudentCategoryServiceTests : TestBase
{
    [Fact]
    public async Task CreateStudentCategoryAsync_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new StudentCategoryService(context);

        var category = new StudentCategory
        {
            StudentCategoryName = "Scholarship",
            IsActive = true
        };

        await service.CreateStudentCategoryAsync(category);

        var result = await service.GetStudentCategoryByIdAsync(category.Id);
        result.Should().NotBeNull();
        result!.StudentCategoryName.Should().Be("Scholarship");
    }

    [Fact]
    public async Task CreateStudentCategoryAsync_ShouldThrow_WhenDuplicate()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new StudentCategoryService(context);

        context.Set<StudentCategory>().Add(new StudentCategory { StudentCategoryName = "Scholarship", IsActive = true });
        await context.SaveChangesAsync();

        var duplicate = new StudentCategory { StudentCategoryName = "Scholarship", IsActive = true };

        await service.Invoking(s => s.CreateStudentCategoryAsync(duplicate))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task GetStudentCategoriesAsync_ShouldReturnPaged()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new StudentCategoryService(context);

        for (int i = 1; i <= 3; i++)
        {
            context.Set<StudentCategory>().Add(new StudentCategory { StudentCategoryName = $"Category {i}", IsActive = true });
        }
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetStudentCategoriesAsync(1, 2, null, "name", "asc");

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateStudentCategoryAsync_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new StudentCategoryService(context);

        var category = new StudentCategory { StudentCategoryName = "Old", IsActive = true };
        context.Set<StudentCategory>().Add(category);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        category.StudentCategoryName = "Updated";
        await service.UpdateStudentCategoryAsync(category);

        var updated = await service.GetStudentCategoryByIdAsync(category.Id);
        updated!.StudentCategoryName.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteStudentCategoryAsync_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new StudentCategoryService(context);

        var category = new StudentCategory { StudentCategoryName = "To Delete", IsActive = true };
        context.Set<StudentCategory>().Add(category);
        await context.SaveChangesAsync();

        await service.DeleteStudentCategoryAsync(category.Id);

        var exists = await service.StudentCategoryExistsAsync(category.Id);
        exists.Should().BeFalse();
    }
}

public class SubjectTypeServiceTests : TestBase
{
    [Fact]
    public async Task CreateSubjectTypeAsync_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new SubjectTypeService(context);

        var subjectType = new SubjectType
        {
            Code = "TH",
            Name = "Theory",
            IsActive = true
        };

        await service.CreateSubjectTypeAsync(subjectType);

        var result = await service.GetSubjectTypeByIdAsync(subjectType.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Theory");
    }

    [Fact]
    public async Task CreateSubjectTypeAsync_WithIsDefault_ShouldUnsetPreviousDefault()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new SubjectTypeService(context);

        var first = new SubjectType { Code = "T1", Name = "Type 1", IsDefault = true, IsActive = true };
        context.Set<SubjectType>().Add(first);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var second = new SubjectType { Code = "T2", Name = "Type 2", IsDefault = true, IsActive = true };
        await service.CreateSubjectTypeAsync(second);

        var updatedFirst = await service.GetSubjectTypeByIdAsync(first.Id);
        updatedFirst!.IsDefault.Should().BeFalse();
        var result = await service.GetSubjectTypeByIdAsync(second.Id);
        result!.IsDefault.Should().BeTrue();
    }

    [Fact]
    public async Task GetSubjectTypesAsync_ShouldReturnPaged()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new SubjectTypeService(context);

        for (int i = 1; i <= 3; i++)
        {
            context.Set<SubjectType>().Add(new SubjectType { Code = $"S{i}", Name = $"Subject {i}", IsActive = true });
        }
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetSubjectTypesAsync(1, 2, null, "name", "asc");

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateSubjectTypeAsync_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new SubjectTypeService(context);

        var st = new SubjectType { Code = "OLD", Name = "Old Name", IsActive = true };
        context.Set<SubjectType>().Add(st);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        st.Name = "New Name";
        await service.UpdateSubjectTypeAsync(st);

        var updated = await service.GetSubjectTypeByIdAsync(st.Id);
        updated!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task DeleteSubjectTypeAsync_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new SubjectTypeService(context);

        var st = new SubjectType { Code = "DEL", Name = "To Delete", IsActive = true };
        context.Set<SubjectType>().Add(st);
        await context.SaveChangesAsync();

        await service.DeleteSubjectTypeAsync(st.Id);

        var exists = await service.SubjectTypeExistsAsync(st.Id);
        exists.Should().BeFalse();
    }
}
