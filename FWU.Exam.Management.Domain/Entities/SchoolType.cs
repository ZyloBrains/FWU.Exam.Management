using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class SchoolType
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Previous Level")]
    public int PreviousLevelId { get; set; }

    [Required, MaxLength(255)]
    [Display(Name = "School Type Name")]
    public string? SchoolTypeName { get; set; }

    public virtual PreviousLevel? PreviousLevel { get; set; }
}
