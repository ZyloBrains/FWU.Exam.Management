using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class StudentDashboardServiceTests
{
    private const string UserId = "user-1";
    private const string Email = "stu@test.com";

    private static StudentDashboardService CreateService(TestDb db) =>
        new(db.Context, new TestUserContext(), NullLogger<StudentDashboardService>.Instance);

    private static DateOnly Past => DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_ReturnsEmpty_WhenStudentHasNoAdmission()
    {
        using var db = new TestDb(TestTenantContext.Standard(), TestData.SeedBase);
        var service = CreateService(db);
        var student = TestData.StudentRegistration(1, Email);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_ReturnsEmpty_WhenStudentHasNoActiveEnrollment()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2, StudentEnrollmentStatus.Dropped));
            ctx.ExamSchedules.Add(TestData.Schedule(11, 2, TestData.Regular, Past, null));
        });

        var student = db.Context.StudentRegistrations!.FirstOrDefault() ?? TestData.StudentRegistration(1, Email);
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_ReturnsOnlyCurrentSemesterRegularSchedule()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));

            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, Past, null));       // regular sem1
            ctx.ExamSchedules.Add(TestData.Schedule(12, 2, TestData.Regular, Past, null));       // regular sem2 (current)
            ctx.ExamSchedules.Add(TestData.Schedule(13, 3, TestData.Regular, Past, null));       // regular sem3
            ctx.ExamSchedules.Add(TestData.Schedule(14, 2, TestData.Supplementary, Past, null)); // supplementary sem2 (no failures)
            ctx.ExamSchedules.Add(TestData.Schedule(15, 2, TestData.Entrance, Past, null));      // entrance
            ctx.ExamSchedules.Add(TestData.Schedule(16, 2, TestData.Regular, Past, null, TestData.ProgramIdOther)); // other program
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        var schedule = Assert.Single(result);
        Assert.Equal(12, schedule.Id);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_ShowsSupplementary_WhenStudentFailedSubjectsInThatSemester()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));

            var regularSem2 = TestData.Schedule(11, 2, TestData.Regular, Past, null);
            var supplementarySem2 = TestData.Schedule(12, 2, TestData.Supplementary, Past, null);
            ctx.ExamSchedules.Add(regularSem2);
            ctx.ExamSchedules.Add(supplementarySem2);

            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 11));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(1, 11, 1));
            ctx.ExamSubjectResults.Add(TestData.Result(1, 1, 102, TestData.Regular, "F", 11));
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, s => s.Id == 11);
        Assert.Contains(result, s => s.Id == 12);
    }

    [Fact]
    public async Task GetExamSchedulesForStudentAsync_HidesSupplementary_WhenNoFailures()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2));

            ctx.ExamSchedules.Add(TestData.Schedule(11, 2, TestData.Regular, Past, null));
            ctx.ExamSchedules.Add(TestData.Schedule(12, 2, TestData.Supplementary, Past, null));

            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 11));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(1, 11, 1));
            ctx.ExamSubjectResults.Add(TestData.Result(1, 1, 102, TestData.Regular, "B", 11));
        });

        var student = db.Context.StudentRegistrations!.Single();
        var service = CreateService(db);

        var result = await service.GetExamSchedulesForStudentAsync(student, UserId);

        var schedule = Assert.Single(result);
        Assert.Equal(11, schedule.Id);
    }

    [Fact]
    public async Task GetCurrentSemesterId_ReturnsLatestActiveEnrollmentSemester()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 1));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(2, 1, 3));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(3, 1, 4, StudentEnrollmentStatus.Dropped));
        });

        var service = CreateService(db);

        var semesterId = await service.GetCurrentSemesterIdForStudentAsync(UserId);

        Assert.Equal(3, semesterId);
    }

    [Fact]
    public async Task GetCurrentSemesterId_ReturnsNull_WhenStudentHasNoAdmission()
    {
        using var db = new TestDb(TestTenantContext.Standard(), TestData.SeedBase);
        var service = CreateService(db);

        var semesterId = await service.GetCurrentSemesterIdForStudentAsync(UserId);

        Assert.Null(semesterId);
    }

    [Fact]
    public async Task GetCurrentSemesterId_ReturnsNull_WhenNoActiveEnrollment()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            ctx.StudentAdmissions.Add(TestData.Admission(1, UserId));
            ctx.SemesterEnrollments.Add(TestData.Enrollment(1, 1, 2, StudentEnrollmentStatus.Inactive));
        });

        var service = CreateService(db);

        var semesterId = await service.GetCurrentSemesterIdForStudentAsync(UserId);

        Assert.Null(semesterId);
    }
}
