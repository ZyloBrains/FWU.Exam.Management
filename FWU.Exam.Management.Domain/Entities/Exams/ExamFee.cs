using FWU.Exam.Management.Domain.Entities.Colleges;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class ExamFee
{
    public int Id { get; set; }

    [Required, MaxLength(400)]
    public string? Name { get; set; }

    public int ExamScheduleId { get; set; }

    public decimal Amount { get; set; }

    public int? CollegeTypeId { get; set; }

    public int? ExamTypeId { get; set; }

    public DateTime? ThroughDate { get; set; }

    public DateTime? ApplicableDate { get; set; }

    public bool IsCollegeFee { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }

    public virtual CollegeType? CollegeType { get; set; }

    public virtual ExamType? ExamType { get; set; }
}
