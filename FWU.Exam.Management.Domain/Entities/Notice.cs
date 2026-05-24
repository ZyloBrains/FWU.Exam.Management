using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Notice : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Required, MaxLength(1024)]
    public string? NoticeTitle { get; set; }

    [Required, MaxLength(1024)]
    public string? NoticePreview { get; set; }

    public DateTime? PublishedDate { get; set; }

    [Required]
    public string? NoticeContent { get; set; }
}
