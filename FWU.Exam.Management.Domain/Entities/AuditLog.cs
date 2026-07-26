using System.ComponentModel.DataAnnotations;
using FWU.Exam.Management.Domain.Interfaces;

namespace FWU.Exam.Management.Domain.Entities;

public class AuditLog : ITenantScoped
{
    public int Id { get; set; }
    public int? TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [MaxLength(128)]
    public string? EntityName { get; set; }

    [MaxLength(128)]
    public string? EntityId { get; set; }

    [MaxLength(32)]
    public string? Action { get; set; }

    [MaxLength(256)]
    public string? UserName { get; set; }

    [MaxLength(128)]
    public string? UserId { get; set; }

    public DateTime Timestamp { get; set; }

    [MaxLength(4000)]
    public string? ChangesJson { get; set; }
}
