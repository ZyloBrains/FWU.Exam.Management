using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class PaymentReconciliationServiceTests
{
    private static PaymentReconciliationService CreateService(TestDb db) =>
        new(db.Context,
            esewaService: null!,
            khaltiService: null!,
            dashboardService: null!,
            notificationService: null!,
            userManager: null!,
            auditLogWriter: null!,
            NullLogger<PaymentReconciliationService>.Instance);

    private static void SeedPaymentType(AppDbContext ctx) =>
        ctx.Set<PaymentType>().Add(new PaymentType { Id = 1, PaymentTypeName = "Online", IsActive = true });

    private static void SeedSchedule(AppDbContext ctx) =>
        ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)), DateTime.UtcNow.AddMonths(2)));

    private static void SeedBase(AppDbContext ctx)
    {
        TestData.SeedBase(ctx);
        SeedPaymentType(ctx);
        SeedSchedule(ctx);
        ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, "stu@test.com"));
    }

    private static PaymentRequestLog Log(int id, int? status, bool hasRegistration = true) =>
        new()
        {
            Id = id,
            TenantId = TestData.TenantId,
            PaymentRequestLogStatus = status,
            InvoiceNumber = $"INV-{id}",
            ForwardedTimestamp = DateTime.UtcNow.AddHours(-id),
            FullName = $"Student {id}",
            Amount = 1000,
            FullRequestContent = "{}",
            PaymentTypeId = 1,
            StudentRegistrationId = hasRegistration ? 1 : null,
            ExamScheduleId = 21
        };

    [Fact]
    public async Task GetPendingPaymentsAsync_ReturnsPendingFailedAndUnderVerification_ExcludesConfirmedAndTerminal()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPaymentType(ctx);
            SeedSchedule(ctx);
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, "stu@test.com"));

            var logs = new[]
            {
                Log(1, null), // Pending
                Log(2, 0),    // Failed
                Log(3, 1),    // Confirmed
                Log(4, 2),    // Terminal
                Log(5, 3)     // Under Verification
            };
            ctx.PaymentRequestLogs!.AddRange(logs);
        });

        var service = CreateService(db);

        var (items, totalCount) = await service.GetPendingPaymentsAsync(null, null, null, 1, 20);

        Assert.Equal(3, totalCount);
        Assert.Equal(new[] { 1, 2, 5 }, items.Select(i => i.Id).OrderBy(id => id).ToArray());
        Assert.DoesNotContain(items, i => i.Id == 3);
        Assert.DoesNotContain(items, i => i.Id == 4);
    }

    [Fact]
    public async Task GetPendingPaymentsAsync_ExcludesFailedLogWithoutStudentRegistration()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPaymentType(ctx);
            SeedSchedule(ctx);
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, "stu@test.com"));

            ctx.PaymentRequestLogs!.Add(Log(1, 0, hasRegistration: false));
            ctx.PaymentRequestLogs.Add(Log(2, 0));
        });

        var service = CreateService(db);

        var (items, totalCount) = await service.GetPendingPaymentsAsync(null, null, null, 1, 20);

        Assert.Equal(1, totalCount);
        var item = Assert.Single(items);
        Assert.Equal(2, item.Id);
    }

    [Fact]
    public async Task GetReconcileablePendingAsync_IncludesFailedLogs()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            SeedPaymentType(ctx);
            SeedSchedule(ctx);
            ctx.StudentRegistrations.Add(TestData.StudentRegistration(1, "stu@test.com"));

            ctx.PaymentRequestLogs!.Add(Log(1, 0));
            ctx.PaymentRequestLogs.Add(Log(2, null));
            ctx.PaymentRequestLogs.Add(Log(3, 1));
            ctx.PaymentRequestLogs.Add(Log(4, 2));
        });

        var service = CreateService(db);

        var items = await service.GetReconcileablePendingAsync();

        var ids = items.Select(i => i.Id).OrderBy(id => id).ToArray();
        Assert.Equal(new[] { 1, 2 }, ids);
    }

    [Fact]
    public async Task GetPendingPaymentsAsync_SearchMatchesTransactionUuidInsideFullRequestContent()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            SeedBase(ctx);

            var log = Log(1, null);
            log.FullRequestContent = "{\"method\":\"esewa\",\"amount\":1400.00,\"transaction_uuid\":\"20260826-760f8b2a\"}";
            ctx.PaymentRequestLogs!.Add(log);
            ctx.PaymentRequestLogs.Add(Log(2, null));
        });

        var service = CreateService(db);

        var (items, totalCount) = await service.GetPendingPaymentsAsync("20260826-760f8b2a", null, null, 1, 20);

        Assert.Equal(1, totalCount);
        var item = Assert.Single(items);
        Assert.Equal(1, item.Id);
    }

    [Fact]
    public async Task GetPendingPaymentsAsync_SearchMatchesTransactionIdColumn()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            SeedBase(ctx);

            var log = Log(1, null);
            log.TransactionId = "KHP-9AX7K2Q4T";
            ctx.PaymentRequestLogs!.Add(log);
            ctx.PaymentRequestLogs.Add(Log(2, null));
        });

        var service = CreateService(db);

        var (items, totalCount) = await service.GetPendingPaymentsAsync("KHP-9AX7", null, null, 1, 20);

        Assert.Equal(1, totalCount);
        var item = Assert.Single(items);
        Assert.Equal(1, item.Id);
    }

    [Fact]
    public async Task GetPendingPaymentsAsync_SearchMatchesGatewayCodeInsidePaymentResponseLog()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            SeedBase(ctx);

            ctx.PaymentRequestLogs!.Add(Log(1, null));
            ctx.PaymentRequestLogs.Add(Log(2, null));

            ctx.Set<PaymentResponseLog>().Add(new PaymentResponseLog
            {
                PaymentRequestLogId = 1,
                ResponseTimestamp = DateTime.UtcNow,
                IsSuccess = false,
                ResponseMessage = "lookup",
                FullResponse = "{\"status\":\"Initiated\",\"transaction_id\":\"eSewaTX-8812063\"}"
            });
        });

        var service = CreateService(db);

        var (items, totalCount) = await service.GetPendingPaymentsAsync("eSewaTX-8812063", null, null, 1, 20);

        Assert.Equal(1, totalCount);
        var item = Assert.Single(items);
        Assert.Equal(1, item.Id);
    }
}