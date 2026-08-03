using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities.Payments;

public class BillTitle : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Required, MaxLength(255)]
    [Display(Name = "Bill Title Name")]
    public string BillTitleName { get; set; } = string.Empty;

    [MaxLength(1024)]
    [Display(Name = "Category")]
    public string? Category { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    [Display(Name = "Amount")]
    public decimal? Amount { get; set; }

    [Display(Name = "Through Date")]
    public DateTime? ThroughDate { get; set; }

    [Display(Name = "Applicable Date")]
    public DateTime? ApplicableDate { get; set; }

    [Display(Name = "Exam Schedule")]
    public int? ExamScheduleId { get; set; }

    [Display(Name = "Practical Fee")]
    public decimal? PracticalFee { get; set; }

    [Display(Name = "Program")]
    public int? ProgramsId { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }
    public virtual Program? Program { get; set; }
}
