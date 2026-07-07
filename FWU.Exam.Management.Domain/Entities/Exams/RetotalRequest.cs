using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class RetotalRequest : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    public int ExamSubjectResultId { get; set; }
    public virtual ExamSubjectResult? ExamSubjectResult { get; set; }

    public int StudentRegistrationId { get; set; }
    public virtual StudentRegistration? StudentRegistration { get; set; }

    public int ExamRegistrationId { get; set; }
    public virtual ExamRegistration? ExamRegistration { get; set; }

    public DateTime RequestedDate { get; set; }
    public string? Reason { get; set; }
    public RetotalStatus Status { get; set; }

    public string? OriginalGradeLetter { get; set; }
    public float? OriginalObtainedMarks { get; set; }
    public string? RetotalledGradeLetter { get; set; }
    public float? RetotalledObtainedMarks { get; set; }

    public string? ReviewedByUsername { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public string? AdminRemarks { get; set; }

    public decimal? FeeAmount { get; set; }
    public bool FeePaid { get; set; }
    public bool IsActive { get; set; }
}
