using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamSchedule : ITenantScoped
{
    public int Id { get; set; }

    [Display(Name = "Tenant")]
    public int TenantId { get; set; }

    public virtual Tenant? Tenant { get; set; }

    [Display(Name = "College")]
    public int? CollegeId { get; set; }
    public virtual College? College { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Exam Schedule Name")]
    public string ExamScheduleName { get; set; } = string.Empty;

    [MaxLength(10)]
    [Display(Name = "Start Date (BS)")]
    public string? StartDateBs { get; set; }

    [MaxLength(10)]
    [Display(Name = "End Date (BS)")]
    public string? EndDateBs { get; set; }

    [Display(Name = "Start Date (AD)")]
    public DateOnly? StartDate { get; set; }

    [Display(Name = "End Date (AD)")]
    public DateOnly? EndDate { get; set; }

    [Display(Name = "Published Date")]
    public DateTime? PublishedDate { get; set; }

    [Display(Name = "Start Time")]
    public TimeOnly StartTime { get; set; }

    [Display(Name = "End Time")]
    public TimeOnly EndTime { get; set; }

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    [Display(Name = "Extended Date")]
    public DateTime? ExtendedDate { get; set; }

    [Display(Name = "Extended Date Charge")]
    public decimal? ExtendedDateCharge { get; set; }

    [Display(Name = "Exam Fee")]
    public decimal? ExamFee { get; set; }

    [Display(Name = "Practical Subject Fee")]
    public decimal? PracticalSubjectFee { get; set; }

    [Display(Name = "Admission Card Release Date")]
    public DateTime? AdmissionCardReleaseDate { get; set; }

    [MaxLength(50)]
    [Display(Name = "Exam Schedule Code")]
    public string? ExamScheduleCode { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Program")]
    public int ProgramId { get; set; }

    public virtual Program? Program { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Semester")]
    public int SemesterInstanceId { get; set; }

    public virtual SemesterInstance? SemesterInstance { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Exam Type")]
    public int ExamTypeId { get; set; }

    public virtual ExamType? ExamType { get; set; }

    [Display(Name = "Level")]
    public int? LevelId { get; set; }
    public virtual Level? Level { get; set; }

    public virtual ICollection<ExamCenter> ExamCenters { get; set; } = [];
    public virtual ICollection<ExamRegistration> ExamRegistrations { get; set; } = [];
    public virtual ICollection<ExamSubjectResult> ExamSubjectResults { get; set; } = [];
    public virtual ICollection<ExamSlot> ExamSlots { get; set; } = [];
}
