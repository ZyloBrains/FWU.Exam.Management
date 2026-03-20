using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
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
        public virtual PaymentRequestLog PaymentRequestLog { get; set; }
    }
}
