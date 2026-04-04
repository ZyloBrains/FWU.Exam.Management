using fwu_examination_management_system.Data.Models.Payments;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamScheduleParent
{
    public int Id { get; set; }

    [Required, MaxLength(255)]
    public string? ExamScheduleParentName { get; set; }

    public bool IsActive { get; set; }
    public virtual ICollection<BankVoucher>? BankVouchers { get; set; }
    public virtual ICollection<ExamRollNumberSetup>? ExamRollNumberSetups { get; set; }
    public virtual ICollection<ExamSchedule>? ExamSchedules { get; set; }
}
