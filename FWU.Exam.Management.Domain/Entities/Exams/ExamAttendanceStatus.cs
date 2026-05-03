using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamAttendanceStatus
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string? ExamAttendanceStatusName { get; set; }
}
