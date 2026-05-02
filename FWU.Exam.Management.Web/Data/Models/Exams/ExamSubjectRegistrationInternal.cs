using fwu_examination_management_system.Data.Models.Students;
using fwu_examination_management_system.Data.Models.Subjects;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamSubjectRegistrationInternal
{
    public int Id { get; set; }

    public int EntryAcademicYearId { get; set; }
    public int StudentProgramYearPartId { get; set; }
    public decimal? ObtainedMarksTheoryInternal { get; set; }
    public decimal? ObtainedMarksPracticalInternal { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }
 
    public int? ExamScheduleId { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }
    public virtual StudentProgramYearPart? StudentProgramYearPart { get; set; }
    public virtual ExamSchedule? ExamSchedule { get; set; }
}
