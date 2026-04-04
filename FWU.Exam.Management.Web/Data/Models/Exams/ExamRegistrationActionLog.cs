using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamRegistrationActionLog
{
    public int Id { get; set; }

    public int ExamRegistrationId { get; set; }
    public DateTime Timestamp { get; set; }

    [Required, MaxLength(255)]
    public string? Action { get; set; }

    public string? Remarks { get; set; }

    public virtual ExamRegistration? ExamRegistration { get; set; }
}
