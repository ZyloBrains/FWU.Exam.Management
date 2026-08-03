using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class GumpNowEmailConfiguration
{
    public int Id { get; set; }

    [Required, MaxLength(1024)]
    [Display(Name = "Api Url")]
    public string ApiUrl { get; set; } = string.Empty;

    [Required, MaxLength(2048)]
    [Display(Name = "Api Key")]
    public string ApiKey { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    [Display(Name = "From Address")]
    public string FromAddr { get; set; } = string.Empty;

    [MaxLength(50)]
    [Display(Name = "Mode")]
    public string? Mode { get; set; }

    [Display(Name = "Override Unsubscription")]
    public bool OverrideUnsubscription { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }
}
