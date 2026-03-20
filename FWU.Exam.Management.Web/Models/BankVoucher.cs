using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class BankVoucher
    {
        [Key]
        public int BankVoucherId { get; set; }

        public int AcademicYearId { get; set; }
        public int CollegeId { get; set; }
        public int BillTitleId { get; set; }
        public int BankId { get; set; }

        [MaxLength(100)]
        public string BankAddress { get; set; }

        public DateTime VoucherDate { get; set; }

        [MaxLength(50)]
        public string VoucherNumber { get; set; }

        public decimal VoucherAmount { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public int? BankVoucherUserAttachmentId { get; set; }
        public int ExamScheduleParentId { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; }

        [ForeignKey(nameof(CollegeId))]
        public virtual College College { get; set; }

        [ForeignKey(nameof(BillTitleId))]
        public virtual BillTitle BillTitle { get; set; }

        [ForeignKey(nameof(BankId))]
        public virtual Bank Bank { get; set; }

        [ForeignKey(nameof(ExamScheduleParentId))]
        public virtual ExamScheduleParent ExamScheduleParent { get; set; }

        [ForeignKey(nameof(BankVoucherUserAttachmentId))]
        public virtual UserAttachment UserAttachment { get; set; }
    }
}
