using FWU.Exam.Management.Domain.Entities.Exams;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Colleges;
public class CollegeType
{
    public int Id { get; set; }

    [Required, MaxLength(30)]
    [Display(Name = "Code")]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1024)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Default")]
    public bool IsDefault { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    public virtual ICollection<College> Colleges { get; set; } = [];
    public virtual ICollection<ExamFee> ExamFees { get; set; } = [];
}

