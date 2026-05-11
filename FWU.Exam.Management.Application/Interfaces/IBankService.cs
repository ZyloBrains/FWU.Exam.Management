using FWU.Exam.Management.Domain.Entities.Payments;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IBankService
{
    Task<(List<Bank> Items, int TotalCount)> GetBanksAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<Bank>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<Bank?> GetBankByIdAsync(int id);
    Task CreateBankAsync(Bank bank);
    Task UpdateBankAsync(Bank bank);
    Task DeleteBankAsync(int id);
    Task<bool> BankExistsAsync(int id);
}
