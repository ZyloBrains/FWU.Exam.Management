using fwu_examination_management_system.Data.Auditing;
using fwu_examination_management_system.Data.Models.Colleges;
using fwu_examination_management_system.Data.Models.Exams;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Payments;

public class BankVoucher: IAuditable
{
    public int Id { get; set; }

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

    public virtual AcademicYear? AcademicYear { get; set; }
    public virtual College? College { get; set; }
    public virtual BillTitle? BillTitle { get; set; }
    public virtual Bank? Bank { get; set; }
    public virtual ExamScheduleParent? ExamScheduleParent { get; set; }
    public virtual UserAttachment? UserAttachment { get; set; }
}
