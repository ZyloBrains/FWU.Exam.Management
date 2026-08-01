using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Services;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class SemesterEnrollmentServiceTests
{
    private const string UserId = "user-1";
    private const string Email = "stu@test.com";

    private static SemesterEnrollmentService CreateService(TestDb db) =>
        new(db.Context, new TestUserContext());

    private static void SeedPromotionBase(AppDbContext ctx)
    {
        ctx.Users.Add(TestData.User(UserId, Email));
    }

    private static DateOnly Past => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));
    private static DateOnly Future => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
    private static DateTime PastDateTime => DateTime.UtcNow.AddDays(-5);
    private static DateTime FutureDateTime => DateTime.UtcNow.AddDays(5);

    [Fact]
    public async Task PromoteCompletedSemestersAsync_CreatesNextSemesterEnrollment_WhenExamEndedAndAdmitCardReleased()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
            ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, Past, PastDateTime));
        });
        var service = CreateService(db);

        var created = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(1, created);
        var next = db.Context.SemesterEnrollments!.Single(se => se.SemesterId == 2);
        Assert.Equal(StudentEnrollmentStatus.Active, next.EnrollmentStatus);
        Assert.Equal(TestData.TenantId, next.TenantId);
    }

    [Fact]
    public async Task PromoteCompletedSemestersAsync_DoesNotPromote_WhenExamNotEndedYet()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
            ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, Future, PastDateTime));
        });
        var service = CreateService(db);

        var created = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(0, created);
    }

    [Fact]
    public async Task PromoteCompletedSemestersAsync_DoesNotPromote_WhenAdmitCardNotReleasedYet()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
            ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, Past, FutureDateTime));
        });
        var service = CreateService(db);

        var created = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(0, created);
    }

    [Fact]
    public async Task PromoteCompletedSemestersAsync_DoesNotPromote_WhenExtendedDateIsInFuture()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
            var schedule = TestData.Schedule(21, 1, TestData.Regular, Past, PastDateTime);
            schedule.ExtendedDate = FutureDateTime;
            ctx.ExamSchedules.Add(schedule);
        });
        var service = CreateService(db);

        var created = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(0, created);
    }

    [Fact]
    public async Task PromoteCompletedSemestersAsync_IsIdempotent_SecondRunCreatesNothing()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
            ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, Past, PastDateTime));
        });
        var service = CreateService(db);

        var first = await service.PromoteCompletedSemestersAsync();
        var second = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(1, first);
        Assert.Equal(0, second);
        Assert.Single(db.Context.SemesterEnrollments!.Where(se => se.SemesterId == 2));
    }

    [Fact]
    public async Task PromoteCompletedSemestersAsync_SkipsStudentsWithoutNextSemesterInProgram()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId, TestData.ProgramIdOther));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));
            ctx.ExamSchedules.Add(TestData.Schedule(21, 2, TestData.Regular, Past, PastDateTime, TestData.ProgramIdOther));
        });
        var service = CreateService(db);

        var created = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(0, created);
    }

    [Fact]
    public async Task PromoteCompletedSemestersAsync_DoesNotPromote_WhenAlreadyEnrolledInNextSemester()
    {
        using var db = new TestDb(TestTenantContext.Central(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPromotionBase(ctx);
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(2, 1, 2));
            ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular, Past, PastDateTime));
        });
        var service = CreateService(db);

        var created = await service.PromoteCompletedSemestersAsync();

        Assert.Equal(0, created);
    }
}
