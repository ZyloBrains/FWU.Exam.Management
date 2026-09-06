namespace FWU.Exam.Management.Infrastructure.Services;

/// <summary>
/// Shared interpretation of Khalti lookup status values used by both the student
/// callback and the payment-reconciliation poller.
/// </summary>
public static class KhaltiPaymentStatus
{
    /// <summary>
    /// True when the Khalti lookup status is a terminal, non-success outcome: the
    /// payment can never reach "Completed", so there is no point polling it again.
    /// </summary>
    public static bool IsTerminalStatus(string? status)
    {
        return !string.IsNullOrWhiteSpace(status) &&
               (string.Equals(status, "Expired", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "User canceled", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "Canceled", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "Failed", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// A student-friendly message for a verification where the lookup was not
    /// "Completed", reassuring the student that no amount was charged and they can
    /// safely make a fresh payment.
    /// </summary>
    public static string GetVerificationFailureMessage(string? status)
    {
        if (string.Equals(status, "Expired", StringComparison.OrdinalIgnoreCase))
            return "This payment link has expired and no amount was charged. Please start a new payment.";

        if (string.Equals(status, "User canceled", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "Canceled", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            return "Payment was canceled. No amount was charged.";

        if (string.Equals(status, "Initiated", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase))
            return "Payment not completed — no amount was charged. If you believe you paid, please contact support; otherwise you can try again.";

        return "Payment verification failed. No amount was charged. If you believe you paid, please contact support; otherwise you can try again.";
    }
}
