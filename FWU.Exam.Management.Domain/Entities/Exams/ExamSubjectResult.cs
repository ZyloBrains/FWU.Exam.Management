using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamSubjectResult : IAuditable, ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Exam Registration")]
    public int ExamRegistrationId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Exam Type")]
    public int ExamTypeId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Subject Offering")]
    public int SubjectOfferingId { get; set; }

    [Display(Name = "Exam Schedule")]
    public int? ExamScheduleId { get; set; }

    [Range(0, float.MaxValue)]
    [Display(Name = "Obtained Marks Theory")]
    public float? ObtainedMarksTheory { get; set; }

    [Range(0, float.MaxValue)]
    [Display(Name = "Obtained Marks Theory Confirm")]
    public float? ObtainedMarksTheoryConfirm { get; set; }

    [Range(0, float.MaxValue)]
    [Display(Name = "Obtained Marks Practical")]
    public float? ObtainedMarksPractical { get; set; }

    [Range(0, float.MaxValue)]
    [Display(Name = "Obtained Marks Practical Confirm")]
    public float? ObtainedMarksPracticalConfirm { get; set; }

    [Range(0, float.MaxValue)]
    [Display(Name = "Obtained Marks Theory Internal")]
    public float? ObtainedMarksTheoryInternal { get; set; }

    [Range(0, float.MaxValue)]
    [Display(Name = "Obtained Marks Practical Internal")]
    public float? ObtainedMarksPracticalInternal { get; set; }

    [MaxLength(3)]
    [Display(Name = "Grade Letter")]
    public string? GradeLetter { get; set; }

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    [Display(Name = "Is Loose Entry")]
    public bool? IsLooseEntry { get; set; }

    [Display(Name = "Is Theory Registered")]
    public bool? IsTheoryRegistered { get; set; }

    [Display(Name = "Is Practical Registered")]
    public bool? IsPracticalRegistered { get; set; }

    [Display(Name = "Is Extra")]
    public bool? IsExtra { get; set; }

    public DateTime? ExamStartedDateTime { get; set; }

    [Display(Name = "Is Submitted")]
    public bool IsSubmitted { get; set; }

    [Range(0, float.MaxValue)]
    [Display(Name = "Obtained Marks")]
    public float? ObtainedMarks { get; set; }

    public DateTime? ExamSubmittedDateTime { get; set; }

    [Display(Name = "Is Auto Submitted")]
    public bool? IsAutoSubmitted { get; set; }

    public DateTime? LastStatusSyncDateTime { get; set; }

    public virtual ExamRegistration? ExamRegistration { get; set; }
    public virtual ExamType? ExamType { get; set; }
    public virtual SubjectOffering? SubjectOffering { get; set; }
    public virtual ExamSchedule? ExamSchedule { get; set; }
}
