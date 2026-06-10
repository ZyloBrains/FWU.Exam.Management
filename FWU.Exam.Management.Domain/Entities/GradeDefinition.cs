using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class GradeDefinition
{
    public int Id { get; set; }

    [Required, MaxLength(10)]
    public string GradeLetter { get; set; } = string.Empty;

    public decimal MinPercentage { get; set; }
    public decimal MaxPercentage { get; set; }
    public decimal GradePoint { get; set; }

    [MaxLength(50)]
    public string? Remark { get; set; }

    public bool IsPass { get; set; } = true;
    public int DisplayOrder { get; set; }

    public int GradingSchemeId { get; set; }

    public virtual GradingScheme? GradingScheme { get; set; }
}
