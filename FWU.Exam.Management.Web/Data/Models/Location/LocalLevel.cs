using fwu_examination_management_system.Data.Enums;
using fwu_examination_management_system.Data.Models.Students;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Location;

public class LocalLevel
{
    public int Id { get; set; }

    public int DistrictId { get; set; }

    [Required, MaxLength(100)]
    public string? LocalLevelName { get; set; }

    [Required]
    public LocalLevelType LocalLevelType { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual District? District { get; set; }
    public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
    public virtual ICollection<Address>? Addresses { get; set; }
}
