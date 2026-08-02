using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities.Location;

public class LocalLevel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "District")]
    public int DistrictId { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Local Level Name")]
    public string LocalLevelName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Local Level Type")]
    public LocalLevelType LocalLevelType { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    public virtual District? District { get; set; }
    public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; } = [];
    public virtual ICollection<Address> Addresses { get; set; } = [];
}
