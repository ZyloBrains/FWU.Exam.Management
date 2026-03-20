using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class BillTitle
    {
        [Key]
        public int BillTitleId { get; set; }

        [Required, MaxLength(255)]
        public string BillTitleName { get; set; }

        [MaxLength(1024)]
        public string Category { get; set; }

        public bool IsActive { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int? ModifiedByUserId { get; set; }
        public DateTime? ModifiedDateTime { get; set; }

        public decimal? Amount { get; set; }
        public DateTime? ThroughDate { get; set; }
        public DateTime? ApplicableDate { get; set; }
        public int? ExamScheduleId { get; set; }

        [ForeignKey(nameof(ExamScheduleId))]
        public virtual ExamSchedule ExamSchedule { get; set; }

        public virtual ICollection<BankVoucher> BankVouchers { get; set; }
    }
}
