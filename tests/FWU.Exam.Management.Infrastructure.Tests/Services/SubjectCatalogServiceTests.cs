using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class SubjectCatalogServiceTests : TestBase
{
    private IUserContext CreateSuperAdminContext()
    {
        var ctx = Substitute.For<IUserContext>();
        ctx.IsSuperAdmin.Returns(true);
        return ctx;
    }

    [Fact]
    public async Task CreateSubjectCatalogAsync_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SubjectCatalogService(context, userCtx);

        var subjectType = new SubjectType { Code = "TH", Name = "Theory", IsActive = true };
        context.Set<SubjectType>().Add(subjectType);
        await context.SaveChangesAsync();

        var catalog = new SubjectCatalog
        {
            SubjectCode = "MTH101",
            SubjectName = "Mathematics",
            ShortName = "Math",
            SubjectTypeId = subjectType.Id,
            CreditHours = 3,
            IsActive = true
        };

        await service.CreateSubjectCatalogAsync(catalog);

        var result = await service.GetSubjectCatalogByIdAsync(catalog.Id);
        result.Should().NotBeNull();
        result!.SubjectCode.Should().Be("MTH101");
    }

    [Fact]
    public async Task GetSubjectCatalogsAsync_ShouldReturnPaged()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SubjectCatalogService(context, userCtx);

        var subjectType = new SubjectType { Code = "TH", Name = "Theory", IsActive = true };
        context.Set<SubjectType>().Add(subjectType);
        await context.SaveChangesAsync();

        for (int i = 1; i <= 3; i++)
        {
            context.Set<SubjectCatalog>().Add(new SubjectCatalog
            {
                SubjectCode = $"SUB{i:D3}", SubjectName = $"Subject {i}", SubjectTypeId = subjectType.Id, IsActive = true
            });
        }
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetSubjectCatalogsAsync(1, 2, null, "subjectcode", "asc");

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateSubjectCatalogAsync_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SubjectCatalogService(context, userCtx);

        var subjectType = new SubjectType { Code = "TH", Name = "Theory", IsActive = true };
        context.Set<SubjectType>().Add(subjectType);
        await context.SaveChangesAsync();

        var catalog = new SubjectCatalog { SubjectCode = "OLD", SubjectName = "Old Name", SubjectTypeId = subjectType.Id, IsActive = true };
        context.Set<SubjectCatalog>().Add(catalog);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        catalog.SubjectName = "New Name";
        await service.UpdateSubjectCatalogAsync(catalog);

        var updated = await service.GetSubjectCatalogByIdAsync(catalog.Id);
        updated!.SubjectName.Should().Be("New Name");
    }

    [Fact]
    public async Task DeleteSubjectCatalogAsync_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SubjectCatalogService(context, userCtx);

        var subjectType = new SubjectType { Code = "TH", Name = "Theory", IsActive = true };
        context.Set<SubjectType>().Add(subjectType);
        await context.SaveChangesAsync();

        var catalog = new SubjectCatalog { SubjectCode = "DEL", SubjectName = "To Delete", SubjectTypeId = subjectType.Id, IsActive = true };
        context.Set<SubjectCatalog>().Add(catalog);
        await context.SaveChangesAsync();

        await service.DeleteSubjectCatalogAsync(catalog.Id);

        var exists = await service.SubjectCatalogExistsAsync(catalog.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task BulkCreateAsync_ShouldCreateMultiple()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SubjectCatalogService(context, userCtx);

        var subjectType = new SubjectType { Code = "TH", Name = "Theory", IsActive = true };
        context.Set<SubjectType>().Add(subjectType);
        await context.SaveChangesAsync();

        var items = new List<SubjectCatalog>
        {
            new() { SubjectCode = "MTH101", SubjectName = "Math", SubjectTypeId = subjectType.Id, IsActive = true },
            new() { SubjectCode = "PHY101", SubjectName = "Physics", SubjectTypeId = subjectType.Id, IsActive = true }
        };

        await service.BulkCreateAsync(items);

        var codes = await service.GetExistingSubjectCodesAsync();
        codes.Should().Contain(new[] { "MTH101", "PHY101" });
    }

    [Fact]
    public async Task GetExistingSubjectCodesAsync_ShouldReturnCodes()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SubjectCatalogService(context, userCtx);

        var subjectType = new SubjectType { Code = "TH", Name = "Theory", IsActive = true };
        context.Set<SubjectType>().Add(subjectType);
        await context.SaveChangesAsync();

        context.Set<SubjectCatalog>().Add(new SubjectCatalog { SubjectCode = "C001", SubjectName = "S1", SubjectTypeId = subjectType.Id, IsActive = true });
        context.Set<SubjectCatalog>().Add(new SubjectCatalog { SubjectCode = "C002", SubjectName = "S2", SubjectTypeId = subjectType.Id, IsActive = true });
        await context.SaveChangesAsync();

        var codes = await service.GetExistingSubjectCodesAsync();

        codes.Should().Contain(new[] { "C001", "C002" });
    }
}
