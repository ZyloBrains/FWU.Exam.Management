using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamSlot : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Exam Schedule")]
    public int ExamScheduleId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Subject Offering")]
    public int SubjectOfferingId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Batch")]
    public int BatchId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Exam Center")]
    public int ExamCenterId { get; set; }

    [MaxLength(10)]
    [Display(Name = "Exam Date")]
    public string? ExamDate { get; set; }

    [Display(Name = "Start Time")]
    public TimeOnly StartTime { get; set; }

    [Display(Name = "End Time")]
    public TimeOnly EndTime { get; set; }

    [MaxLength(50)]
    [Display(Name = "Room Number")]
    public string? RoomNumber { get; set; }

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }
    public virtual SubjectOffering? SubjectOffering { get; set; }
    public virtual Batch? Batch { get; set; }
    public virtual ExamCenter? ExamCenter { get; set; }
}
