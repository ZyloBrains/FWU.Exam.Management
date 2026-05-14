using FWU.Exam.Management.Domain.Entities.Payments;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IPaymentTypeService
{
    Task<(List<PaymentType> Items, int TotalCount)> GetPaymentTypesAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<PaymentType>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<PaymentType?> GetPaymentTypeByIdAsync(int id);
    Task CreatePaymentTypeAsync(PaymentType paymentType);
    Task UpdatePaymentTypeAsync(PaymentType paymentType);
    Task DeletePaymentTypeAsync(int id);
    Task<bool> PaymentTypeExistsAsync(int id);
}
