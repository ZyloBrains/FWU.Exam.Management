using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Semesters;

public class SemesterInstance : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Semester")]
    public int SemesterId { get; set; }
    public virtual Semester? Semester { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Academic Year")]
    public int AcademicYearId { get; set; }
    public virtual AcademicYear? AcademicYear { get; set; }

    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }

    [MaxLength(50)]
    [Display(Name = "Remark")]
    public string? Remark { get; set; }

    public virtual ICollection<SemesterEnrollment> SemesterEnrollments { get; set; } = [];
}
