using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class SmsConfiguration
{
    public int Id { get; set; }

    [Required, MaxLength(1024)]
    public string? ApiUrl { get; set; }

    [Required, MaxLength(2048)]
    public string? ApiKey { get; set; }

    [MaxLength(50)]
    public string? Mode { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    public bool IsActive { get; set; }
}
