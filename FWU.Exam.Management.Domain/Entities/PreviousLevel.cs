using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class PreviousLevel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Previous Level Name")]
    public string PreviousLevelName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    [Display(Name = "Level Id")]
    public int? LevelId { get; set; }

    [Display(Name = "Level Display Order")]
    public int? LevelDisplayOrder { get; set; }

    [MaxLength(1024)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    public virtual Level? Level { get; set; }
    public virtual ICollection<SchoolType> SchoolTypes { get; set; } = [];
    public virtual ICollection<StudentQualification> StudentQualifications { get; set; } = [];
}
