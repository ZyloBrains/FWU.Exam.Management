using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Organization")]
        public int? OrganizationId { get; set; }
    }
}
