using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class GumpNowEmailLog
{
    public int Id { get; set; }

    [Required, MaxLength(500)]
    [Display(Name = "To Address")]
    public string ToAddr { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "From Address")]
    public string? FromAddr { get; set; }

    [MaxLength(500)]
    [Display(Name = "Subject")]
    public string? Subject { get; set; }

    [Display(Name = "Template Code")]
    [MaxLength(100)]
    public string? TemplateId { get; set; }

    [Display(Name = "Context Json")]
    public string? ContextJson { get; set; }

    [MaxLength(50)]
    [Display(Name = "Mode")]
    public string? Mode { get; set; }

    [MaxLength(50)]
    [Display(Name = "Status")]
    public string Status { get; set; } = "Pending";

    [Display(Name = "Error Message")]
    public string? ErrorMessage { get; set; }

    [Display(Name = "Sent At")]
    public DateTime SentAt { get; set; }
}
