using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Payments;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Infrastructure.Data.Models;

public class BankVoucher : IAuditable
{
    public int Id { get; set; }

    public int AcademicYearId { get; set; }
    public int CollegeId { get; set; }
    public int BillTitleId { get; set; }
    public int BankId { get; set; }

    [MaxLength(100)]
    public string? BankAddress { get; set; }

    public DateTime VoucherDate { get; set; }

    [MaxLength(50)]
    public string? VoucherNumber { get; set; }

    public decimal VoucherAmount { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public int? BankVoucherUserAttachmentId { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }
    public virtual College? College { get; set; }
    public virtual BillTitle? BillTitle { get; set; }
    public virtual Bank? Bank { get; set; }
    public virtual UserAttachment? BankVoucherAttachment { get; set; }
}
