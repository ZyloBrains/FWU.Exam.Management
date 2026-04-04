using fwu_examination_management_system.Data.Models.Exams;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Colleges;
public class CollegeType
{
    public int Id { get; set; }

    [Required, MaxLength(2)]
    public string Code { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; }

    [MaxLength(1024)]
    public string? Remarks { get; set; }

    public bool? IsDefault { get; set; }
    public bool IsActive { get; set; }

    public virtual ICollection<College>? Colleges { get; set; }
    public virtual ICollection<ExamFormFeeRate>? ExamFormFeeRates { get; set; }
}
