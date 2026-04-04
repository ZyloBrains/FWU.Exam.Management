using fwu_examination_management_system.Data.Models.Subjects;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamScheduleDetail
{
    public int Id { get; set; }

    public int ExamScheduleId { get; set; }
    public int ExamTypeId { get; set; }
    public int SubjectDetailId { get; set; }
    public DateTime ExamDate { get; set; }

    [MaxLength(10)]
    public string? ExamDateBs { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey(nameof(ExamScheduleId))]
    [ValidateNever]
    public virtual ExamSchedule ExamSchedule { get; set; }

    [ForeignKey(nameof(ExamTypeId))]
    [ValidateNever]
    public virtual ExamType ExamType { get; set; }

    [ForeignKey(nameof(SubjectDetailId))]
    [ValidateNever]
    public virtual SubjectDetail SubjectDetail { get; set; }
}
