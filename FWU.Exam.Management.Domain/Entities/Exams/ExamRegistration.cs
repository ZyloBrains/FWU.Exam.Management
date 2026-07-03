using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamRegistration : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Academic Year")]
    public int AcademicYearId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Exam Center")]
    public int? ExamCenterId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "College")]
    public int CollegeId { get; set; }

    [MaxLength(20)]
    [Display(Name = "Exam Roll Number")]
    public string? ExamRollNumber { get; set; }

    [Display(Name = "Exam Roll Number Coding")]
    public long? ExamRollNumberCoding { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Fee Enclosed")]
    public decimal? FeeEnclosed { get; set; }

    [Range(0, 100)]
    [Display(Name = "Attendance Percentage")]
    public decimal? AttendancePercentage { get; set; }

    [Display(Name = "Registration Date")]
    public DateTime? RegistrationDate { get; set; }

    [Display(Name = "Status")]
    public RegistrationStatus Status { get; set; }

    [MaxLength(100)]
    [Display(Name = "Verified By Username")]
    public string? VerifiedByUsername { get; set; }

    [Display(Name = "Verified Date")]
    public DateTime? VerifiedDate { get; set; }

    [MaxLength(50)]
    [Display(Name = "SGPA")]
    public string? Sgpa { get; set; }

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Exam Schedule")]
    public int ExamScheduleId { get; set; }

    [Display(Name = "Roll Number Index")]
    public int? RollNumberIndex { get; set; }

    [Display(Name = "Is Applied By Student")]
    public bool? IsAppliedByStudent { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Program")]
    public int? ProgramsId { get; set; }

    [Display(Name = "Application Voucher")]
    public int? ApplicationVoucherId { get; set; }

    [MaxLength(100)]
    [Display(Name = "Admin Verified By Username")]
    public string? AdminVerifiedByUsername { get; set; }

    [MaxLength(50)]
    [Display(Name = "Symbol Number")]
    public string? SymbolNumber { get; set; }

    [Display(Name = "Admin Verified Date")]
    public DateTime? AdminVerifiedDate { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }

    public virtual ExamCenter? ExamCenter { get; set; }

    public virtual College? College { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }

    public virtual Program? Program { get; set; }

    public virtual ApplicationVoucher? ApplicationVoucher { get; set; }

    public virtual ICollection<ExamSubjectResult>? ExamSubjectResults { get; set; }
}
