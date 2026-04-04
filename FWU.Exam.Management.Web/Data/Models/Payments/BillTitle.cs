using fwu_examination_management_system.Data.Models.Exams;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Payments;

public class BillTitle
{
    public int Id { get; set; }

    [Required, MaxLength(255)]
    public string? BillTitleName { get; set; }

    [MaxLength(1024)]
    public string? Category { get; set; }

    public bool IsActive { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? ThroughDate { get; set; }
    public DateTime? ApplicableDate { get; set; }
    public int? ExamScheduleId { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }

    public virtual ICollection<BankVoucher>? BankVouchers { get; set; }
}
