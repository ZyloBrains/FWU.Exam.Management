using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.CollegeAdmins;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class CurriculumVersionServiceTests
{
    private const int SourceVersionId = 1;
    private const int TargetAcademicYearId = 2;

    private static CurriculumVersionService CreateService(TestDb db) => new(db.Context);

    private static void SeedCopyScenario(AppDbContext ctx)
    {
        TestData.SeedBase(ctx);

        ctx.AcademicYears.Add(new AcademicYear
        {
            Id = TargetAcademicYearId,
            AcademicYearCode = "2082",
            AcademicYearName = "2082",
            AcademicYearNameNepali = "2082",
            IsActive = true,
            IsRunning = false
        });

        ctx.Semesters.Add(new Semester
        {
            Id = 7,
            Year = 2,
            Number = 1,
            Name = "Semester 2.1",
            Code = "SEM21",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6),
            AcademicYearId = TargetAcademicYearId
        });
        ctx.Semesters.Add(new Semester
        {
            Id = 8,
            Year = 2,
            Number = 2,
            Name = "Semester 2.2",
            Code = "SEM22",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6),
            AcademicYearId = TargetAcademicYearId
        });

        ctx.CurriculumVersions.Add(new CurriculumVersion
        {
            Id = SourceVersionId,
            TenantId = TestData.TenantId,
            Name = "Default - BCA (2081)",
            ProgramId = TestData.ProgramId,
            EffectiveAcademicYearId = TestData.AcademicYearId,
            IsActive = true
        });

        ctx.SubjectOfferings.Local.First(o => o.Id == 101).CurriculumVersionId = SourceVersionId;
        ctx.SubjectOfferings.Local.First(o => o.Id == 102).CurriculumVersionId = SourceVersionId;
        ctx.SubjectOfferings.Local.First(o => o.Id == 103).CurriculumVersionId = SourceVersionId;
    }

    [Fact]
    public async Task CopyCurriculumVersionAsync_CopiesOfferingsIntoMatchingSemesterNumbers_AndActivatesCopy()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedCopyScenario);
        var service = CreateService(db);

        var copied = await service.CopyCurriculumVersionAsync(SourceVersionId, TargetAcademicYearId, "Default - BCA (2082)");

        Assert.NotNull(copied);
        Assert.Equal(TargetAcademicYearId, copied.EffectiveAcademicYearId);
        Assert.Equal(TestData.ProgramId, copied.ProgramId);
        Assert.True(copied.IsActive);

        var source = await db.Context.CurriculumVersions.AsNoTracking().SingleAsync(c => c.Id == SourceVersionId);
        Assert.False(source.IsActive);

        var copiedOfferings = await db.Context.SubjectOfferings
            .Where(o => o.CurriculumVersionId == copied.Id)
            .ToListAsync();

        Assert.Equal(2, copiedOfferings.Count);

        var semesterNumbers = copiedOfferings
            .Select(o => db.Context.Semesters.AsNoTracking().Single(s => s.Id == o.SemesterId).Number)
            .OrderBy(n => n)
            .ToList();
        Assert.Equal(new[] { 1, 2 }, semesterNumbers);
        Assert.All(copiedOfferings, o => Assert.Equal(TestData.ProgramId, o.ProgramId));
    }

    [Fact]
    public async Task CopyCurriculumVersionAsync_SkipsOfferings_WhenTargetYearHasNoMatchingSemesterNumber()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            SeedCopyScenario(ctx);
            ctx.SubjectOfferings.Local.First(o => o.Id == 104).CurriculumVersionId = SourceVersionId;
        });
        var service = CreateService(db);

        var copied = await service.CopyCurriculumVersionAsync(SourceVersionId, TargetAcademicYearId, "Copy");

        Assert.NotNull(copied);
        var copiedOfferings = await db.Context.SubjectOfferings
            .Where(o => o.CurriculumVersionId == copied.Id)
            .ToListAsync();
        Assert.Equal(2, copiedOfferings.Count);
    }

    [Fact]
    public async Task CopyCurriculumVersionAsync_ReturnsNull_WhenSourceVersionNotFound()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedCopyScenario);
        var service = CreateService(db);

        var result = await service.CopyCurriculumVersionAsync(9999, TargetAcademicYearId, "Copy");

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateCurriculumVersionAsync_DeactivatesOtherActiveVersionsOfSameProgram()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedCopyScenario);
        var service = CreateService(db);

        await service.CreateCurriculumVersionAsync(new CurriculumVersion
        {
            TenantId = TestData.TenantId,
            Name = "New Active",
            ProgramId = TestData.ProgramId,
            EffectiveAcademicYearId = TargetAcademicYearId,
            IsActive = true
        });

        var all = await db.Context.CurriculumVersions.AsNoTracking().ToListAsync();
        Assert.Single(all, v => v.IsActive);
    }

    [Fact]
    public async Task DeleteCurriculumVersionAsync_DeletesVersionAndItsOfferings_WhenNoneAreReferenced()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedCopyScenario);
        var service = CreateService(db);

        var (deleted, skipped) = await service.DeleteCurriculumVersionAsync(SourceVersionId);

        Assert.True(deleted);
        Assert.Equal(0, skipped);
        Assert.False(await db.Context.CurriculumVersions.AnyAsync(v => v.Id == SourceVersionId));
        Assert.False(await db.Context.SubjectOfferings.AnyAsync(o => o.CurriculumVersionId == SourceVersionId));
        Assert.True(await db.Context.SubjectOfferings.AnyAsync(o => o.Id == 104));
        Assert.True(await db.Context.SubjectOfferings.AnyAsync(o => o.Id == 201));
    }

    [Fact]
    public async Task DeleteCurriculumVersionAsync_KeepsVersion_WhenAnOfferingIsReferenced()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            SeedCopyScenario(ctx);
            ctx.Users.Add(TestData.User("admin-1", "admin@test.com"));
            ctx.CollegeAdminSubjectAssignments.Add(new CollegeAdminSubjectAssignment
            {
                Id = 1,
                TenantId = TestData.TenantId,
                CollegeAdminUserId = "admin-1",
                SubjectOfferingId = 101,
                IsActive = true
            });
        });
        var service = CreateService(db);

        var (deleted, skipped) = await service.DeleteCurriculumVersionAsync(SourceVersionId);

        Assert.False(deleted);
        Assert.Equal(1, skipped);
        Assert.True(await db.Context.CurriculumVersions.AnyAsync(v => v.Id == SourceVersionId));
        Assert.True(await db.Context.SubjectOfferings.AnyAsync(o => o.Id == 101));
        Assert.False(await db.Context.SubjectOfferings.AnyAsync(o => o.Id == 102));
        Assert.False(await db.Context.SubjectOfferings.AnyAsync(o => o.Id == 103));
    }

    [Fact]
    public async Task UpdateCurriculumVersionAsync_PreservesTenantId()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedCopyScenario);
        var service = CreateService(db);

        await service.UpdateCurriculumVersionAsync(new CurriculumVersion
        {
            Id = SourceVersionId,
            TenantId = 0,
            Name = "Updated Name",
            ProgramId = TestData.ProgramId,
            EffectiveAcademicYearId = TestData.AcademicYearId,
            IsActive = true
        });

        var version = await db.Context.CurriculumVersions.AsNoTracking().SingleAsync(v => v.Id == SourceVersionId);
        Assert.Equal(TestData.TenantId, version.TenantId);
        Assert.Equal("Updated Name", version.Name);
    }

    [Fact]
    public async Task UniqueIndex_PreventsDuplicateOfferingWithinSameVersion()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedCopyScenario);

        var duplicate = TestData.Offering(900, 1, TestData.ProgramId);
        duplicate.CurriculumVersionId = SourceVersionId;
        db.Context.SubjectOfferings.Add(duplicate);

        await Assert.ThrowsAsync<DbUpdateException>(() => db.Context.SaveChangesAsync());
    }
}
