using fwu_examination_management_system.Data.Auditing;
using fwu_examination_management_system.Data.Models.Colleges;
using fwu_examination_management_system.Data.Models.Exams;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models.Payments;

public class BankVoucher: IAuditable
{
    [Key]
    public int BankVoucherId { get; set; }

    public int AcademicYearId { get; set; }
    public int CollegeId { get; set; }
    public int BillTitleId { get; set; }
    public int BankId { get; set; }

    [MaxLength(100)]
    public string? BankAddress { get; set; }

    public DateTime VoucherDate { get; set; }

    [MaxLength(50)]
    public string? VoucherNumber { get; set; }

    public decimal VoucherAmount { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public int? BankVoucherUserAttachmentId { get; set; }
    public int ExamScheduleParentId { get; set; }

    [ForeignKey(nameof(AcademicYearId))]
    [ValidateNever]
    public virtual AcademicYear? AcademicYear { get; set; }

    [ForeignKey(nameof(CollegeId))]
    [ValidateNever]
    public virtual College? College { get; set; }

    [ForeignKey(nameof(BillTitleId))]
    [ValidateNever]
    public virtual BillTitle? BillTitle { get; set; }

    [ForeignKey(nameof(BankId))]
    [ValidateNever]
    public virtual Bank? Bank { get; set; }

    [ForeignKey(nameof(ExamScheduleParentId))]
    [ValidateNever]
    public virtual ExamScheduleParent ExamScheduleParent { get; set; }

    [ForeignKey(nameof(BankVoucherUserAttachmentId))]
    [ValidateNever]
    public virtual UserAttachment UserAttachment { get; set; }
}
