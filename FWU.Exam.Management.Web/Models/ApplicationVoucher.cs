using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class ApplicationVoucher
    {
        [Key]
        public int ApplicationVoucherId { get; set; }

        [Required, MaxLength(50)]
        public string VoucherNumber { get; set; }

        [Required, MaxLength(1024)]
        public string StudentName { get; set; }

        public DateTime? DateOfBirthAd { get; set; }

        [MaxLength(50)]
        public string DateOfBirthBs { get; set; }

        public decimal Amount { get; set; }
        public DateTime? VoucherDate { get; set; }
        public DateTime? Timestamp { get; set; }

        [Required, MaxLength(1024)]
        public string ContactNumber { get; set; }

        [MaxLength(1024)]
        public string Branch { get; set; }

        public int ExamScheduleId { get; set; }
        public int? StudentRegistrationId { get; set; }

        [ForeignKey(nameof(ExamScheduleId))]
        [ValidateNever]
        public virtual ExamSchedule? ExamSchedule { get; set; }

        [ForeignKey(nameof(StudentRegistrationId))]
        [ValidateNever]
        public virtual StudentRegistration StudentRegistration { get; set; }
    }
}
