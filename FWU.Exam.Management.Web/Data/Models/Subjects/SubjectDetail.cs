using fwu_examination_management_system.Data.Models.Exams;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Subjects;


public class SubjectDetail
{
    public int Id { get; set; }

    public int ProgramsId { get; set; }

    [Required, MaxLength(50)]
    public string? SubjectCode { get; set; }

    [Required, MaxLength(100)]
    public string? SubjectName { get; set; }

    public decimal TheoryFullMarks { get; set; }
    public decimal TheoryPassMarks { get; set; }
    public decimal? PracticalFullMarks { get; set; }
    public decimal? PracticalPassMarks { get; set; }
    public decimal? InternalTheoryFullMarks { get; set; }
    public decimal? InternalTheoryPassMarks { get; set; }
    public decimal? InternalPracticalFullMarks { get; set; }
    public decimal? InternalPracticalPassMarks { get; set; }
    public int? CreditHours { get; set; }
    public bool HasPractical { get; set; }
    public bool HasInternal { get; set; }
    public int? DisplayOrder { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public bool IsCompulsory { get; set; }

    [MaxLength(50)]
    public string? ShortName { get; set; }

    [MaxLength(50)]
    public string? ConcurrentSubjectCode { get; set; }

    public int SubjectTypeId { get; set; }
    public bool HasTheory { get; set; }

    [MaxLength(50)]
    public string? Year { get; set; }

    [MaxLength(50)]
    public string? Part { get; set; }

    public virtual Program? Program { get; set; }
    public virtual SubjectType? SubjectType { get; set; }
    public virtual ICollection<ExamScheduleDetail>? ExamScheduleDetails { get; set; }
    public virtual ICollection<ExamSubjectRegistration>? ExamSubjectRegistrations { get; set; }
    public virtual ICollection<ExamSubjectRegistrationInternal>? ExamSubjectRegistrationInternals { get; set; }
    public virtual ICollection<ResultRecord>? ResultRecords { get; set; }
}
