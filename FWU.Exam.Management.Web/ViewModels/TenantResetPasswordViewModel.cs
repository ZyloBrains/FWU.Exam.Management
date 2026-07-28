using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Web.ViewModels;

public class TenantResetPasswordViewModel
{
    public int TenantId { get; set; }

    [Display(Name = "Tenant Name")]
    public string TenantName { get; set; } = string.Empty;

    [Display(Name = "Office Code")]
    public string OfficeCode { get; set; } = string.Empty;

    [Display(Name = "Admin Email")]
    public string AdminEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm the new password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
