using FWU.Exam.Management.Domain.Entities.Payments;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamScheduleParent
{
    public int Id { get; set; }

    [Required, MaxLength(255)]
    public string? ExamScheduleParentName { get; set; }

    public bool IsActive { get; set; }
    public virtual ICollection<ExamRollNumberSetup>? ExamRollNumberSetups { get; set; }
    public virtual ICollection<ExamSchedule>? ExamSchedules { get; set; }
}
