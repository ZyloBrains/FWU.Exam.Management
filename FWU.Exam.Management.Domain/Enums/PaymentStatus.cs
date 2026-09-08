namespace FWU.Exam.Management.Domain.Enums;

/// <summary>
/// Lifecycle states for a payment raised against a gateway. The recommended set is
/// Created → Initiated → Pending → Completed | Failed | Cancelled | Refunded.
/// <c>Paid</c> is retained as a legacy alias for <c>Completed</c> so existing views/tests
/// that still reference it continue to compile; the string column stores the canonical
/// value via <see cref="Constants.PaymentStatusValues"/>.
/// </summary>
public enum PaymentStatus
{
    Pending,
    Paid,
    Failed,
    Cancelled,
    Refunded,
    Created,
    Initiated,
    Completed
}