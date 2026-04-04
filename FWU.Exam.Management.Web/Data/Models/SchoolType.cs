using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class SchoolType
{
    public int Id { get; set; }

    public int PreviousLevelId { get; set; }

    [Required, MaxLength(255)]
    public string? SchoolTypeName { get; set; }

    public virtual PreviousLevel? PreviousLevel { get; set; }
}
