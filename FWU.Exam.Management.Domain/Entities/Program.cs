using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Program
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Level Id")]
    public int LevelId { get; set; }

    [Display(Name = "Board Id")]
    public int? BoardId { get; set; }

    [Display(Name = "Faculty")]
    public int? FacultyId { get; set; }
    public virtual Faculty? Faculty { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Program Code")]
    public string ProgramCode { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    [Display(Name = "Program Name")]
    public string ProgramName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [Display(Name = "Short Name")]
    public string ShortName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    [Display(Name = "Duration")]
    public int Duration { get; set; }

    [Display(Name = "Grand Total Marks")]
    public int? GrandTotalMarks { get; set; }
    public bool HasMultipleIntakes { get; set; }

    [MaxLength(50)]
    [Display(Name = "Number Of Seats")]
    public string? NumberOfSeats { get; set; }

    [Display(Name = "Scholarship Seats")]
    public int? ScholarshipSeats { get; set; }

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    [MaxLength(10)]
    [Display(Name = "Roll Number Prefix")]
    public string? RollNumberPrefix { get; set; }

    public virtual Level? Level { get; set; }

    public virtual Board? Board { get; set; }

    public virtual ICollection<CollegeProgram> CollegePrograms { get; set; } = [];
    public virtual ICollection<ExamRegistration> ExamRegistrations { get; set; } = [];
    public virtual ICollection<StudentAdmission> StudentAdmissions { get; set; } = [];
    public virtual ICollection<ProgramSemester> ProgramSemesters { get; set; } = [];
}
