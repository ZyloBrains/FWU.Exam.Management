using fwu_examination_management_system.Data.Models.Colleges;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamCenterDetail
{
    public int Id { get; set; }

    public int ExamCenterId { get; set; }
    public int CollegeId { get; set; }
    public int? ProgramsId { get; set; }
    public long RollNumberFrom { get; set; }
    public long RollNumberTo { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public virtual ExamCenter? ExamCenter { get; set; }
    public virtual College? College { get; set; }
    public virtual Program? Program { get; set; }
}
