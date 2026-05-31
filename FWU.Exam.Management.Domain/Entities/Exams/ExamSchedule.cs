using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamSchedule : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Required, MaxLength(50)]
    public string? ExamScheduleName { get; set; }

    [MaxLength(10)]
    public string? StartDateBs { get; set; }

    [MaxLength(10)]
    public string? EndDateBs { get; set; }

    public DateTime? PublishedDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public DateTime? ExtendedDate { get; set; }
    public decimal? ExtendedDateCharge { get; set; }
    public DateTime? CollegeApprovalDate { get; set; }
    public DateTime? AdmissionCardReleaseDate { get; set; }

    [MaxLength(50)]
    public string? ExamScheduleCode { get; set; }

    [Display(Name = "Academic Year")]
    public int AcademicYearId { get; set; }
    public virtual AcademicYear? AcademicYear { get; set; }

    [Display(Name = "Program")]
    public int ProgramId { get; set; }
    public virtual Program? Program { get; set; }

    [Display(Name = "Semester")]
    public int SemesterId { get; set; }
    public virtual Semester? Semester { get; set; }

    public int ExamTypeId { get; set; }
    public virtual ExamType? ExamType { get; set; }

    public int? LevelId { get; set; }
    public virtual Level? Level { get; set; }

    public virtual ICollection<ExamCenter>? ExamCenters { get; set; }
    public virtual ICollection<ExamRegistration>? ExamRegistrations { get; set; }
    public virtual ICollection<ExamSubjectResult>? ExamSubjectResults { get; set; }
    public virtual ICollection<ExamSlot>? ExamSlots { get; set; }
}
