using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class Region
{
    [Key]
    public int RegionId { get; set; }

    [Required, MaxLength(2)]
    public string RegionCode { get; set; }

    [Required, MaxLength(100)]
    public string RegionName { get; set; }

    [MaxLength(55)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }
 
}
