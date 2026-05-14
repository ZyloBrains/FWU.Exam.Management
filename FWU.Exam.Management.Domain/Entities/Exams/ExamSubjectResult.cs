using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamSubjectResult : IAuditable
{
    public int Id { get; set; }

    public int ExamRegistrationId { get; set; }
    public int ExamTypeId { get; set; }
    public int SubjectOfferingId { get; set; }
    public int? ExamScheduleId { get; set; }

    [MaxLength(3)]
    public string? ObtainedMarksTheory { get; set; }

    [MaxLength(3)]
    public string? ObtainedMarksTheoryConfirm { get; set; }

    [MaxLength(3)]
    public string? ObtainedMarksPractical { get; set; }

    [MaxLength(3)]
    public string? ObtainedMarksPracticalConfirm { get; set; }

    public decimal? ObtainedMarksTheoryInternal { get; set; }
    public decimal? ObtainedMarksPracticalInternal { get; set; }

    [MaxLength(3)]
    public string? GradeLetter { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }
    public bool? IsLooseEntry { get; set; }
    public bool? IsTheoryRegistered { get; set; }
    public bool? IsPracticalRegistered { get; set; }
    public bool? IsExtra { get; set; }

    public DateTime? ExamStartedDateTime { get; set; }
    public bool IsSubmitted { get; set; }
    public decimal? ObtainedMarks { get; set; }
    public DateTime? ExamSubmittedDateTime { get; set; }
    public bool? IsAutoSubmitted { get; set; }
    public DateTime? LastStatusSyncDateTime { get; set; }

    public virtual ExamRegistration? ExamRegistration { get; set; }
    public virtual ExamType? ExamType { get; set; }
    public virtual SubjectOffering? SubjectOffering { get; set; }
    public virtual ExamSchedule? ExamSchedule { get; set; }
}
