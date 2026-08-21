using FWU.Exam.Management.Domain.Entities.CollegeAdmins;
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
}