using FWU.Exam.Management.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Notifications;

/// <summary>
/// A single global, editable notification template. Content uses {{Placeholder}} tokens
/// that are replaced at send time with a context dictionary.
/// </summary>
public class NotificationTemplate
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Code")]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Channel")]
    public NotificationChannel Channel { get; set; }

    [MaxLength(250)]
    [Display(Name = "Subject")]
    public string? Subject { get; set; }

    [Required]
    [Display(Name = "Body")]
    public string Body { get; set; } = string.Empty;

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    [Display(Name = "Available Placeholders")]
    public string? PlaceholdersHelp { get; set; }
}
