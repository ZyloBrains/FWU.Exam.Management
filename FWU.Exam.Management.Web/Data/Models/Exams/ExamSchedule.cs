using fwu_examination_management_system.Data.Models.Payments;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamSchedule
{
    public int Id { get; set; }
    public int AcademicYearId { get; set; }
    public int LevelId { get; set; }
    public int ExamTypeId { get; set; }

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

    public int? ExamScheduleParentId { get; set; }
    public DateTime? ExtendedDate { get; set; }
    public decimal? ExtendedDateCharge { get; set; }
    public DateTime? CollegeApprovalDate { get; set; }
    public DateTime? AdmissionCardReleaseDate { get; set; }

    [MaxLength(50)]
    public string? ExamScheduleCode { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }
    public virtual Level? Level { get; set; }
    public virtual ExamType? ExamType { get; set; }
    public virtual ICollection<ExamCenter>? ExamCenters { get; set; }
    public virtual ICollection<ExamRegistration>? ExamRegistrations { get; set; }
}
