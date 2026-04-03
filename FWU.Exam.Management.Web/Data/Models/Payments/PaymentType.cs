using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Payments;

public class PaymentType
{
    [Key]
    public int PaymentTypeId { get; set; }

    [Required, MaxLength(255)]
    public string PaymentTypeName { get; set; }

    public bool IsActive { get; set; }     // IsActive = Status
    [ValidateNever]
    public virtual ICollection<PaymentRequestLog> PaymentRequestLogs { get; set; }
}
