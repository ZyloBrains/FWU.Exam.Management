namespace FWU.Exam.Management.Domain.Constants;

/// <summary>
/// Convention values for <see cref="Entities.Payments.PaymentRequestLog.PaymentRequestLogStatus"/>.
/// These are stored as a raw nullable int (no DB enum). Semantics across the app:
///   null = initiated / not yet processed
///   0    = failed / open-unconfirmed
///   1    = confirmed / PAID
///   2    = terminal / superseded / cancelled (never revisited)
///   3    = payment acknowledged by the gateway (COMPLETE/Completed) but could not be
///          linked to an existing log during the callback, so the exam office must
///          verify the payment receipt manually before an admit card is issued.
public static class PaymentRequestLogStatuses
{
    public static readonly int? Pending = null;
    public static readonly int? Failed = 0;
    public static readonly int? Confirmed = 1;
    public static readonly int? Terminal = 2;
    public static readonly int? PendingVerification = 3;
}