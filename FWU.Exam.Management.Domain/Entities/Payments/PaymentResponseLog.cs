using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Payments;

public class PaymentResponseLog : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }
    public int PaymentRequestLogId { get; set; }

    public DateTime ResponseTimestamp { get; set; }
    public bool IsSuccess { get; set; }

    [Required, MaxLength(1024)]
    public string? ResponseMessage { get; set; }

    [Required]
    public string? FullResponse { get; set; }

    public virtual PaymentRequestLog? PaymentRequestLog { get; set; }
}
