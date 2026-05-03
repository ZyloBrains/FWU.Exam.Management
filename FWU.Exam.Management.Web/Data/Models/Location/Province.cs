using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Location;

public class Province
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string? ProvinceName { get; set; }

    [MaxLength(10)]
    public string? ProvinceCode { get; set; }

    public bool IsActive { get; set; }
    [ValidateNever]
    public virtual ICollection<District>? Districts { get; set; }
}
