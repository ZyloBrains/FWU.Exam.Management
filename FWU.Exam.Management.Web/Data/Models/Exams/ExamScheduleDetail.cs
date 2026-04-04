using fwu_examination_management_system.Data.Models.Subjects;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamScheduleDetail
{
    public int Id { get; set; }

    public int ExamScheduleId { get; set; }
    public int ExamTypeId { get; set; }
    public int SubjectDetailId { get; set; }
    public DateTime ExamDate { get; set; }

    [MaxLength(10)]
    public string? ExamDateBs { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }

    public virtual ExamType? ExamType { get; set; }

    public virtual SubjectDetail? SubjectDetail { get; set; }
}
