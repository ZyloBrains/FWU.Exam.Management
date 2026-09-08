using FWU.Exam.Management.Domain.Enums;

namespace FWU.Exam.Management.Domain.Constants;

/// <summary>
/// String values stored on <see cref="Entities.Payments.PaymentRequestLog.PaymentStatus"/>
/// (the dedicated, searchable status column). Semantics mirror the recommended lifecycle:
/// Created → Initiated → Completed | Failed | Cancelled | Refunded, with Pending as an
/// intermediary "awaiting gateway" state.
/// </summary>
public static class PaymentStatusValues
{
    public const string Created = "Created";
    public const string Initiated = "Initiated";
    public const string Pending = "Pending";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
    public const string Refunded = "Refunded";

    public static string ToValue(PaymentStatus status) => status switch
    {
        Enums.PaymentStatus.Created => Created,
        Enums.PaymentStatus.Initiated => Initiated,
        Enums.PaymentStatus.Pending => Pending,
        Enums.PaymentStatus.Completed => Completed,
        Enums.PaymentStatus.Failed => Failed,
        Enums.PaymentStatus.Cancelled => Cancelled,
        Enums.PaymentStatus.Refunded => Refunded,
        _ => Pending
    };

    /// <summary>
    /// Maps to the legacy <c>PaymentRequestLogStatus</c> int convention so existing queries
    /// (reconciliation poll, open-apply-again, verification list) keep working.
    ///   Completed → 1 (paid/confirmed)
    ///   Created/Initiated/Pending → null (still open)
    ///   Failed/Cancelled → 0 (failed / open-unconfirmed)
    ///   Terminal/superseded → 2 (closed, never revisited)
    /// </summary>
    public static int? ToLegacyStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        return status switch
        {
            Completed => PaymentRequestLogStatuses.Confirmed,
            Failed or Cancelled => PaymentRequestLogStatuses.Failed,
            Refunded => PaymentRequestLogStatuses.Terminal,
            _ => PaymentRequestLogStatuses.Pending
        };
    }
}
