using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamSlot : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    public int ExamScheduleId { get; set; }
    public int SubjectOfferingId { get; set; }
    public int BatchId { get; set; }
    public int ExamCenterId { get; set; }

    [MaxLength(10)]
    public string? ExamDate { get; set; }

    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    [MaxLength(50)]
    public string? RoomNumber { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }
    public virtual SubjectOffering? SubjectOffering { get; set; }
    public virtual Batch? Batch { get; set; }
    public virtual ExamCenter? ExamCenter { get; set; }
}
