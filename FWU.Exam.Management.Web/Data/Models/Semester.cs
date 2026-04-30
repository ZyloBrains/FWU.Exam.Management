using fwu_examination_management_system.Data.Models.Exams;
using fwu_examination_management_system.Data.Models.Students;
using fwu_examination_management_system.Data.Models.Subjects;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class Semester
{
    public int Id { get; set; }

    public int AcademicYearId { get; set; }
    public int SemesterNumber { get; set; }

    [Required, MaxLength(50)]
    public string? SemesterName { get; set; }

    [MaxLength(50)]
    public string? SemesterNameNepali { get; set; }

    [MaxLength(50)]
    public string? Remark { get; set; }

    public bool IsActive { get; set; }

    [MaxLength(50)]
    public string? Code { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }

    public virtual ICollection<ExamSchedule>? ExamSchedules { get; set; }
    public virtual ICollection<ProgramSemester>? ProgramSemesters { get; set; }
    public virtual ICollection<StudentProgramSemester>? StudentProgramSemesters { get; set; }
    public virtual ICollection<SubjectDetail>? SubjectDetails { get; set; }
    public virtual ICollection<SubjectGroup>? SubjectGroups { get; set; }
    public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
