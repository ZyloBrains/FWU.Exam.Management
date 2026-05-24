using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Colleges;

public class CollegeProfile : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Required, MaxLength(1024)]
    public string? BankName { get; set; }

    [Required, MaxLength(1024)]
    public string? BankBranchName { get; set; }

    [Required, MaxLength(1024)]
    public string? BankAccountNumber { get; set; }

    [Required, MaxLength(1024)]
    public string? ContactPersonName { get; set; }

    [Required, MaxLength(1024)]
    public string? ContactPersonMobileNumber { get; set; }

    [Required, MaxLength(1024)]
    public string? ContactPersonEmail { get; set; }

    public int? Status { get; set; }

    public int CollegeId { get; set; }
    public virtual College? College { get; set; }

    public int BlankChequeUserAttachmentId { get; set; }
    public virtual UserAttachment? BlankChequeUserAttachment { get; set; }

    public int AuditReportUserAttachmentId { get; set; }
    public virtual UserAttachment? AuditReportUserAttachment { get; set; }
}
