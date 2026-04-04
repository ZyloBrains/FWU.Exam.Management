using fwu_examination_management_system.Data.Models.Colleges;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamFormFeeRate
{
    public int Id { get; set; }

    public int ExamScheduleId { get; set; }
    public int ExamFormFeeNameId { get; set; }
    public decimal Amount { get; set; }
    public int? CollegeTypeId { get; set; }
    public int? ExamTypeId { get; set; }
    public DateTime? ThroughDate { get; set; }
    public DateTime? ApplicableDate { get; set; }
    public bool IsCollegeFee { get; set; }

    public virtual ExamSchedule? ExamSchedule { get; set; }

    public virtual ExamFormFeeName? ExamFormFeeName { get; set; }

    public virtual CollegeType? CollegeType { get; set; }

    public virtual ExamType? ExamType { get; set; }
}
