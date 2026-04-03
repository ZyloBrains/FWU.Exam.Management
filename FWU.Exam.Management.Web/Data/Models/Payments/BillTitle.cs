using fwu_examination_management_system.Data.Models.Exams;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models.Payments;

public class BillTitle
{
    [Key]
    public int BillTitleId { get; set; }

    [Required, MaxLength(255)]
    public string BillTitleName { get; set; }

    [MaxLength(1024)]
    public string? Category { get; set; }

    public bool IsActive { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? ThroughDate { get; set; }
    public DateTime? ApplicableDate { get; set; }
    public int? ExamScheduleId { get; set; }

    [ForeignKey(nameof(ExamScheduleId))]
    [ValidateNever]
    public virtual ExamSchedule ExamSchedule { get; set; }

    [ValidateNever]
    public virtual ICollection<BankVoucher> BankVouchers { get; set; }
}
