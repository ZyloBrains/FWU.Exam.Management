using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class CollegeServiceTests : TestBase
{
    private static IUserContext CreateSuperAdminContext()
    {
        var u = Substitute.For<IUserContext>();
        u.IsSuperAdmin.Returns(true);
        u.IsFacultyAdmin.Returns(false);
        u.IsCollegeAdmin.Returns(false);
        return u;
    }

    private async Task<CollegeType> SeedCollegeTypeAsync(AppDbContext context)
    {
        var ct = new CollegeType { Code = "UT", Name = "University", IsActive = true };
        context.Set<CollegeType>().Add(ct);
        await context.SaveChangesAsync();
        return ct;
    }

    [Fact]
    public async Task GetAllColleges_ShouldReturnList()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var ct = await SeedCollegeTypeAsync(context);

        context.Set<College>().AddRange(
            new College { TenantId = TestTenantId, Code = "C001", Name = "College A", Email = "a@test.com", PrincipalName = "P1", PrincipalContactNumber = "123", IsActive = true, CollegeTypeId = ct.Id },
            new College { TenantId = TestTenantId, Code = "C002", Name = "College B", Email = "b@test.com", PrincipalName = "P2", PrincipalContactNumber = "456", IsActive = true, CollegeTypeId = ct.Id }
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new CollegeService(context, CreateSuperAdminContext());

        var (items, totalCount) = await service.GetCollegesAsync(1, 10, null, "name", "asc");

        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCollegeById_ShouldReturn_WhenExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var ct = await SeedCollegeTypeAsync(context);

        var college = new College { TenantId = TestTenantId, Code = "C001", Name = "College A", Email = "a@test.com", PrincipalName = "P1", PrincipalContactNumber = "123", IsActive = true, CollegeTypeId = ct.Id };
        context.Set<College>().Add(college);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new CollegeService(context, CreateSuperAdminContext());

        var result = await service.GetCollegeByIdAsync(college.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("College A");
    }

    [Fact]
    public async Task GetCollegeById_ShouldReturnNull_WhenNotExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var service = new CollegeService(context, CreateSuperAdminContext());

        var result = await service.GetCollegeByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateCollege_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var ct = await SeedCollegeTypeAsync(context);

        var college = new College { TenantId = TestTenantId, Code = "C001", Name = "New College", Email = "new@test.com", PrincipalName = "P1", PrincipalContactNumber = "123", IsActive = true, CollegeTypeId = ct.Id };
        var service = new CollegeService(context, CreateSuperAdminContext());

        var id = await service.CreateCollegeAsync(college, null, null, null, null);

        id.Should().BeGreaterThan(0);
        context.ChangeTracker.Clear();
        var saved = await service.GetCollegeByIdAsync(id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("New College");
    }

    [Fact]
    public async Task UpdateCollege_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var ct = await SeedCollegeTypeAsync(context);

        var college = new College { TenantId = TestTenantId, Code = "C001", Name = "Original", Email = "orig@test.com", PrincipalName = "P1", PrincipalContactNumber = "123", IsActive = true, CollegeTypeId = ct.Id };
        context.Set<College>().Add(college);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new CollegeService(context, CreateSuperAdminContext());

        var existing = await service.GetCollegeByIdAsync(college.Id);
        existing!.Name = "Updated";
        await service.UpdateCollegeAsync(existing, null, null, null, null);

        context.ChangeTracker.Clear();
        var updated = await service.GetCollegeByIdAsync(college.Id);
        updated!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteCollege_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var ct = await SeedCollegeTypeAsync(context);

        var college = new College { TenantId = TestTenantId, Code = "C001", Name = "To Delete", Email = "del@test.com", PrincipalName = "P1", PrincipalContactNumber = "123", IsActive = true, CollegeTypeId = ct.Id };
        context.Set<College>().Add(college);
        await context.SaveChangesAsync();

        var service = new CollegeService(context, CreateSuperAdminContext());

        await service.DeleteCollegeAsync(college.Id);

        var exists = await service.CollegeExistsAsync(college.Id);
        exists.Should().BeFalse();
    }
}
