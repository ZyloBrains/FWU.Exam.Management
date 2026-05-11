namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamRollNumberSetup
{
    public int Id { get; set; }

    public int ExamScheduleId { get; set; }
    public int FirstExamRollNumber { get; set; }

    public string? Prefix { get; set; }

    public string? Suffix { get; set; }

    public string? DetailsJson { get; set; }

    public int MinimumRollNumberLength { get; set; }
    public int Round { get; set; }
    public int MinimumGap { get; set; }
    public bool IsActive { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }
}
