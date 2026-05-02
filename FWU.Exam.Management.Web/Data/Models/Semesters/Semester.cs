using fwu_examination_management_system.Data.Models.Exams;
using fwu_examination_management_system.Data.Models.Subjects;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Semesters;

public class Semester
{
    public int Id { get; set; }

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
    public virtual ICollection<SemesterSubject>? SemesterSubjects { get; set; }
    public virtual ICollection<SubjectOffering>? SubjectOfferings { get; set; }
}
