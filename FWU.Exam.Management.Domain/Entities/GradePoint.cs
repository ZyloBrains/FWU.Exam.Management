using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class GradePoint
{
    public int Id { get; set; }

    [Required, MaxLength(5)]
    [Display(Name = "Grade")]
    public string Grade { get; set; } = string.Empty;

    [Range(0, 100)]
    [Display(Name = "Obtained Mark")]
    public int ObtainedMark { get; set; }

    [Range(0, 10)]
    [Display(Name = "Grade Point")]
    public decimal GradePointValue { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Grade Group")]
    public int GradeGroupId { get; set; }

    public virtual GradeGroup? GradeGroup { get; set; }
}
