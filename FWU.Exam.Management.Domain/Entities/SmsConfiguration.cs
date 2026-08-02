using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class SmsConfiguration
{
    public int Id { get; set; }

    [Required, MaxLength(1024)]
    [Display(Name = "Api Url")]
    public string ApiUrl { get; set; } = string.Empty;

    [Required, MaxLength(2048)]
    [Display(Name = "Api Key")]
    public string ApiKey { get; set; } = string.Empty;

    [MaxLength(50)]
    [Display(Name = "Mode")]
    public string? Mode { get; set; }

    [MaxLength(500)]
    [Display(Name = "Tags")]
    public string? Tags { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }
}
