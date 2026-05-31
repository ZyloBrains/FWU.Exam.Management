using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Web.ViewModels;

public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Full Name")]
    public string? FullName { get; set; }

    [Display(Name = "Faculty")]
    public int? FacultyId { get; set; }

    [Display(Name = "College")]
    public int? CollegeId { get; set; }
}
