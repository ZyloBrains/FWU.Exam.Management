using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamAttendanceStatus
{
    [Key]
    public int ExamAttendanceStatusId { get; set; }

    [Required, MaxLength(50)]
    public string ExamAttendanceStatusName { get; set; }
}
