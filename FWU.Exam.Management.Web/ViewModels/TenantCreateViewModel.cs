using System.ComponentModel.DataAnnotations;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FWU.Exam.Management.Web.ViewModels;

public class TenantCreateViewModel
{
    public Tenant Tenant { get; set; } = new();

    [Display(Name = "Faculty")]
    public int? SelectedFacultyId { get; set; }
    public IEnumerable<SelectListItem>? FacultyList { get; set; }

    [Required(ErrorMessage = "Admin full name is required.")]
    [Display(Name = "Admin Full Name")]
    public string AdminFullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Admin email is required.")]
    [EmailAddress]
    [Display(Name = "Admin Email")]
    public string AdminEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Admin password is required.")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Admin Password")]
    public string AdminPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm Admin Password")]
    [Compare("AdminPassword", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmAdminPassword { get; set; } = string.Empty;
}
