using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamRollNumberSetup : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Exam Schedule Id")]
    public int ExamScheduleId { get; set; }
    [Range(1, int.MaxValue)]
    [Display(Name = "First Exam Roll Number")]
    public int FirstExamRollNumber { get; set; }

    [MaxLength(50)]
    [Display(Name = "Prefix")]
    public string? Prefix { get; set; }

    [MaxLength(50)]
    [Display(Name = "Suffix")]
    public string? Suffix { get; set; }

    [MaxLength(4000)]
    [Display(Name = "Details Json")]
    public string? DetailsJson { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Minimum Roll Number Length")]
    public int MinimumRollNumberLength { get; set; }
    [Range(1, int.MaxValue)]
    [Display(Name = "Round")]
    public int Round { get; set; }
    [Range(0, int.MaxValue)]
    [Display(Name = "Minimum Gap")]
    public int MinimumGap { get; set; }
    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }
}
