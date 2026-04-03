using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models.Payments;

public class PaymentPracticalSubjects
{
    [Key]
    public int PaymentPracticalSubjectsId { get; set; }

    public int PaymentRequestLogId { get; set; }
    public int PracticalSubjectsCount { get; set; }
    public decimal TotalAmount { get; set; }

    [ForeignKey(nameof(PaymentRequestLogId))]
    [ValidateNever]
    public virtual PaymentRequestLog PaymentRequestLog { get; set; }
}
