using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities.Payments;

public class PaymentType
{
    public int Id { get; set; }

    [Required, MaxLength(255)]
    [Display(Name = "Payment Type Name")]
    public string? PaymentTypeName { get; set; }

    [MaxLength(500)]
    [Display(Name = "Logo URL")]
    public string? LogoUrl { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }
    public virtual ICollection<PaymentRequestLog>? PaymentRequestLogs { get; set; }
}
