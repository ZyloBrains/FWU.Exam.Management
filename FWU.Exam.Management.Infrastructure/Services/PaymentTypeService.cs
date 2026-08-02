using System.Linq.Expressions;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Payments;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class PaymentTypeService(AppDbContext context) : IPaymentTypeService
{
    public async Task<(List<PaymentType> Items, int TotalCount)> GetPaymentTypesAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.Set<PaymentType>().AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(pt =>
                pt.PaymentTypeName.Contains(search));
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

    public async Task<List<PaymentType>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.Set<PaymentType>().AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(pt =>
                pt.PaymentTypeName.Contains(search));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<PaymentType?> GetPaymentTypeByIdAsync(int id)
    {
        return await context.Set<PaymentType>().FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreatePaymentTypeAsync(PaymentType paymentType)
    {
        context.Set<PaymentType>().Add(paymentType);
        await context.SaveChangesAsync();
    }

    public async Task UpdatePaymentTypeAsync(PaymentType paymentType)
    {
        var existing = await context.Set<PaymentType>().AsNoTracking().FirstOrDefaultAsync(pt => pt.Id == paymentType.Id);
        if (existing is null) throw new InvalidOperationException("Payment type not found.");
        paymentType.TenantId = existing.TenantId;
        context.Set<PaymentType>().Update(paymentType);
        await context.SaveChangesAsync();
    }

    public async Task DeletePaymentTypeAsync(int id)
    {
        var paymentType = await context.Set<PaymentType>().FindAsync(id);
        if (paymentType != null)
        {
            context.Set<PaymentType>().Remove(paymentType);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> PaymentTypeExistsAsync(int id)
    {
        return await context.Set<PaymentType>().AnyAsync(pt => pt.Id == id);
    }

    private static Expression<Func<PaymentType, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "paymenttypename" => pt => pt.PaymentTypeName ?? "",
            "isactive" => pt => pt.IsActive,
            _ => pt => pt.PaymentTypeName ?? ""
        };
    }
}
