using System.Data.Common;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class StudentDashboardPerformanceTests
{
    private const string UserId = "user-1";
    private const string UserIdOther = "user-2";
    private const string Email = "stu@test.com";
    private const string EmailOther = "other@test.com";

    private static StudentDashboardService CreateService(TestDb db) =>
        new(db.Context, new TestUserContext(), NullLogger<StudentDashboardService>.Instance, new NullKhaltiService());

    [Fact]
    public void Model_HasPerformanceIndexes_UsedByStudentExamFormQueries()
    {
        using var db = new TestDb(TestTenantContext.Standard());

        var registration = db.Context.Model.FindEntityType(typeof(ExamRegistration))!;
        var voucherIndex = registration.GetIndexes().SingleOrDefault(i =>
            i.Properties.Select(p => p.Name).SequenceEqual(["ApplicationVoucherId", "IsActive"]));
        Assert.NotNull(voucherIndex);
        Assert.Equal("[ApplicationVoucherId] IS NOT NULL", voucherIndex.GetFilter());

        var schedule = db.Context.Model.FindEntityType(typeof(ExamSchedule))!;
        Assert.Contains(schedule.GetIndexes(), i =>
            i.Properties.Select(p => p.Name)
                .SequenceEqual(["ProgramId", "IsActive", "SemesterInstanceId"]));
    }

    [Fact]
    public async Task HasAnyExamResultsInSemesterAsync_DoesNotTreatNullScheduleIdAsMatch()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.Users.Add(TestData.User(UserId, Email));
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 11));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(1, 11, 1));
            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), null));

            // Only a legacy row with no schedule is graded. It must not make the
            // student eligible for a result in semester 1.
            ctx.ExamSubjectResults.Add(TestData.Result(1, 1, 101, TestData.Regular, "F", examScheduleId: null));
        });

        var service = CreateService(db);

        Assert.False(await service.HasAnyExamResultsInSemesterAsync(UserId, 1, TestData.ProgramId));
    }

    [Fact]
    public async Task GetFailedSubjectOfferingIdsForSemesterAsync_ReturnsOnlyTheStudentsOwnResults()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);

            ctx.Users.Add(TestData.User(UserId, Email));
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 11));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(1, 11, 1));
            ctx.ExamSubjectResults.Add(TestData.Result(1, 1, 101, TestData.Regular, "F", examScheduleId: 11));

            ctx.Users.Add(TestData.User(UserIdOther, EmailOther));
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(2, EmailOther));
            ctx.ApplicationVouchers.Add(TestData.Voucher(2, 2, 11));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(2, 11, 2));
            ctx.ExamSubjectResults.Add(TestData.Result(2, 2, 102, TestData.Regular, "F", examScheduleId: 11));

            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), null));
        });

        var service = CreateService(db);

        var result = await service.GetFailedSubjectOfferingIdsForSemesterAsync(UserId, 1, TestData.ProgramId);
        Assert.Equal([101], result);
    }

    [Fact]
    public async Task RepeatedRegistrationIdLookups_WithinOneServiceInstance_AreMemoized()
    {
        var tenant = TestTenantContext.Standard();
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var counter = new SelectCountingInterceptor();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(counter)
            .Options;
        using var context = new AppDbContext(options, NullLogger<AppDbContext>.Instance, tenant);
        context.Database.EnsureCreated();
        TestData.SeedBase(context);
        context.Users.Add(TestData.User(UserId, Email));
        context.StudentRegistrations.Add(TestData.StudentRegistration(1, Email));
        context.ApplicationVouchers.Add(TestData.Voucher(1, 1, 11));
        context.ExamRegistrations.Add(TestData.ExamRegistration(1, 11, 1));
        context.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), null));
        context.SaveChanges();

        var service = new StudentDashboardService(context, new TestUserContext(),
            NullLogger<StudentDashboardService>.Instance, new NullKhaltiService());

        counter.Reset();
        await service.HasExistingExamRegistrationAsync(11, UserId);
        var firstCallQueries = counter.SelectCount;

        counter.Reset();
        await service.HasExistingExamRegistrationAsync(11, UserId);
        var secondCallQueries = counter.SelectCount;

        Assert.True(firstCallQueries > secondCallQueries,
            $"First call issued {firstCallQueries} selects, second {secondCallQueries}; the 4-query registration-id chain should be cached.");
        Assert.Equal(1, secondCallQueries);
    }

    private sealed class SelectCountingInterceptor : DbCommandInterceptor
    {
        public int SelectCount { get; private set; }

        public void Reset() => SelectCount = 0;

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Count(command);
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Count(command);
            return ValueTask.FromResult(result);
        }

        private void Count(DbCommand command)
        {
            if (command.CommandText.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                SelectCount++;
        }
    }
}
