using System.ComponentModel.DataAnnotations.Schema;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Interfaces;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class AdmitCard : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    public int ExamRegistrationId { get; set; }
    public virtual ExamRegistration? ExamRegistration { get; set; }

    public int ExamScheduleId { get; set; }
    public virtual ExamSchedule? ExamSchedule { get; set; }

    public int? StudentRegistrationId { get; set; }
    public virtual StudentRegistration? StudentRegistration { get; set; }

    public string? AdmitCardNumber { get; set; }
    public string? ExamRollNo { get; set; }
    public string? Campus { get; set; }
    public string? Level { get; set; }
    public string? Program { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Semester { get; set; }
    public string? ExamType { get; set; }
    public string? Year { get; set; }
    public string? PhotoPath { get; set; }
    public string? SignaturePath { get; set; }
    public string? ControllerSignaturePath { get; set; }
    public DateTime GeneratedDate { get; set; }
    public bool IsDownloaded { get; set; }
    public DateTime? DownloadedDate { get; set; }
    public bool IsActive { get; set; }

    [NotMapped]
    public List<Subject>? Subjects { get; set; }
}

public class Subject
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public bool Theory { get; set; }
    public bool Practical { get; set; }
    public string? Remarks { get; set; }
}
