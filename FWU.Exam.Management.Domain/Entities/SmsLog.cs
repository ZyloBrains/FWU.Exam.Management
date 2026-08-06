using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class SmsLog
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "To Address")]
    public string ToAddr { get; set; } = string.Empty;

    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;

    [MaxLength(50)]
    [Display(Name = "Mode")]
    public string? Mode { get; set; }

    [Display(Name = "Tags Json")]
    public string? TagsJson { get; set; }

    [MaxLength(50)]
    [Display(Name = "Status")]
    public string Status { get; set; } = "Pending";

    [Display(Name = "Error Message")]
    public string? ErrorMessage { get; set; }

    [Display(Name = "Sent At")]
    public DateTime SentAt { get; set; }
}
