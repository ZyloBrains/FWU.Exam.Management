using fwu_examination_management_system.Data.Models.Colleges;
using fwu_examination_management_system.Data.Models.Exams;
using fwu_examination_management_system.Data.Models.Students;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Payments;

public class PaymentRequestLog
{
    public int Id { get; set; }
    public int? PaymentRequestLogStatus { get; set; }

    [Required, MaxLength(50)]
    public string? InvoiceNumber { get; set; }

    public DateTime ForwardedTimestamp { get; set; }
    public DateTime? DateOfBirthAd { get; set; }

    [MaxLength(20)]
    public string? MobileNumber { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [Required, MaxLength(255)]
    public string? FullName { get; set; }

    public decimal Amount { get; set; }

    [Required]
    public string? FullRequestContent { get; set; }

    public int PaymentTypeId { get; set; }
    public int? StudentRegistrationId { get; set; }
    public int ExamScheduleId { get; set; }

    [MaxLength(50)]
    public string? TransactionId { get; set; }

    public int? CollegeId { get; set; }
    public int StudentCount { get; set; }

    public virtual PaymentType? PaymentType { get; set; }

    public virtual StudentRegistration? StudentRegistration { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }

    public virtual College? College { get; set; }
    public virtual ICollection<PaymentResponseLog>? PaymentResponseLog { get; set; }
    public virtual ICollection<PaymentPracticalSubjects>? PaymentPracticalSubjects { get; set; }
}
