using FWU.Exam.Management.Domain.Entities.Colleges;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamRollNumberSetupDetail
{
    public int Id { get; set; }

    public int ExamRollNumberSetupId { get; set; }
    public int ExamScheduleId { get; set; }
    public int ProgramId { get; set; }
    public int ExamTypeId { get; set; }
    public int CollegeId { get; set; }
    public int StartRollNumber { get; set; }
    public int EndRollNumber { get; set; }
    public int Count { get; set; }

    [MaxLength(50)]
    public string? Prefix { get; set; }

    [MaxLength(50)]
    public string? Suffix { get; set; }

    public virtual ExamRollNumberSetup? ExamRollNumberSetup { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }

    public virtual Program? Program { get; set; }

    public virtual ExamType? ExamType { get; set; }

    public virtual College? College { get; set; }
}
