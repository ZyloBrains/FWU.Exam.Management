using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class CollegeProgramServiceTests : TestBase
{
    private static IUserContext CreateSuperAdminContext()
    {
        var u = Substitute.For<IUserContext>();
        u.IsSuperAdmin.Returns(true);
        u.IsFacultyAdmin.Returns(false);
        u.IsCollegeAdmin.Returns(false);
        return u;
    }

    private async Task<(College college, Program program)> SeedCollegeAndProgramAsync(AppDbContext context)
    {
        var college = new College
        {
            TenantId = TestTenantId,
            Code = "C001",
            Name = "Test College",
            Email = "college@test.com",
            PrincipalName = "Principal",
            PrincipalContactNumber = "123",
            IsActive = true
        };
        context.Set<College>().Add(college);

        var level = new Level { LevelName = "Bachelor", LevelCode = "BACH", IsActive = true };
        context.Set<Level>().Add(level);
        await context.SaveChangesAsync();

        var program = new Program
        {
            LevelId = level.Id,
            ProgramCode = "BCA",
            ProgramName = "Bachelor in Computer Application",
            ShortName = "BCA",
            Duration = 4,
            IsActive = true
        };
        context.Set<Program>().Add(program);
        await context.SaveChangesAsync();
        return (college, program);
    }

    [Fact]
    public async Task CreateCollegeProgram_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var (college, program) = await SeedCollegeAndProgramAsync(context);
        var service = new CollegeProgramService(context, CreateSuperAdminContext());

        var cp = new CollegeProgram
        {
            TenantId = TestTenantId,
            CollegeId = college.Id,
            ProgramId = program.Id,
            IsActive = true
        };
        await service.CreateCollegeProgramAsync(cp);

        var result = await service.GetCollegeProgramByIdAsync(cp.Id);
        result.Should().NotBeNull();
        result!.CollegeId.Should().Be(college.Id);
        result.ProgramId.Should().Be(program.Id);
    }

    [Fact]
    public async Task GetCollegePrograms_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var (college, program) = await SeedCollegeAndProgramAsync(context);

        context.Set<CollegeProgram>().AddRange(
            new CollegeProgram { TenantId = TestTenantId, CollegeId = college.Id, ProgramId = program.Id, IsActive = true },
            new CollegeProgram { TenantId = TestTenantId, CollegeId = college.Id, ProgramId = program.Id, IsActive = true }
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new CollegeProgramService(context, CreateSuperAdminContext());
        var (items, totalCount) = await service.GetCollegeProgramsAsync(1, 10, null, "id", "asc");

        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateCollegeProgram_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var (college, program) = await SeedCollegeAndProgramAsync(context);

        var cp = new CollegeProgram
        {
            TenantId = TestTenantId,
            CollegeId = college.Id,
            ProgramId = program.Id,
            Remarks = "Original",
            IsActive = true
        };
        context.Set<CollegeProgram>().Add(cp);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new CollegeProgramService(context, CreateSuperAdminContext());

        var existing = await service.GetCollegeProgramByIdAsync(cp.Id);
        existing!.Remarks = "Updated";
        await service.UpdateCollegeProgramAsync(existing);

        context.ChangeTracker.Clear();
        var updated = await service.GetCollegeProgramByIdAsync(cp.Id);
        updated!.Remarks.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteCollegeProgram_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var (college, program) = await SeedCollegeAndProgramAsync(context);

        var cp = new CollegeProgram
        {
            TenantId = TestTenantId,
            CollegeId = college.Id,
            ProgramId = program.Id,
            IsActive = true
        };
        context.Set<CollegeProgram>().Add(cp);
        await context.SaveChangesAsync();

        var service = new CollegeProgramService(context, CreateSuperAdminContext());
        await service.DeleteCollegeProgramAsync(cp.Id);

        var exists = await service.CollegeProgramExistsAsync(cp.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetExistingProgramIds_ShouldReturnProgramIds()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var (college, program) = await SeedCollegeAndProgramAsync(context);

        var level2 = new Level { LevelName = "Masters", LevelCode = "MAST", IsActive = true };
        context.Set<Level>().Add(level2);
        await context.SaveChangesAsync();

        var program2 = new Program
        {
            LevelId = level2.Id,
            ProgramCode = "BBA",
            ProgramName = "BBA",
            ShortName = "BBA",
            Duration = 4,
            IsActive = true
        };
        context.Set<Program>().Add(program2);
        await context.SaveChangesAsync();

        context.Set<CollegeProgram>().AddRange(
            new CollegeProgram { TenantId = TestTenantId, CollegeId = college.Id, ProgramId = program.Id, IsActive = true },
            new CollegeProgram { TenantId = TestTenantId, CollegeId = college.Id, ProgramId = program2.Id, IsActive = true }
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new CollegeProgramService(context, CreateSuperAdminContext());
        var ids = await service.GetExistingProgramIdsAsync(college.Id);

        ids.Should().HaveCount(2);
        ids.Should().Contain([program.Id, program2.Id]);
    }

    [Fact]
    public async Task GetSelectLists_ShouldReturnCollegesAndPrograms()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var (college, program) = await SeedCollegeAndProgramAsync(context);
        context.ChangeTracker.Clear();

        var service = new CollegeProgramService(context, CreateSuperAdminContext());
        var (colleges, programs) = await service.GetSelectListsAsync();

        colleges.Should().Contain(c => c.Id == college.Id);
        programs.Should().Contain(p => p.Id == program.Id);
    }
}
