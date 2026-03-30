using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
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
}
