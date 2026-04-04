using fwu_examination_management_system.Data.Models.Colleges;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamCenter
{
    public int Id { get; set; }

    public int ExamScheduleId { get; set; }
    public int CollegeId { get; set; }

    [MaxLength(255)]
    public string? Remark { get; set; }

    public bool IsActive { get; set; }

    public int Code { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }

    public virtual College? College { get; set; }
    public virtual ICollection<ExamCenterDetail>? ExamCenterDetails { get; set; }
    public virtual ICollection<ExamRegistration>? ExamRegistrations { get; set; }
}
