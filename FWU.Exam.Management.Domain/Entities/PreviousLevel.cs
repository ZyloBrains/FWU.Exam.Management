using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class PreviousLevel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string? PreviousLevelName { get; set; }

    public int? LevelId { get; set; }
    public int? LevelDisplayOrder { get; set; }

    [MaxLength(1024)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public virtual Level? Level { get; set; }
    public virtual ICollection<SchoolType>? SchoolTypes { get; set; }
    public virtual ICollection<StudentQualification>? StudentQualifications { get; set; }
}
