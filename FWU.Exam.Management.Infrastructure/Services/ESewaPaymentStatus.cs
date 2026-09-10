namespace FWU.Exam.Management.Infrastructure.Services;

/// <summary>
/// Shared interpretation of eSewa transaction status values used by both the
/// student callback and the payment-reconciliation poller.
/// </summary>
public static class ESewaPaymentStatus
{
    /// <summary>
    /// True when the eSewa status is a terminal, non-success outcome: the payment
    /// can never reach "COMPLETE", so there is no point polling it again.
    /// NOTE: NOT_FOUND is intentionally NOT terminal. Real production logs show
    /// eSewa can report NOT_FOUND/PENDING for a transaction_uuid and later confirm
    /// the SAME uuid as COMPLETE, so those statuses must keep being retried.
    /// </summary>
    public static bool IsTerminalStatus(string? status)
    {
        return !string.IsNullOrWhiteSpace(status) &&
               (string.Equals(status, "FAILED", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "CANCELED", StringComparison.OrdinalIgnoreCase));
    }
}