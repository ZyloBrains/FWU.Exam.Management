using fwu_examination_management_system.Data.Models.Students;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models;

public class PreviousLevel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string PreviousLevelName { get; set; }

    public int? LevelId { get; set; }
    public int? LevelDisplayOrder { get; set; }

    [MaxLength(1024)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey(nameof(LevelId))]
    [ValidateNever]
    public virtual Level Level { get; set; }
    [ValidateNever]
    public virtual ICollection<SchoolType> SchoolTypes { get; set; }
    [ValidateNever]
    public virtual ICollection<StudentQualification> StudentQualifications { get; set; }
}
