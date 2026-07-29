using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Web.ViewModels;

public class TestGumpNowSmsViewModel
{
    [Display(Name = "Phone Number")]
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "Message")]
    [Required]
    public string Message { get; set; } = "This is a test SMS from the FWU Examination System. If you received this, the SMS service is working correctly.";
}
