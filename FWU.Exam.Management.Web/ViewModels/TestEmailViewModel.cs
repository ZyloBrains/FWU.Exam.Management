using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Web.ViewModels;

public class TestEmailViewModel
{
    [Display(Name = "To Email")]
    [Required, EmailAddress]
    public string ToEmail { get; set; } = string.Empty;

    [Display(Name = "Subject")]
    [Required]
    public string Subject { get; set; } = string.Empty;

    [Display(Name = "Body")]
    [Required]
    public string Body { get; set; } = string.Empty;
}
