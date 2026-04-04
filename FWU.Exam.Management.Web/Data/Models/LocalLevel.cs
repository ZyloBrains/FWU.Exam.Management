using fwu_examination_management_system.Data.Models.Students;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class LocalLevel
{
    public int Id { get; set; }

    public int DistrictId { get; set; }

    [Required, MaxLength(100)]
    public string? LocalLevelName { get; set; }

    [MaxLength(50)]
    public string? Remark { get; set; }

    public bool? IsActive { get; set; }

    public virtual District? District { get; set; }
    public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
