using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamRollNumberSetup
{
    public int Id { get; set; }

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

    public virtual ExamScheduleParent? ExamScheduleParent { get; set; }
    public virtual ICollection<ExamRollNumberSetupDetail>? ExamRollNumberSetupDetails { get; set; }
}
