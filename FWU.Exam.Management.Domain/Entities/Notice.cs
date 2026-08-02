using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Notice : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Required, MaxLength(1024)]
    [Display(Name = "Notice Title")]
    public string NoticeTitle { get; set; } = string.Empty;

    [Required, MaxLength(1024)]
    [Display(Name = "Notice Preview")]
    public string NoticePreview { get; set; } = string.Empty;

    [Display(Name = "Published Date")]
    public DateTime? PublishedDate { get; set; }

    [Required]
    [Display(Name = "Notice Content")]
    public string NoticeContent { get; set; } = string.Empty;
}
