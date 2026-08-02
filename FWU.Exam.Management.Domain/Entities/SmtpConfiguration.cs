using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class SmtpConfiguration
{
    public int Id { get; set; }

    [Required, MaxLength(1024)]
    [Display(Name = "Host")]
    public string Host { get; set; } = string.Empty;

    [Required, MaxLength(1024)]
    [Display(Name = "From")]
    public string From { get; set; } = string.Empty;

    [Range(1, 65535)]
    [Display(Name = "Port")]
    public int Port { get; set; }

    [Required, MaxLength(1024)]
    [Display(Name = "User Name")]
    public string UserName { get; set; } = string.Empty;

    [Required, MaxLength(1024)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Enable SSL")]
    public bool EnableSsl { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }
}
