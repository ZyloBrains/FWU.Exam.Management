using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class GradeGroup
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Grade Group Name")]
    public string GradeGroupName { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [MaxLength(100)]
    [Display(Name = "Created By")]
    public string? CreatedBy { get; set; }

    [Display(Name = "Created Date")]
    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<GradePoint> GradePoints { get; set; } = [];
}
