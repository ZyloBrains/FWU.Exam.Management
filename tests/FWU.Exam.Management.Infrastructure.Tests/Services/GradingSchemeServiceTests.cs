using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class GradingSchemeServiceTests : TestBase
{
    private static IUserContext CreateSuperAdminContext()
    {
        var ctx = Substitute.For<IUserContext>();
        ctx.IsSuperAdmin.Returns(true);
        ctx.IsFacultyAdmin.Returns(false);
        ctx.FacultyId.Returns((int?)null);
        return ctx;
    }

    [Fact]
    public async Task CreateGradingScheme_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var program = new Program { ProgramCode = "BCA", ProgramName = "Bachelor in Computer Applications", ShortName = "BCA", Duration = 4, LevelId = levelId, IsActive = true };
        context.Set<Program>().Add(program);
        await context.SaveChangesAsync();
        var userCtx = CreateSuperAdminContext();
        var service = new GradingSchemeService(context, userCtx);

        var scheme = new GradingScheme
        {
            Name = "Test Scheme",
            ProgramId = program.Id,
            AcademicYearId = null,
            Description = "A test grading scheme",
            IsActive = true
        };

        await service.CreateGradingSchemeAsync(scheme);

        var result = await service.GetGradingSchemeByIdAsync(scheme.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Scheme");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetGradingSchemesAsync_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var program = new Program { ProgramCode = "BCA", ProgramName = "Bachelor in Computer Applications", ShortName = "BCA", Duration = 4, LevelId = levelId, IsActive = true };
        context.Set<Program>().Add(program);
        await context.SaveChangesAsync();
        var userCtx = CreateSuperAdminContext();
        var service = new GradingSchemeService(context, userCtx);

        context.Set<GradingScheme>().Add(new GradingScheme
        {
            Name = "Scheme A", ProgramId = program.Id, IsActive = true
        });
        context.Set<GradingScheme>().Add(new GradingScheme
        {
            Name = "Scheme B", ProgramId = program.Id, IsActive = true
        });
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetGradingSchemesAsync(1, 10, null, "name", "asc");

        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSelectListDataAsync_ShouldReturnProgramsAndAcademicYears()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedAcademicYearAsync(context);
        var levelId = await SeedLevelAsync(context);

        context.Set<Program>().Add(new Program
        {
            ProgramCode = "BCA", ProgramName = "Bachelor in Computer Applications",
            ShortName = "BCA", Duration = 4, LevelId = levelId, IsActive = true
        });
        await context.SaveChangesAsync();

        var userCtx = CreateSuperAdminContext();
        var service = new GradingSchemeService(context, userCtx);

        var result = await service.GetSelectListDataAsync();

        result.Programs.Should().NotBeEmpty();
        result.AcademicYears.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DeleteGradingScheme_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var program = new Program { ProgramCode = "BCA", ProgramName = "Bachelor in Computer Applications", ShortName = "BCA", Duration = 4, LevelId = levelId, IsActive = true };
        context.Set<Program>().Add(program);
        await context.SaveChangesAsync();
        var userCtx = CreateSuperAdminContext();
        var service = new GradingSchemeService(context, userCtx);

        var scheme = new GradingScheme
        {
            Name = "To Delete", ProgramId = program.Id, IsActive = true
        };
        context.Set<GradingScheme>().Add(scheme);
        await context.SaveChangesAsync();

        await service.DeleteGradingSchemeAsync(scheme.Id);

        var exists = await service.GradingSchemeExistsAsync(scheme.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateGradingScheme_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var program = new Program { ProgramCode = "BCA", ProgramName = "Bachelor in Computer Applications", ShortName = "BCA", Duration = 4, LevelId = levelId, IsActive = true };
        context.Set<Program>().Add(program);
        await context.SaveChangesAsync();
        var userCtx = CreateSuperAdminContext();

        var scheme = new GradingScheme
        {
            Name = "Original", ProgramId = program.Id, IsActive = true
        };
        context.Set<GradingScheme>().Add(scheme);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var service = new GradingSchemeService(context, userCtx);

        scheme.Name = "Modified";
        await service.UpdateGradingSchemeAsync(scheme);

        context.ChangeTracker.Clear();
        var result = await service.GetGradingSchemeByIdAsync(scheme.Id);
        result!.Name.Should().Be("Modified");
    }
}
