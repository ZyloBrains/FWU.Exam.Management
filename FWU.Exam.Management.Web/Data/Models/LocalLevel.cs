using fwu_examination_management_system.Data.Models.Students;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models;

public class LocalLevel
{
    public int Id { get; set; }

    public int DistrictId { get; set; }

    [Required, MaxLength(100)]
    public string LocalLevelName { get; set; }

    [MaxLength(50)]
    public string? Remark { get; set; }

    public bool? IsActive { get; set; }

    [ForeignKey(nameof(DistrictId))]
    [ValidateNever]
    public virtual District District { get; set; }
    [ValidateNever]
    public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; }
}
