using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class ExamScheduleServiceTests
{
    private static ExamScheduleService CreateService(TestDb db) =>
        new(db.Context, new TestUserContext());

    [Fact]
    public async Task DeleteExamScheduleAsync_DeletesScheduleAndApprovalRows_WhenNoRegistrationsOrResults()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.ExamSchedules.Add(TestData.Schedule(11, 1, TestData.Regular, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), null));
            ctx.ExamScheduleCollegeApprovals.Add(new ExamScheduleCollegeApproval
            {
                Id = 1,
                TenantId = TestData.TenantId,
                ExamScheduleId = 11,
                CollegeId = TestData.CollegeId,
                Status = ExamScheduleApprovalStatus.Pending,
                IsActive = true
            });
        });

        var service = CreateService(db);
        await service.DeleteExamScheduleAsync(11);

        Assert.False(await db.Context.ExamSchedules.AnyAsync(e => e.Id == 11));
        Assert.False(await db.Context.ExamScheduleCollegeApprovals.AnyAsync(a => a.ExamScheduleId == 11));
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
}
