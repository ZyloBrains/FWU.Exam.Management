using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Semesters;

public class Semester : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    public int Number { get; set; }
    public int Year { get; set; }

    [Required, MaxLength(50)]
    public string? Name { get; set; }

    [Required, MaxLength(50)]
    public string? Code { get; set; }

    [MaxLength(50)]
    public string? Remark { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int AcademicYearId { get; set; }
    public virtual AcademicYear? AcademicYear { get; set; }

    public virtual ICollection<ExamSchedule>? ExamSchedules { get; set; }
    public virtual ICollection<SemesterEnrollment>? SemesterEnrollments { get; set; }
    public virtual ICollection<SubjectOffering>? SubjectOfferings { get; set; }
}
