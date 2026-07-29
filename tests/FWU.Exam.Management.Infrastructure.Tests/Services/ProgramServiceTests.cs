using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class ProgramServiceTests : TestBase
{
    private static IUserContext CreateSuperAdminContext()
    {
        var ctx = Substitute.For<IUserContext>();
        ctx.IsSuperAdmin.Returns(true);
        ctx.IsFacultyAdmin.Returns(false);
        ctx.IsCollegeAdmin.Returns(false);
        ctx.FacultyId.Returns((int?)null);
        return ctx;
    }

    [Fact]
    public async Task CreateProgram_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new ProgramService(context, userCtx);

        var program = new Program
        {
            ProgramCode = "BCA",
            ProgramName = "Bachelor in Computer Applications",
            ShortName = "BCA",
            Duration = 4,
            LevelId = levelId,
            IsActive = true
        };

        await service.CreateProgramAsync(program);

        var result = await service.GetProgramByIdAsync(program.Id);
        result.Should().NotBeNull();
        result!.ProgramName.Should().Be("Bachelor in Computer Applications");
    }

    [Fact]
    public async Task GetProgramsAsync_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new ProgramService(context, userCtx);

        context.Set<Program>().Add(new Program { ProgramCode = "BBA", ProgramName = "Bachelor of Business Administration", ShortName = "BBA", Duration = 4, LevelId = levelId, IsActive = true });
        context.Set<Program>().Add(new Program { ProgramCode = "BSc", ProgramName = "Bachelor of Science", ShortName = "BSc", Duration = 4, LevelId = levelId, IsActive = true });
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetProgramsAsync(1, 10, null, "programname", "asc");

        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateProgram_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var userCtx = CreateSuperAdminContext();

        var program = new Program { ProgramCode = "OLD", ProgramName = "Old Program", ShortName = "OLD", Duration = 3, LevelId = levelId, IsActive = true };
        context.Set<Program>().Add(program);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var service = new ProgramService(context, userCtx);

        program.ProgramName = "Updated Program";
        await service.UpdateProgramAsync(program);

        context.ChangeTracker.Clear();
        var result = await service.GetProgramByIdAsync(program.Id);
        result!.ProgramName.Should().Be("Updated Program");
    }

    [Fact]
    public async Task DeleteProgram_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var userCtx = CreateSuperAdminContext();

        var program = new Program { ProgramCode = "TMP", ProgramName = "Temporary", ShortName = "TMP", Duration = 1, LevelId = levelId, IsActive = true };
        context.Set<Program>().Add(program);
        await context.SaveChangesAsync();

        var service = new ProgramService(context, userCtx);
        await service.DeleteProgramAsync(program.Id);

        var exists = await service.ProgramExistsAsync(program.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetSelectListsAsync_ShouldReturnBoardsAndLevels()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var levelId = await SeedLevelAsync(context);
        var userCtx = CreateSuperAdminContext();

        var country = new Country { CountryName = "Nepal", IsActive = true };
        context.Set<Country>().Add(country);
        await context.SaveChangesAsync();
        context.Set<Board>().Add(new Board { BoardName = "Pokhara University", CountryId = country.Id, IsActive = true });
        await context.SaveChangesAsync();

        var service = new ProgramService(context, userCtx);

        var (boards, levels) = await service.GetSelectListsAsync();

        boards.Should().NotBeEmpty();
        levels.Should().NotBeEmpty();
    }
}
