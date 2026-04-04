using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamType
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string? Name { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public int Code { get; set; }
    public virtual ICollection<ExamFormFeeRate>? ExamFormFeeRates { get; set; }
    public virtual ICollection<ExamSchedule>? ExamSchedules { get; set; }
    public virtual ICollection<ExamScheduleBatch>? ExamScheduleBatches { get; set; }
    public virtual ICollection<ExamScheduleDetail>? ExamScheduleDetails { get; set; }
    public virtual ICollection<ExamSubjectRegistration>? ExamSubjectRegistrations { get; set; }
}
