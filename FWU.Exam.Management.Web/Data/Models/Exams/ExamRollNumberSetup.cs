using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamRollNumberSetup
{
    [Key]
    public int ExamRollNumberSetupId { get; set; }

    public int ExamScheduleParentId { get; set; }
    public int FirstExamRollNumber { get; set; }

    [MaxLength(50)]
    public string? Prefix { get; set; }

    [MaxLength(50)]
    public string? Suffix { get; set; }

    public int MinimumRollNumberLength { get; set; }
    public int Round { get; set; }
    public int MinimumGap { get; set; }
    public bool IsActive { get; set; }

    [ForeignKey(nameof(ExamScheduleParentId))]
    [ValidateNever]
    public virtual ExamScheduleParent ExamScheduleParent { get; set; }
    [ValidateNever]
    public virtual ICollection<ExamRollNumberSetupDetail> ExamRollNumberSetupDetails { get; set; }
}
