using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;
public class ApplicationVoucher : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Required, MaxLength(50)]
    public string? VoucherNumber { get; set; }

    [Required, MaxLength(1024)]
    public string? StudentName { get; set; }

    public DateOnly? DateOfBirthAd { get; set; }

    [MaxLength(50)]
    public string? DateOfBirthBs { get; set; }

    public decimal Amount { get; set; }
    public DateTime? VoucherDate { get; set; }
    public DateTime? Timestamp { get; set; }

    [Required, MaxLength(1024)]
    public string? ContactNumber { get; set; }

    [MaxLength(1024)]
    public string? Branch { get; set; }

    public int ExamScheduleId { get; set; }
    public int? StudentRegistrationId { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }
    public virtual StudentRegistration? StudentRegistration { get; set; }
}
