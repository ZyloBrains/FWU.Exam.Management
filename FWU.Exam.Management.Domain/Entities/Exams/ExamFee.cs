using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamFee : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Required, MaxLength(400)]
    [Display(Name = "Name")]
    public string? Name { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Exam Schedule")]
    public int ExamScheduleId { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Amount")]
    public decimal Amount { get; set; }

    [Display(Name = "College Type")]
    public int? CollegeTypeId { get; set; }

    [Display(Name = "Exam Type")]
    public int? ExamTypeId { get; set; }

    [Display(Name = "Through Date")]
    public DateTime? ThroughDate { get; set; }

    [Display(Name = "Applicable Date")]
    public DateTime? ApplicableDate { get; set; }

    [Display(Name = "Is College Fee")]
    public bool IsCollegeFee { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }

    public virtual CollegeType? CollegeType { get; set; }

    public virtual ExamType? ExamType { get; set; }
}
