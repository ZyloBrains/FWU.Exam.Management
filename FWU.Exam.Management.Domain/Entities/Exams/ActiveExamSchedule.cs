using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ActiveExamSchedule
{
    public int Id { get; set; }

    public int ExamScheduleId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    [MaxLength(1024)]
    public string? Remarks { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }
}
