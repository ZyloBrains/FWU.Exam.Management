using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Infrastructure.Services;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class ExamScheduleServiceTests
{
    private static ExamScheduleService CreateService(TestDb db) =>
        new(db.Context, new TestUserContext());

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    private static ExamSchedule NewSchedule(DateOnly start, DateOnly end, int id = 0) => new()
    {
        Id = id,
        TenantId = TestData.TenantId,
        ExamScheduleName = "Test Schedule",
        AcademicYearId = TestData.AcademicYearId,
        ProgramId = TestData.ProgramId,
        SemesterId = 1,
        ExamTypeId = TestData.Regular,
        StartDate = start,
        EndDate = end,
        IsActive = true
    };

    [Fact]
    public async Task CreateExamScheduleAsync_Throws_WhenEndDateIsInPast()
    {
        using var db = new TestDb(TestTenantContext.Standard(), TestData.SeedBase);
        var service = CreateService(db);

        var schedule = NewSchedule(Today, Today.AddDays(-1));
        schedule.StartDate = null;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateExamScheduleAsync(schedule));

        Assert.Contains("End date cannot be in the past", ex.Message);
    }

    [Fact]
    public async Task CreateExamScheduleAsync_Throws_WhenStartDateIsInPast()
    {
        using var db = new TestDb(TestTenantContext.Standard(), TestData.SeedBase);
        var service = CreateService(db);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateExamScheduleAsync(NewSchedule(Today.AddDays(-1), Today.AddDays(5))));

        Assert.Contains("Start date cannot be in the past", ex.Message);
    }

    [Fact]
    public async Task CreateExamScheduleAsync_Throws_WhenStartDateAfterEndDate()
    {
        using var db = new TestDb(TestTenantContext.Standard(), TestData.SeedBase);
        var service = CreateService(db);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateExamScheduleAsync(NewSchedule(Today.AddDays(10), Today.AddDays(5))));

        Assert.Contains("Start date cannot be after end date", ex.Message);
    }

    [Fact]
    public async Task CreateExamScheduleAsync_Saves_WhenDatesAreTodayOrFuture()
    {
        using var db = new TestDb(TestTenantContext.Standard(), TestData.SeedBase);
        var service = CreateService(db);

        await service.CreateExamScheduleAsync(NewSchedule(Today, Today.AddDays(10)));

        Assert.Equal(1, db.Context.ExamSchedules.Count());
    }

    [Fact]
    public async Task UpdateExamScheduleAsync_AllowsUnchangedPastDates()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.ExamSchedules.Add(NewSchedule(Today.AddDays(-20), Today.AddDays(-10), id: 1));
        });

        var service = CreateService(db);
        var schedule = await service.GetExamScheduleByIdAsync(1);
        Assert.NotNull(schedule);

        schedule.ExamScheduleName = "Renamed";

        await service.UpdateExamScheduleAsync(schedule);

        Assert.Equal("Renamed", db.Context.ExamSchedules.First().ExamScheduleName);
    }

    [Fact]
    public async Task UpdateExamScheduleAsync_Throws_WhenEndDateChangedToPast()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.ExamSchedules.Add(NewSchedule(Today.AddDays(1), Today.AddDays(10), id: 1));
        });

        var service = CreateService(db);
        var schedule = await service.GetExamScheduleByIdAsync(1);
        Assert.NotNull(schedule);

        schedule.StartDate = null;
        schedule.EndDate = Today.AddDays(-1);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateExamScheduleAsync(schedule));

        Assert.Contains("End date cannot be in the past", ex.Message);
    }

    [Fact]
    public async Task UpdateExamScheduleAsync_Throws_WhenStartDateAfterEndDate()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.ExamSchedules.Add(NewSchedule(Today.AddDays(1), Today.AddDays(10), id: 1));
        });

        var service = CreateService(db);
        var schedule = await service.GetExamScheduleByIdAsync(1);
        Assert.NotNull(schedule);

        schedule.EndDate = Today.AddDays(0);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateExamScheduleAsync(schedule));

        Assert.Contains("Start date cannot be after end date", ex.Message);
    }
}
