using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Payments;

public class PaymentType
{
    public int Id { get; set; }

    [Required, MaxLength(255)]
    public string? PaymentTypeName { get; set; }

    public bool IsActive { get; set; }
    public virtual ICollection<PaymentRequestLog>? PaymentRequestLogs { get; set; }
}
