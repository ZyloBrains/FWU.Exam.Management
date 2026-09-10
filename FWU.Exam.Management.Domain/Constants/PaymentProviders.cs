namespace FWU.Exam.Management.Domain.Constants;

/// <summary>
/// Canonical gateway names stored on <see cref="Entities.Payments.PaymentRequestLog.PaymentProvider"/>.
/// These are the normalized values used across initiate, callback and reconciliation so
/// payments can be searched by provider without substring-matching on a display name.
/// </summary>
public static class PaymentProviders
{
    public const string Khalti = "Khalti";
    public const string Esewa = "Esewa";
    public const string ImePay = "ImePay";
    public const string Waiver = "Waiver";

    /// <summary>
    /// Normalizes a free-text payment method (e.g. from a PaymentType name or UI dropdown)
    /// to a canonical provider value, or <c>null</c> when it cannot be recognized.
    /// </summary>
    public static string? Normalize(string? paymentMethod)
    {
        if (string.IsNullOrWhiteSpace(paymentMethod))
            return null;

        var lower = paymentMethod.Trim().ToLowerInvariant();
        if (lower.Contains("waiver"))
            return Waiver;
        if (lower.Contains("khalti"))
            return Khalti;
        if (lower.Contains("esewa") || lower.Contains("e-sewa"))
            return Esewa;
        if (lower.Contains("ime") || lower.Contains("imepay") || lower.Contains("ime pay"))
            return ImePay;

        return null;
    }
}
