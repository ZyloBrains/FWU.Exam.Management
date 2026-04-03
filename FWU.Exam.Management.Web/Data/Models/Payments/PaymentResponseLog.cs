using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models.Payments;

public class PaymentResponseLog
{
    [Key]
    public int PaymentResponseLogId { get; set; }   
    public int PaymentRequestLogId { get; set; }

    public DateTime ResponseTimestamp { get; set; }
    public bool IsSuccess { get; set; }

    [Required, MaxLength(1024)]
    public string ResponseMessage { get; set; }

    [Required]
    public string FullResponse { get; set; }

    [ForeignKey(nameof(PaymentRequestLogId))]
    [ValidateNever]
    public virtual PaymentRequestLog PaymentRequestLog { get; set; }
}
