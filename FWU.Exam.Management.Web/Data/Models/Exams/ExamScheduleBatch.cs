namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamScheduleBatch
{
    public int Id { get; set; }

    public int ExamScheduleId { get; set; }
    public int ExamTypeId { get; set; }
    public int BatchId { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }

    public virtual ExamType? ExamType { get; set; }

    public virtual Batch? Batch { get; set; }
}
