using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class SmtpConfiguration
{
    public int Id { get; set; }

    [Required, MaxLength(1024)]
    public string? Host { get; set; }

    [Required, MaxLength(1024)]
    public string? From { get; set; }

    public int Port { get; set; }

    [Required, MaxLength(1024)]
    public string? UserName { get; set; }

    [Required, MaxLength(1024)]
    public string? Password { get; set; }

    public bool EnableSsl { get; set; }
}
