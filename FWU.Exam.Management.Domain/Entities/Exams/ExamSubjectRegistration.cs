using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamSubjectRegistration : IAuditable
{
    public int Id { get; set; }

    public int ExamRegistrationId { get; set; }
    public int ExamTypeId { get; set; }
    public int SubjectOfferingId { get; set; }

    [MaxLength(3)]
    public string? ObtainedMarksTheory { get; set; }

    [MaxLength(3)]
    public string? ObtainedMarksTheoryConfirm { get; set; }

    [MaxLength(3)]
    public string? ObtainedMarksPractical { get; set; }

    [MaxLength(3)]
    public string? ObtainedMarksPracticalConfirm { get; set; }

    [MaxLength(3)]
    public string? GradeLetter { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }
    public bool? IsLooseEntry { get; set; }
    public bool? IsTheoryRegistered { get; set; }
    public bool? IsPracticalRegistered { get; set; }
    public bool? IsExtra { get; set; }
    
    public virtual ExamRegistration? ExamRegistration { get; set; }

    public virtual ExamType? ExamType { get; set; }
    public virtual SubjectOffering? SubjectOffering { get; set; }
    public virtual ExamSubjectRegistrationExamSession? ExamSubjectRegistrationExamSession { get; set; }
}
