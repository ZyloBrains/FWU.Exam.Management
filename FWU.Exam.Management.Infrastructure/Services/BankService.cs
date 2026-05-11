using System.Linq.Expressions;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class BankService(AppDbContext context) : IBankService
{
    public async Task<(List<Bank> Items, int TotalCount)> GetBanksAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.Banks.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(b =>
                b.BankName.Contains(search) ||
                b.BankCode.Contains(search) ||
                b.Remarks.Contains(search));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Bank>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.Banks.AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(b =>
                b.BankName.Contains(search) ||
                b.BankCode.Contains(search) ||
                b.Remarks.Contains(search));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<Bank?> GetBankByIdAsync(int id)
    {
        return await context.Banks.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateBankAsync(Bank bank)
    {
        context.Banks.Add(bank);
        await context.SaveChangesAsync();
    }

    public async Task UpdateBankAsync(Bank bank)
    {
        context.Banks.Update(bank);
        await context.SaveChangesAsync();
    }

    public async Task DeleteBankAsync(int id)
    {
        var bank = await context.Banks.FindAsync(id);
        if (bank != null)
        {
            context.Banks.Remove(bank);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> BankExistsAsync(int id)
    {
        return await context.Banks.AnyAsync(b => b.Id == id);
    }

    private static Expression<Func<Bank, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "bankname" => b => b.BankName ?? "",
            "bankcode" => b => b.BankCode ?? "",
            "remarks" => b => b.Remarks ?? "",
            "isactive" => b => b.IsActive,
            _ => b => b.BankName ?? ""
        };
    }
}
