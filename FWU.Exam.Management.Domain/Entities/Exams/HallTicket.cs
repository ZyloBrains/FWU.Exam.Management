using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Interfaces;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class HallTicket : ITenantScoped
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

    public string? HallTicketNumber { get; set; }
    public DateTime GeneratedDate { get; set; }
    public bool IsDownloaded { get; set; }
    public DateTime? DownloadedDate { get; set; }
    public bool IsActive { get; set; }
}
