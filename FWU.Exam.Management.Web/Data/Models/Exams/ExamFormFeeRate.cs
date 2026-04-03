using fwu_examination_management_system.Data.Models.Colleges;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamFormFeeRate
{
    [Key]
    public int ExamFormFeeRateId { get; set; }

    public int ExamScheduleId { get; set; }
    public int ExamFormFeeNameId { get; set; }
    public decimal Amount { get; set; }
    public int? CollegeTypeId { get; set; }
    public int? ExamTypeId { get; set; }
    public DateTime? ThroughDate { get; set; }
    public DateTime? ApplicableDate { get; set; }
    public bool IsCollegeFee { get; set; }

    [ForeignKey(nameof(ExamScheduleId))]
    [ValidateNever]
    public virtual ExamSchedule ExamSchedule { get; set; }

    [ForeignKey(nameof(ExamFormFeeNameId))]
    [ValidateNever]
    public virtual ExamFormFeeName ExamFormFeeName { get; set; }

    [ForeignKey(nameof(CollegeTypeId))]
    [ValidateNever]
    public virtual CollegeType CollegeType { get; set; }

    [ForeignKey(nameof(ExamTypeId))]
    [ValidateNever]
    public virtual ExamType ExamType { get; set; }
}
