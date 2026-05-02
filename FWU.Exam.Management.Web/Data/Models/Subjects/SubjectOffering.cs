using fwu_examination_management_system.Data.Models.Exams;
using fwu_examination_management_system.Data.Semesters;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Subjects;

public class SubjectOffering
{
    public int Id { get; set; }

    public int SubjectCatalogId { get; set; }
    public int ProgramId { get; set; }
    public int SemesterId { get; set; }
    public int? SubjectGroupId { get; set; }
    public int AcademicYearId { get; set; }

    public bool IsCompulsory { get; set; }
    public int DisplayOrder { get; set; }

    public bool HasTheory { get; set; }
    public bool HasPractical { get; set; }
    public bool HasInternal { get; set; }

    public decimal TheoryFullMarks { get; set; }
    public decimal TheoryPassMarks { get; set; }
    public decimal? PracticalFullMarks { get; set; }
    public decimal? PracticalPassMarks { get; set; }
    public decimal? InternalTheoryFullMarks { get; set; }
    public decimal? InternalTheoryPassMarks { get; set; }
    public decimal? InternalPracticalFullMarks { get; set; }
    public decimal? InternalPracticalPassMarks { get; set; }

    public virtual SubjectCatalog? SubjectCatalog { get; set; }
    public virtual Program? Program { get; set; }
    public virtual Semester? Semester { get; set; }
    public virtual SubjectGroup? SubjectGroup { get; set; }
    public virtual AcademicYear? AcademicYear { get; set; }

    public virtual ICollection<ExamSubjectRegistration>? ExamSubjectRegistrations { get; set; }
    public virtual ICollection<ExamSubjectRegistrationInternal>? ExamSubjectRegistrationInternals { get; set; }
    public virtual ICollection<ExamScheduleDetail>? ExamScheduleDetails { get; set; }
    public virtual ICollection<ResultRecord>? ResultRecords { get; set; }
}
