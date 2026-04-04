using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamScheduleBatch
{
    public int Id { get; set; }

    public int ExamScheduleId { get; set; }
    public int ExamTypeId { get; set; }
    public int BatchId { get; set; }

    [ForeignKey(nameof(ExamScheduleId))]
    [ValidateNever]
    public virtual ExamSchedule ExamSchedule { get; set; }

    [ForeignKey(nameof(ExamTypeId))]
    [ValidateNever]
    public virtual ExamType ExamType { get; set; }

    [ForeignKey(nameof(BatchId))]
    [ValidateNever]
    public virtual Batch Batch { get; set; }
}
