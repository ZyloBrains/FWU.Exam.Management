using fwu_examination_management_system.Data.Models.Exams;
using fwu_examination_management_system.Data.Models.Students;
using fwu_examination_management_system.Data.Models.Subjects;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class YearPart
{
    public int Id { get; set; }

    public int ProgramPeriodTypeId { get; set; }
    public int Year { get; set; }
    public int Part { get; set; }

    [Required, MaxLength(50)]
    public string? YearPartName { get; set; }

    [MaxLength(50)]
    public string? Remark { get; set; }

    public bool IsActive { get; set; }
    public bool IsEditable { get; set; }

    [MaxLength(50)]
    public string? Code { get; set; }

    public virtual ProgramPeriodType? ProgramPeriodType { get; set; }

    public virtual ICollection<ExamSchedule>? ExamSchedules { get; set; }
    public virtual ICollection<ProgramYearPart>? ProgramYearParts { get; set; }
    public virtual ICollection<StudentProgramYearPart>? StudentProgramYearParts { get; set; }
    public virtual ICollection<SubjectDetail>? SubjectDetails { get; set; }
    public virtual ICollection<SubjectGroup>? SubjectGroups { get; set; }
}
