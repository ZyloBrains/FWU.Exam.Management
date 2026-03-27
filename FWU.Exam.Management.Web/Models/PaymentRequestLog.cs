using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class PaymentRequestLog
    {
        [Key]
        public int PaymentRequestLogId { get; set; }
        public int? PaymentRequestLogStatus { get; set; }     //extra field ??

        [Required, MaxLength(50)]
        public string InvoiceNumber { get; set; }

        public DateTime ForwardedTimestamp { get; set; }
        public DateTime? DateOfBirthAd { get; set; }

        [MaxLength(20)]
        public string? MobileNumber { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [Required, MaxLength(255)]
        public string FullName { get; set; }

        public decimal Amount { get; set; }

        [Required]
        public string FullRequestContent { get; set; }

        public int PaymentTypeId { get; set; }
        public int? StudentRegistrationId { get; set; }
        public int ExamScheduleId { get; set; }

        [MaxLength(50)]
        public string? TransactionId { get; set; }

        public int? CollegeId { get; set; }
        public int StudentCount { get; set; }

        [ForeignKey(nameof(PaymentTypeId))]
        [ValidateNever]
        public virtual PaymentType PaymentType { get; set; }

        [ForeignKey(nameof(StudentRegistrationId))]
        [ValidateNever]
        public virtual StudentRegistration StudentRegistration { get; set; }

        [ForeignKey(nameof(ExamScheduleId))]
        [ValidateNever]
        public virtual ExamSchedule ExamSchedule { get; set; }

        [ForeignKey(nameof(CollegeId))]
        [ValidateNever]
        public virtual College College { get; set; }
        [ValidateNever]
        public virtual ICollection<PaymentResponseLog> PaymentResponseLog { get; set; }
        [ValidateNever]
        public virtual ICollection<PaymentPracticalSubjects> PaymentPracticalSubjects { get; set; }
    }
}
