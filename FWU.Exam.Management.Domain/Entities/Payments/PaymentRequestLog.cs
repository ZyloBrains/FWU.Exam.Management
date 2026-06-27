using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Payments;

public class PaymentRequestLog : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Display(Name = "Payment Status")]
    public int? PaymentRequestLogStatus { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Invoice Number")]
    public string? InvoiceNumber { get; set; }

    [Display(Name = "Forwarded Timestamp")]
    public DateTime ForwardedTimestamp { get; set; }

    [Display(Name = "Date of Birth (AD)")]
    public DateTime? DateOfBirthAd { get; set; }

    [MaxLength(20)]
    [Display(Name = "Mobile Number")]
    public string? MobileNumber { get; set; }

    [MaxLength(100)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required, MaxLength(255)]
    [Display(Name = "Full Name")]
    public string? FullName { get; set; }

    [Display(Name = "Amount")]
    public decimal Amount { get; set; }

    [Required]
    [Display(Name = "Full Request Content")]
    public string? FullRequestContent { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Payment Type")]
    public int PaymentTypeId { get; set; }

    [Display(Name = "Student Registration")]
    public int? StudentRegistrationId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Exam Schedule")]
    public int ExamScheduleId { get; set; }

    [MaxLength(50)]
    [Display(Name = "Transaction ID")]
    public string? TransactionId { get; set; }

    [Display(Name = "College")]
    public int? CollegeId { get; set; }

    [Display(Name = "Student Count")]
    public int StudentCount { get; set; }

    [MaxLength(1000)]
    [Display(Name = "Selected Subjects")]
    public string? SelectedSubjectIds { get; set; }

    public virtual PaymentType? PaymentType { get; set; }

    public virtual StudentRegistration? StudentRegistration { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }

    public virtual College? College { get; set; }
    public virtual ICollection<PaymentResponseLog>? PaymentResponseLog { get; set; }
    public virtual ICollection<PaymentPracticalSubjects>? PaymentPracticalSubjects { get; set; }
}
