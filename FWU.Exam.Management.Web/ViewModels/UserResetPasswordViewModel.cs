using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Web.ViewModels;

public class UserResetPasswordViewModel
{
    public string UserId { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty;

    public string? FullName { get; set; }

    public string? FacultyName { get; set; }

    public string? CollegeName { get; set; }

    public bool IsActive { get; set; }

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 8)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm the new password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class UserResetPasswordPageViewModel
{
    public UserResetPasswordViewModel? SelectedUser { get; set; }

    public List<UserListItemViewModel> Users { get; set; } = [];

    public string? Search { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
