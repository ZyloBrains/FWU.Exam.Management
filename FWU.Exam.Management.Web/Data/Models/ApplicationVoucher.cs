using fwu_examination_management_system.Data.Models.Exams;
using fwu_examination_management_system.Data.Models.Students;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;
public class ApplicationVoucher
{
    public int Id { get; set; }

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
