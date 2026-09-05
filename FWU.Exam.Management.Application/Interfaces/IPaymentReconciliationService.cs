using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IPaymentReconciliationService
{
    Task<(List<PaymentReconciliationListDto> Items, int TotalCount)> GetPendingPaymentsAsync(
        string? search, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<PaymentReconciliationResult> ReconcilePaymentAsync(int logId);
    Task<PaymentReconciliationResult> MarkPaymentFailedAsync(int logId, string reason);
    Task<PaymentReconciliationResult> ConfirmPaymentManuallyAsync(int logId, string? remark);
    Task<int> ReconcilePendingBatchAsync();
    Task<PaymentReconciliationBatchResult> ReconcilePendingWithDetailsAsync();
    Task<List<PaymentReconciliationListDto>> GetReconcileablePendingAsync();
}
