using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class BankServiceTests : TestBase
{
    [Fact]
    public async Task CreateBank_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new BankService(context);

        var bank = new Bank { BankName = "Nepal Bank", BankCode = "NB001", Remarks = "Test", IsActive = true };
        await service.CreateBankAsync(bank);

        var result = await service.GetBankByIdAsync(bank.Id);
        result.Should().NotBeNull();
        result!.BankName.Should().Be("Nepal Bank");
    }

    [Fact]
    public async Task GetBanks_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<Bank>().AddRange(
            new Bank { BankName = "Bank A", BankCode = "BA", Remarks = "", IsActive = true },
            new Bank { BankName = "Bank B", BankCode = "BB", Remarks = "", IsActive = true }
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new BankService(context);
        var (items, totalCount) = await service.GetBanksAsync(1, 10, null, "bankname", "asc");

        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBanks_WithSearch_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<Bank>().AddRange(
            new Bank { BankName = "Alpha Bank", BankCode = "AB", Remarks = "", IsActive = true },
            new Bank { BankName = "Beta Finance", BankCode = "BF", Remarks = "", IsActive = true }
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new BankService(context);
        var (items, totalCount) = await service.GetBanksAsync(1, 10, "Alpha", "bankname", "asc");

        totalCount.Should().Be(1);
        items[0].BankName.Should().Be("Alpha Bank");
    }

    [Fact]
    public async Task UpdateBank_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var bank = new Bank { BankName = "Original", BankCode = "OR", Remarks = "", IsActive = true };
        context.Set<Bank>().Add(bank);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new BankService(context);

        var existing = await service.GetBankByIdAsync(bank.Id);
        existing!.BankName = "Updated";
        await service.UpdateBankAsync(existing);

        context.ChangeTracker.Clear();
        var updated = await service.GetBankByIdAsync(bank.Id);
        updated!.BankName.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteBank_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var bank = new Bank { BankName = "Delete Me", BankCode = "DM", Remarks = "", IsActive = true };
        context.Set<Bank>().Add(bank);
        await context.SaveChangesAsync();

        var service = new BankService(context);
        await service.DeleteBankAsync(bank.Id);

        var exists = await service.BankExistsAsync(bank.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task BankExists_ShouldReturnTrue_WhenExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var bank = new Bank { BankName = "Exists", BankCode = "EX", Remarks = "", IsActive = true };
        context.Set<Bank>().Add(bank);
        await context.SaveChangesAsync();

        var service = new BankService(context);
        var exists = await service.BankExistsAsync(bank.Id);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task BankExists_ShouldReturnFalse_WhenNotExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new BankService(context);

        var exists = await service.BankExistsAsync(999);
        exists.Should().BeFalse();
    }
}
