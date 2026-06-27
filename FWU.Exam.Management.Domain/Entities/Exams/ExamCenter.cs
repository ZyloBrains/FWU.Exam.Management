using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamCenter : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Exam Schedule Id")]
    public int ExamScheduleId { get; set; }
    [Range(1, int.MaxValue)]
    [Display(Name = "College Id")]
    public int CollegeId { get; set; }

    [MaxLength(500)]
    [Display(Name = "Remark")]
    public string? Remark { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    [Required, MaxLength(30)]
    [Display(Name = "Code")]
    public string? Code { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }

    public virtual College? College { get; set; }
    public virtual ICollection<ExamRegistration>? ExamRegistrations { get; set; }
    public virtual ICollection<ExamSlot>? ExamSlots { get; set; }
}

