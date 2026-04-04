using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Payments;

public class PaymentType
{
    public int Id { get; set; }

    [Required, MaxLength(255)]
    public string PaymentTypeName { get; set; }

    public bool IsActive { get; set; }
    [ValidateNever]
    public virtual ICollection<PaymentRequestLog> PaymentRequestLogs { get; set; }
}
