using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class SmtpConfiguration
{
    [Key]
    public int SmtpConfigurationId { get; set; }

    [Required, MaxLength(1024)]
    public string Host { get; set; }

    [Required, MaxLength(1024)]
    public string From { get; set; }

    public int Port { get; set; }

    [Required, MaxLength(1024)]
    public string UserName { get; set; }

    [Required, MaxLength(1024)]
    public string Password { get; set; }

    public bool EnableSsl { get; set; }
}
