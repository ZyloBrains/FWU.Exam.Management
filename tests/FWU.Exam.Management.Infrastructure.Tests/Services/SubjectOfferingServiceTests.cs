using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class SubjectOfferingServiceTests : TestBase
{
    private IUserContext CreateSuperAdminContext()
    {
        var ctx = Substitute.For<IUserContext>();
        ctx.IsSuperAdmin.Returns(true);
        return ctx;
    }

    private async Task<(
        SubjectCatalog SubjectCatalog,
        Domain.Entities.Program Program,
        Semester Semester)> SeedPrerequisitesAsync(AppDbContext context)
    {
        var subjectType = new SubjectType { Code = "TH", Name = "Theory", IsActive = true };
        context.SubjectTypes!.Add(subjectType);

        var level = new Level { LevelName = "Bachelor", LevelCode = "BACH", IsActive = true };
        context.Set<Level>().Add(level);

        var academicYear = new AcademicYear { AcademicYearCode = "2081/082", AcademicYearName = "2081/082", AcademicYearCodeNepali = "२०८१/०८२", AcademicYearNameNepali = "२०८१/०८२", IsRunning = true, IsActive = true };
        context.Set<AcademicYear>().Add(academicYear);

        await context.SaveChangesAsync();

        var cat1 = new SubjectCatalog { SubjectCode = "MTH101", SubjectName = "Mathematics", SubjectTypeId = subjectType.Id, IsActive = true };
        context.SubjectCatalogs!.Add(cat1);

        var program = new Domain.Entities.Program { ProgramCode = "BSC", ProgramName = "B.Sc.", ShortName = "BSc", LevelId = level.Id, Duration = 4, IsActive = true };
        context.Programs!.Add(program);

        var semester = new Semester { Name = "First Semester", Code = "SEM1", Number = 1, Year = 1, AcademicYearId = academicYear.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(6) };
        context.Semesters!.Add(semester);

        await context.SaveChangesAsync();

        return (cat1, program, semester);
    }

    [Fact]
    public async Task CreateSubjectOfferingAsync_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SubjectOfferingService(context, userCtx);

        var (subjectCatalog, program, semester) = await SeedPrerequisitesAsync(context);

        var offering = new SubjectOffering
        {
            TenantId = TestTenantId,
            SubjectCatalogId = subjectCatalog.Id,
            ProgramId = program.Id,
            SemesterId = semester.Id,
            IsCompulsory = true,
            DisplayOrder = 1,
            HasTheory = true,
            TheoryFullMarks = 100,
            TheoryPassMarks = 40
        };

        await service.CreateSubjectOfferingAsync(offering);

        var result = await service.GetSubjectOfferingByIdAsync(offering.Id);
        result.Should().NotBeNull();
        result!.SubjectCatalogId.Should().Be(subjectCatalog.Id);
    }

    [Fact]
    public async Task GetSubjectOfferingsAsync_ShouldReturnGroupedByProgram()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SubjectOfferingService(context, userCtx);

        var (subjectCatalog, program, semester) = await SeedPrerequisitesAsync(context);

        context.SubjectOfferings!.Add(new SubjectOffering
        {
            TenantId = TestTenantId, SubjectCatalogId = subjectCatalog.Id, ProgramId = program.Id,
            SemesterId = semester.Id, IsCompulsory = true, DisplayOrder = 1,
            HasTheory = true, TheoryFullMarks = 100, TheoryPassMarks = 40
        });
        await context.SaveChangesAsync();

        var (items, totalProgramCount) = await service.GetSubjectOfferingsAsync(1, 10, null, "program", "asc");

        totalProgramCount.Should().Be(1);
        items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateSubjectOfferingAsync_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SubjectOfferingService(context, userCtx);

        var (subjectCatalog, program, semester) = await SeedPrerequisitesAsync(context);

        var offering = new SubjectOffering
        {
            TenantId = TestTenantId, SubjectCatalogId = subjectCatalog.Id, ProgramId = program.Id,
            SemesterId = semester.Id, IsCompulsory = true, DisplayOrder = 1,
            HasTheory = true, TheoryFullMarks = 100, TheoryPassMarks = 40
        };
        context.SubjectOfferings!.Add(offering);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        offering.IsCompulsory = false;
        await service.UpdateSubjectOfferingAsync(offering);

        var updated = await service.GetSubjectOfferingByIdAsync(offering.Id);
        updated!.IsCompulsory.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteSubjectOfferingAsync_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SubjectOfferingService(context, userCtx);

        var (subjectCatalog, program, semester) = await SeedPrerequisitesAsync(context);

        var offering = new SubjectOffering
        {
            TenantId = TestTenantId, SubjectCatalogId = subjectCatalog.Id, ProgramId = program.Id,
            SemesterId = semester.Id, IsCompulsory = true, DisplayOrder = 1,
            HasTheory = true, TheoryFullMarks = 100, TheoryPassMarks = 40
        };
        context.SubjectOfferings!.Add(offering);
        await context.SaveChangesAsync();

        await service.DeleteSubjectOfferingAsync(offering.Id);

        var exists = await service.SubjectOfferingExistsAsync(offering.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CreateSubjectOfferingsAsync_ShouldCreateMultiple()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var userCtx = CreateSuperAdminContext();
        var service = new SubjectOfferingService(context, userCtx);

        var (cat1, program, semester) = await SeedPrerequisitesAsync(context);

        var cat2 = new SubjectCatalog { SubjectCode = "PHY101", SubjectName = "Physics", SubjectTypeId = 1, IsActive = true };
        context.SubjectCatalogs!.Add(cat2);
        await context.SaveChangesAsync();

        var offerings = new[]
        {
            new SubjectOffering { TenantId = TestTenantId, SubjectCatalogId = cat1.Id, ProgramId = program.Id, SemesterId = semester.Id, IsCompulsory = true, DisplayOrder = 1, HasTheory = true, TheoryFullMarks = 100, TheoryPassMarks = 40 },
            new SubjectOffering { TenantId = TestTenantId, SubjectCatalogId = cat2.Id, ProgramId = program.Id, SemesterId = semester.Id, IsCompulsory = true, DisplayOrder = 2, HasTheory = true, TheoryFullMarks = 100, TheoryPassMarks = 40 }
        }.ToList();

        await service.CreateSubjectOfferingsAsync(offerings);

        var existingIds = await service.GetExistingSubjectCatalogIdsAsync(program.Id);
        existingIds.Should().HaveCount(2);
    }
}
