using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class PaymentPracticalSubjects
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int PaymentRequestLogId { get; set; }
        public int PracticalSubjectsCount { get; set; }
        public decimal TotalAmount { get; set; }

        [ForeignKey(nameof(PaymentRequestLogId))]
        public virtual PaymentRequestLog PaymentRequestLog { get; set; }
    }
}
