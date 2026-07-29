using System.ComponentModel.DataAnnotations;
using FWU.Exam.Management.Domain.Interfaces;

namespace FWU.Exam.Management.Domain.Entities;

public class BulkUserCreationJob : ITenantScoped
{
    public int Id { get; set; }
    public int? TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [MaxLength(128)]
    public string UserId { get; set; } = string.Empty;

    public int TotalStudents { get; set; }
    public int ProcessedCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }

    [MaxLength(32)]
    public string Status { get; set; } = "Pending";

    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
