using System.ComponentModel.DataAnnotations;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Infrastructure.Data.Models;

public class PasswordResetLog
{
    public int Id { get; set; }

    public string? UserId { get; set; }

    [MaxLength(400)]
    public string? Browser { get; set; }

    [MaxLength(400)]
    public string? Device { get; set; }

    [MaxLength(400)]
    public string? IpAddress { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? PasswordChangedDate { get; set; }

    public virtual AppUser? User { get; set; }
}
