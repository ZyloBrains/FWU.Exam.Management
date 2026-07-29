using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class CurriculumVersionServiceTests : TestBase
{
    private async Task SeedDataAsync(AppDbContext context)
    {
        context.Set<Level>().Add(new Level { LevelName = "Bachelor", IsActive = true });
        await context.SaveChangesAsync();

        context.Set<Program>().Add(new Program { LevelId = 1, ProgramCode = "BCA", ProgramName = "Bachelor of Computer Application", ShortName = "BCA", Duration = 4, IsActive = true });
        await context.SaveChangesAsync();

        context.Set<AcademicYear>().Add(new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateCurriculumVersion_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);
        var service = new CurriculumVersionService(context);

        var entity = new CurriculumVersion
        {
            Name = "2078 Curriculum",
            ProgramId = 1,
            EffectiveAcademicYearId = 1,
            IsActive = true
        };

        await service.CreateCurriculumVersionAsync(entity);

        var result = await service.GetCurriculumVersionByIdAsync(entity.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("2078 Curriculum");
    }

    [Fact]
    public async Task GetCurriculumVersions_ShouldReturnPaged()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<CurriculumVersion>().Add(new CurriculumVersion { TenantId = TestTenantId, Name = "CV1", ProgramId = 1, EffectiveAcademicYearId = 1, IsActive = true });
        context.Set<CurriculumVersion>().Add(new CurriculumVersion { TenantId = TestTenantId, Name = "CV2", ProgramId = 1, EffectiveAcademicYearId = 1, IsActive = true });
        context.Set<CurriculumVersion>().Add(new CurriculumVersion { TenantId = TestTenantId, Name = "CV3", ProgramId = 1, EffectiveAcademicYearId = 1, IsActive = true });
        await context.SaveChangesAsync();

        var service = new CurriculumVersionService(context);

        var (items, totalCount) = await service.GetCurriculumVersionsAsync(1, 2, null, "Id", "asc");

        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetCurriculumVersions_WithSearch_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<CurriculumVersion>().Add(new CurriculumVersion { TenantId = TestTenantId, Name = "Alpha", ProgramId = 1, EffectiveAcademicYearId = 1, IsActive = true });
        context.Set<CurriculumVersion>().Add(new CurriculumVersion { TenantId = TestTenantId, Name = "Beta", ProgramId = 1, EffectiveAcademicYearId = 1, IsActive = true });
        await context.SaveChangesAsync();

        var service = new CurriculumVersionService(context);

        var (items, totalCount) = await service.GetCurriculumVersionsAsync(1, 10, "Alpha", "Name", "asc");

        totalCount.Should().Be(1);
        items.Should().HaveCount(1);
        items[0].Name.Should().Be("Alpha");
    }

    [Fact]
    public async Task UpdateCurriculumVersion_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<CurriculumVersion>().Add(new CurriculumVersion { TenantId = TestTenantId, Name = "Original", ProgramId = 1, EffectiveAcademicYearId = 1, IsActive = true });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new CurriculumVersionService(context);

        var entity = await service.GetCurriculumVersionByIdAsync(1);
        entity.Should().NotBeNull();

        entity!.Name = "Updated";
        await service.UpdateCurriculumVersionAsync(entity);

        context.ChangeTracker.Clear();
        var updated = await service.GetCurriculumVersionByIdAsync(1);
        updated!.Name.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteCurriculumVersion_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<CurriculumVersion>().Add(new CurriculumVersion { TenantId = TestTenantId, Name = "ToDelete", ProgramId = 1, EffectiveAcademicYearId = 1, IsActive = true });
        await context.SaveChangesAsync();

        var service = new CurriculumVersionService(context);
        await service.DeleteCurriculumVersionAsync(1);

        var exists = await service.CurriculumVersionExistsAsync(1);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CurriculumVersionExists_ShouldReturnTrue_WhenExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<CurriculumVersion>().Add(new CurriculumVersion { TenantId = TestTenantId, Name = "Exists", ProgramId = 1, EffectiveAcademicYearId = 1, IsActive = true });
        await context.SaveChangesAsync();

        var service = new CurriculumVersionService(context);

        var exists = await service.CurriculumVersionExistsAsync(1);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetSelectLists_ShouldReturnActiveProgramsAndAcademicYears()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        await SeedDataAsync(context);

        context.Set<Program>().Add(new Program { LevelId = 1, ProgramCode = "BBA", ProgramName = "BBA", ShortName = "BBA", Duration = 4, IsActive = true });
        context.Set<Program>().Add(new Program { LevelId = 1, ProgramCode = "INACTIVE", ProgramName = "Inactive", ShortName = "INA", Duration = 4, IsActive = false });
        await context.SaveChangesAsync();

        var service = new CurriculumVersionService(context);

        var (programs, academicYears) = await service.GetSelectListsAsync();

        programs.Should().HaveCount(2);
        academicYears.Should().HaveCount(1);
    }
}
