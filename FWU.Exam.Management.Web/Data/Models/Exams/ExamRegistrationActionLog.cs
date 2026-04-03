using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamRegistrationActionLog
{
    [Key]
    public int ExamRegistrationActionLogId { get; set; }

    public int ExamRegistrationId { get; set; }
    public DateTime Timestamp { get; set; }

    [Required, MaxLength(255)]
    public string Action { get; set; }

    public string? Remarks { get; set; }

    [ForeignKey(nameof(ExamRegistrationId))]
    [ValidateNever]
    public virtual ExamRegistration ExamRegistration { get; set; }
}
