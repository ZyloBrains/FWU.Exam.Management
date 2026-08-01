using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Subjects;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Semesters;

public class Semester
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Number")]
    public int Number { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Year")]
    public int Year { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Name")]
    public string? Name { get; set; }

    [Required, MaxLength(30)]
    [Display(Name = "Code")]
    public string? Code { get; set; }

    [MaxLength(50)]
    [Display(Name = "Remark")]
    public string? Remark { get; set; }

    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Academic Year")]
    public int AcademicYearId { get; set; }
    public virtual AcademicYear? AcademicYear { get; set; }

    [Display(Name = "Faculty")]
    public int? FacultyId { get; set; }
    public virtual Faculty? Faculty { get; set; }

    public virtual ICollection<ExamSchedule>? ExamSchedules { get; set; }
    public virtual ICollection<SemesterEnrollment>? SemesterEnrollments { get; set; }
    public virtual ICollection<SubjectOffering>? SubjectOfferings { get; set; }
    public virtual ICollection<ProgramSemester>? ProgramSemesters { get; set; }
}

