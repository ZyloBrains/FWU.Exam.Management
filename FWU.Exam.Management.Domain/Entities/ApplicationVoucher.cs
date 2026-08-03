using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;
public class ApplicationVoucher : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Voucher Number")]
    public string VoucherNumber { get; set; } = string.Empty;

    [Required, MaxLength(1024)]
    [Display(Name = "Student Name")]
    public string StudentName { get; set; } = string.Empty;

    [Display(Name = "Date Of Birth Ad")]
    public DateOnly? DateOfBirthAd { get; set; }

    [MaxLength(50)]
    [Display(Name = "Date Of Birth Bs")]
    public string? DateOfBirthBs { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }

    [Display(Name = "Voucher Date")]
    public DateTime? VoucherDate { get; set; }
    public DateTime? Timestamp { get; set; }

    [Required, MaxLength(1024)]
    [Display(Name = "Contact Number")]
    public string ContactNumber { get; set; } = string.Empty;

    [MaxLength(1024)]
    [Display(Name = "Branch")]
    public string? Branch { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Exam Schedule Id")]
    public int ExamScheduleId { get; set; }
    public int? StudentRegistrationId { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }
    public virtual StudentRegistration? StudentRegistration { get; set; }
}
