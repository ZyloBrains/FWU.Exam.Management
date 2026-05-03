using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Program
{
    public int Id { get; set; }

    public int LevelId { get; set; }
    public int FacultyId { get; set; }
    public int? BoardId { get; set; }

    [Required, MaxLength(50)]
    public string? ProgramCode { get; set; }

    [Required, MaxLength(255)]
    public string? ProgramName { get; set; }

    [Required, MaxLength(50)]
    public string? ShortName { get; set; }

    public int Duration { get; set; }
    public int? GrandTotalMarks { get; set; }
    public bool HasMultipleIntakes { get; set; }

    [MaxLength(50)]
    public string? NumberOfSeats { get; set; }

    public int? ScholarshipSeats { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    [MaxLength(10)]
    public string? RollNumberPrefix { get; set; }

    public virtual Level? Level { get; set; }

    public virtual Faculty? Faculty { get; set; }

    public virtual Board? Board { get; set; }

    public virtual ICollection<CollegeProgram>? CollegePrograms { get; set; }
    public virtual ICollection<ExamRegistration>? ExamRegistrations { get; set; }
    public virtual ICollection<StudentAdmission>? StudentAdmissions { get; set; }
}
