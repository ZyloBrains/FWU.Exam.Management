using fwu_examination_management_system.Data.Models.Payments;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamScheduleParent
{
    [Key]
    public int ExamScheduleParentId { get; set; }

    [Required, MaxLength(255)]
    public string ExamScheduleParentName { get; set; }

    public bool IsActive { get; set; }
    [ValidateNever]
    public virtual ICollection<BankVoucher> BankVouchers { get; set; }
    [ValidateNever]
    public virtual ICollection<ExamRollNumberSetup> ExamRollNumberSetups { get; set; }
    [ValidateNever]
    public virtual ICollection<ExamSchedule> ExamSchedules { get; set; }
}
