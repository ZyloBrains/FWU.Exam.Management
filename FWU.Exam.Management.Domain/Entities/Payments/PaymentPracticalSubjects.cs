namespace FWU.Exam.Management.Domain.Entities.Payments;

public class PaymentPracticalSubjects
{
    public int Id { get; set; }

    public int PaymentRequestLogId { get; set; }
    public int PracticalSubjectsCount { get; set; }
    public decimal TotalAmount { get; set; }

    public virtual PaymentRequestLog? PaymentRequestLog { get; set; }
}
