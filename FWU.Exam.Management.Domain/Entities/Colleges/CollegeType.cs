using FWU.Exam.Management.Domain.Entities.Exams;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Colleges;
public class CollegeType
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string? Code { get; set; }

    [Required, MaxLength(50)]
    public string? Name { get; set; }

    [MaxLength(1024)]
    public string? Remarks { get; set; }

    public bool? IsDefault { get; set; }
    public bool IsActive { get; set; }

    public virtual ICollection<College>? Colleges { get; set; }
    public virtual ICollection<ExamFee>? ExamFees { get; set; }
}
