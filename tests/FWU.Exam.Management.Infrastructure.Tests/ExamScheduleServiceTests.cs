using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Helpers;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class ExamScheduleServiceTests
{
    private static ExamScheduleService CreateService(TestDb db) =>
        new(db.Context, new TestUserContext());

    [Fact]
    public async Task DeleteExamScheduleAsync_DeletesSchedule_WhenNoRegistrationsOrResults()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), null));
        });

        var service = CreateService(db);
        await service.DeleteExamScheduleAsync(11);

        Assert.False(await db.Context.ExamSchedules.AnyAsync(e => e.Id == 11));
    }

    [Fact]
    public async Task DeleteExamScheduleAsync_DeletesRelatedChildRows_WithSchedule()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), null));
            ctx.ExamCenters.Add(new ExamCenter
            {
                Id = 1,
                TenantId = TestData.TenantId,
                ExamScheduleId = 11,
                Code = "C1",
                IsActive = true
            });
            ctx.ExamFees.Add(new ExamFee
            {
                Id = 1,
                TenantId = TestData.TenantId,
                ExamScheduleId = 11,
                Name = "Fee",
                Amount = 1000
            });
            ctx.ExamRollNumberSetup.Add(new ExamRollNumberSetup
            {
                Id = 1,
                TenantId = TestData.TenantId,
                ExamScheduleId = 11,
                FirstExamRollNumber = 1,
                MinimumRollNumberLength = 4,
                Round = 1,
                MinimumGap = 0,
                IsActive = true
            });
        });

        var service = CreateService(db);
        await service.DeleteExamScheduleAsync(11);

        Assert.False(await db.Context.ExamSchedules.AnyAsync(e => e.Id == 11));
        Assert.False(await db.Context.ExamCenters.AnyAsync(c => c.ExamScheduleId == 11));
        Assert.False(await db.Context.ExamFees.AnyAsync(f => f.ExamScheduleId == 11));
        Assert.False(await db.Context.ExamRollNumberSetup.AnyAsync(r => r.ExamScheduleId == 11));
    }

    [Fact]
    public async Task DeleteExamScheduleAsync_Throws_WhenRegistrationsExist()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, "s@t.com"));
            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), null));
            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 11));
            ctx.ExamRegistrations.Add(TestData.ExamRegistration(1, 11, 1));
        });

        var service = CreateService(db);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteExamScheduleAsync(11));
        Assert.Contains("registered", ex.Message);
        Assert.True(await db.Context.ExamSchedules.AnyAsync(e => e.Id == 11));
    }

    [Fact]
    public async Task DeleteExamScheduleAsync_Throws_WhenResultsExist()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, "s@t.com"));
            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), null));
            ctx.ApplicationVouchers.Add(TestData.Voucher(1, 1, 11));
        });

        var service = CreateService(db);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteExamScheduleAsync(11));
        Assert.Contains("result", ex.Message);
        Assert.True(await db.Context.ExamSchedules.AnyAsync(e => e.Id == 11));
    }

    [Fact]
    public async Task DeleteExamScheduleAsync_DoesNothing_WhenScheduleDoesNotExist()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx => TestData.SeedBase(ctx));

        var service = CreateService(db);
        await service.DeleteExamScheduleAsync(999);
    }

    [Fact]
    public async Task DeactivateExpiredSchedulesAsync_DeactivatesSchedule_EndedBeforeToday()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), null));
        });

        var service = CreateService(db);
        await service.DeactivateExpiredSchedulesAsync();

        Assert.False(await db.Context.ExamSchedules.AnyAsync(e => e.Id == 11 && e.IsActive));
    }

    [Fact]
    public async Task DeactivateExpiredSchedulesAsync_KeepsScheduleActive_WhenEndingToday()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.Today), null));
        });

        var service = CreateService(db);
        await service.DeactivateExpiredSchedulesAsync();

        Assert.True(await db.Context.ExamSchedules.AnyAsync(e => e.Id == 11 && e.IsActive));
    }

    [Fact]
    public async Task DeactivateExpiredSchedulesAsync_KeepsScheduleActive_WhenExtendedDateIsInFuture()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            var schedule = TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), null);
            schedule.ExtendedDate = DateTime.Today.AddDays(5);
            ctx.ExamSchedules.Add(schedule);
        });

        var service = CreateService(db);
        await service.DeactivateExpiredSchedulesAsync();

        Assert.True(await db.Context.ExamSchedules.AnyAsync(e => e.Id == 11 && e.IsActive));
    }

    [Fact]
    public async Task DeactivateExpiredSchedulesAsync_DeactivatesSchedule_WhenExtendedDateAlsoPassed()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            var schedule = TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.Today.AddDays(-10)), null);
            schedule.ExtendedDate = DateTime.Today.AddDays(-1);
            ctx.ExamSchedules.Add(schedule);
        });

        var service = CreateService(db);
        await service.DeactivateExpiredSchedulesAsync();

        Assert.False(await db.Context.ExamSchedules.AnyAsync(e => e.Id == 11 && e.IsActive));
    }

    [Fact]
    public async Task CreateExamScheduleAsync_DerivesBsDates_WhenAdProvided()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx => TestData.SeedBase(ctx));
        var service = CreateService(db);

        var schedule = TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.Today.AddDays(30)), null);
        schedule.StartDate = DateOnly.FromDateTime(new DateTime(2026, 9, 1));
        schedule.EndDate = DateOnly.FromDateTime(new DateTime(2026, 9, 15));
        schedule.StartDateBs = null;
        schedule.EndDateBs = null;

        await service.CreateExamScheduleAsync(schedule);

        var saved = await db.Context.ExamSchedules.AsNoTracking().FirstAsync(e => e.Id == 11);
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}$", saved.StartDateBs);
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}$", saved.EndDateBs);

        var startBs = saved.StartDateBs!.Split('-').Select(int.Parse).ToArray();
        var startAd = NepaliDateConverter.BsToAd(startBs[0], startBs[1], startBs[2]);
        Assert.Equal(schedule.StartDate.Value, DateOnly.FromDateTime(startAd!.Value));

        var endBs = saved.EndDateBs!.Split('-').Select(int.Parse).ToArray();
        var endAd = NepaliDateConverter.BsToAd(endBs[0], endBs[1], endBs[2]);
        Assert.Equal(schedule.EndDate.Value, DateOnly.FromDateTime(endAd!.Value));
    }
}
