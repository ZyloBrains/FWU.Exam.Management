using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Colleges;

public class CollegeProfile : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Required, MaxLength(1024)]
    [Display(Name = "Bank Name")]
    public string BankName { get; set; } = string.Empty;

    [Required, MaxLength(1024)]
    [Display(Name = "Bank Branch Name")]
    public string BankBranchName { get; set; } = string.Empty;

    [Required, MaxLength(1024)]
    [Display(Name = "Bank Account Number")]
    public string BankAccountNumber { get; set; } = string.Empty;

    [Required, MaxLength(1024)]
    [Display(Name = "Contact Person Name")]
    public string ContactPersonName { get; set; } = string.Empty;

    [Required, MaxLength(1024)]
    [Display(Name = "Contact Person Mobile Number")]
    public string ContactPersonMobileNumber { get; set; } = string.Empty;

    [Required, MaxLength(1024)]
    [Display(Name = "Contact Person Email")]
    public string ContactPersonEmail { get; set; } = string.Empty;

    [Display(Name = "Status")]
    public int? Status { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "College")]
    public int CollegeId { get; set; }
    public virtual College? College { get; set; }

    public int BlankChequeUserAttachmentId { get; set; }
    public virtual UserAttachment? BlankChequeUserAttachment { get; set; }

    public int AuditReportUserAttachmentId { get; set; }
    public virtual UserAttachment? AuditReportUserAttachment { get; set; }
}
