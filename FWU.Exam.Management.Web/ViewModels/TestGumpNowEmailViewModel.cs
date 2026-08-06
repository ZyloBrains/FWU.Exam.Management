using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Web.ViewModels;

public class TestGumpNowEmailViewModel
{
    [Display(Name = "To Email")]
    [Required, EmailAddress]
    public string ToEmail { get; set; } = string.Empty;

    [Display(Name = "Subject")]
    [Required]
    public string Subject { get; set; } = "Test Email from FWU Examination System";

    [Display(Name = "Send Mode")]
    [Required]
    public string SendMode { get; set; } = "html";

    [Display(Name = "Template Code")]
    public string? TemplateId { get; set; }

    [Display(Name = "Context Variables (key=value, one per line)")]
    public string? ContextVariables { get; set; }

    [Display(Name = "HTML Body")]
    public string? HtmlBody { get; set; }
}
