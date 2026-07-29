using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class PaymentTypeServiceTests : TestBase
{
    [Fact]
    public async Task CreatePaymentType_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new PaymentTypeService(context);

        var paymentType = new PaymentType
        {
            PaymentTypeName = "eSewa",
            LogoUrl = "https://example.com/esewa.png",
            IsActive = true
        };

        await service.CreatePaymentTypeAsync(paymentType);

        var result = await service.GetPaymentTypeByIdAsync(paymentType.Id);
        result.Should().NotBeNull();
        result!.PaymentTypeName.Should().Be("eSewa");
    }

    [Fact]
    public async Task GetPaymentTypesAsync_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new PaymentTypeService(context);

        context.Set<PaymentType>().Add(new PaymentType { PaymentTypeName = "Khalti", IsActive = true });
        context.Set<PaymentType>().Add(new PaymentType { PaymentTypeName = "eSewa", IsActive = true });
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetPaymentTypesAsync(1, 10, null, "paymenttypename", "asc");

        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdatePaymentType_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var pt = new PaymentType { PaymentTypeName = "Cash", IsActive = true };
        context.Set<PaymentType>().Add(pt);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var service = new PaymentTypeService(context);

        pt.PaymentTypeName = "Bank Transfer";
        await service.UpdatePaymentTypeAsync(pt);

        context.ChangeTracker.Clear();
        var result = await service.GetPaymentTypeByIdAsync(pt.Id);
        result!.PaymentTypeName.Should().Be("Bank Transfer");
    }

    [Fact]
    public async Task DeletePaymentType_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var pt = new PaymentType { PaymentTypeName = "Connect IPS", IsActive = true };
        context.Set<PaymentType>().Add(pt);
        await context.SaveChangesAsync();

        var service = new PaymentTypeService(context);
        await service.DeletePaymentTypeAsync(pt.Id);

        var exists = await service.PaymentTypeExistsAsync(pt.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task PaymentTypeExists_ShouldReturnFalse_WhenNotExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new PaymentTypeService(context);

        var exists = await service.PaymentTypeExistsAsync(999);

        exists.Should().BeFalse();
    }
}
