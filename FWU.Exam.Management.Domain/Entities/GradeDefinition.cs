using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class GradeDefinition
{
    public int Id { get; set; }

    [Required, MaxLength(10)]
    [Display(Name = "Grade Letter")]
    public string GradeLetter { get; set; } = string.Empty;

    [Range(0, 100)]
    [Display(Name = "Min Percentage")]
    public decimal MinPercentage { get; set; }

    [Range(0, 100)]
    [Display(Name = "Max Percentage")]
    public decimal MaxPercentage { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Grade Point")]
    public decimal GradePoint { get; set; }

    [MaxLength(50)]
    [Display(Name = "Remark")]
    public string? Remark { get; set; }

    [Display(Name = "Is Pass")]
    public bool IsPass { get; set; } = true;

    [Range(1, int.MaxValue)]
    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Grading Scheme")]
    public int GradingSchemeId { get; set; }

    public virtual GradingScheme? GradingScheme { get; set; }
}
