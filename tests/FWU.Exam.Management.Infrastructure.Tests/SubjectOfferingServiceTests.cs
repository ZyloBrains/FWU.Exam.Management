using FWU.Exam.Management.Domain.Entities.CollegeAdmins;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Infrastructure.Services;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class SubjectOfferingServiceTests
{
    private const int OfferingId = 101;
    private const int ReferencedOfferingId = 201;

    private static SubjectOfferingService CreateService(TestDb db) => new(db.Context, new TestUserContext());

    [Fact]
    public async Task IsSubjectOfferingReferencedAsync_ReturnsFalse_WhenNoReferencesExist()
    {
        using var db = new TestDb(TestTenantContext.Standard(), TestData.SeedBase);

        var result = await CreateService(db).IsSubjectOfferingReferencedAsync(OfferingId);

        Assert.False(result);
    }

    [Fact]
    public async Task IsSubjectOfferingReferencedAsync_ReturnsTrue_WhenReferencedByCollegeAdminAssignment()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User("admin-user", "admin@test.com"));
            ctx.CollegeAdminSubjectAssignments.Add(new CollegeAdminSubjectAssignment
            {
                Id = 1,
                TenantId = TestData.TenantId,
                CollegeAdminUserId = "admin-user",
                SubjectOfferingId = ReferencedOfferingId,
                IsActive = true
            });
        });

        var result = await CreateService(db).IsSubjectOfferingReferencedAsync(ReferencedOfferingId);

        Assert.True(result);
    }

    [Fact]
    public async Task IsSubjectOfferingReferencedAsync_ReturnsTrue_WhenReferencedByExamSubjectResult()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, "reg@test.com"));
            ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow), null));
            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 21));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(1, 21, 1));
            ctx.ExamSubjectResults.Add(TestData.Result(1, 1, ReferencedOfferingId, TestData.Regular, "C"));
        });

        var result = await CreateService(db).IsSubjectOfferingReferencedAsync(ReferencedOfferingId);

        Assert.True(result);
    }

    [Fact]
    public async Task IsSubjectOfferingReferencedAsync_ReturnsFalse_WhenOnlyAnotherOfferingIsReferenced()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User("admin-user", "admin@test.com"));
            ctx.CollegeAdminSubjectAssignments.Add(new CollegeAdminSubjectAssignment
            {
                Id = 1,
                TenantId = TestData.TenantId,
                CollegeAdminUserId = "admin-user",
                SubjectOfferingId = ReferencedOfferingId,
                IsActive = true
            });
        });

        var result = await CreateService(db).IsSubjectOfferingReferencedAsync(OfferingId);

        Assert.False(result);
    }

    [Fact]
    public async Task ArchiveSubjectOfferingAsync_SetsIsActiveToFalse()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
        });

        var service = CreateService(db);
        await service.ArchiveSubjectOfferingAsync(OfferingId);

        var offering = await db.Context.SubjectOfferings.FindAsync(OfferingId);
        Assert.NotNull(offering);
        Assert.False(offering.IsActive);
    }

    [Fact]
    public async Task SaveSubjectOfferingChangesAsync_CreatesUpdatesAndArchives()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.SubjectCatalogs.Add(new SubjectCatalog
            {
                Id = 2,
                TenantId = TestData.TenantId,
                SubjectCode = "SUB2",
                SubjectName = "Subject 2",
                SubjectTypeId = 1,
                IsActive = true
            });
        });

        var existing = await db.Context.SubjectOfferings.FindAsync(OfferingId);
        Assert.NotNull(existing);

        var updated = new SubjectOffering
        {
            Id = existing.Id,
            TenantId = existing.TenantId,
            SubjectCatalogId = existing.SubjectCatalogId,
            ProgramId = existing.ProgramId,
            SemesterId = existing.SemesterId,
            CurriculumVersionId = existing.CurriculumVersionId,
            IsActive = true,
            IsCompulsory = false,
            DisplayOrder = 5,
            HasTheory = true,
            TheoryFullMarks = 90,
            TheoryPassMarks = 35
        };

        var created = new SubjectOffering
        {
            Id = 999,
            TenantId = TestData.TenantId,
            SubjectCatalogId = 2,
            ProgramId = TestData.ProgramId,
            SemesterId = 1,
            IsActive = true,
            IsCompulsory = true,
            DisplayOrder = 1,
            HasTheory = true,
            TheoryFullMarks = 75,
            TheoryPassMarks = 27
        };

        await CreateService(db).SaveSubjectOfferingChangesAsync(
            new[] { created },
            new[] { updated },
            new[] { 201 });

        db.Context.ChangeTracker.Clear();

        var updatedStored = await db.Context.SubjectOfferings.FindAsync(OfferingId);
        Assert.NotNull(updatedStored);
        Assert.Equal(5, updatedStored.DisplayOrder);
        Assert.False(updatedStored.IsCompulsory);

        Assert.NotNull(await db.Context.SubjectOfferings.FindAsync(999));

        var archived = await db.Context.SubjectOfferings.FindAsync(201);
        Assert.NotNull(archived);
        Assert.False(archived.IsActive);
    }

    [Fact]
    public async Task SaveSubjectOfferingChangesAsync_RestoresArchivedOffering()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx => TestData.SeedBase(ctx));
        var service = CreateService(db);

        await service.ArchiveSubjectOfferingAsync(201);
        db.Context.ChangeTracker.Clear();

        var archived = await db.Context.SubjectOfferings.FindAsync(201);
        Assert.NotNull(archived);
        Assert.False(archived.IsActive);

        var restored = new SubjectOffering
        {
            Id = archived.Id,
            TenantId = archived.TenantId,
            SubjectCatalogId = archived.SubjectCatalogId,
            ProgramId = archived.ProgramId,
            SemesterId = archived.SemesterId,
            CurriculumVersionId = archived.CurriculumVersionId,
            IsActive = true,
            IsCompulsory = true,
            DisplayOrder = archived.DisplayOrder,
            HasTheory = true,
            TheoryFullMarks = 75,
            TheoryPassMarks = 27
        };

        await service.SaveSubjectOfferingChangesAsync(
            Array.Empty<SubjectOffering>(),
            new[] { restored },
            Array.Empty<int>());

        db.Context.ChangeTracker.Clear();

        var result = await db.Context.SubjectOfferings.FindAsync(201);
        Assert.NotNull(result);
        Assert.True(result.IsActive);
    }
}