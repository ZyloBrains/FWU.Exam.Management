using FWU.Exam.Management.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities.Location;

public class Address
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Local Level")]
    public int LocalLevelId { get; set; }

    [MaxLength(50)]
    [Display(Name = "Ward Number")]
    public int? WardNumber { get; set; }

    [MaxLength(50)]
    [Display(Name = "House Number")]
    public string? HouseNumber { get; set; }

    [MaxLength(255)]
    [Display(Name = "Tole/Street")]
    public string? ToleStreet { get; set; }

    [MaxLength(500)]
    [Display(Name = "Full Address")]
    public string? FullAddress { get; set; }

    [Display(Name = "Address Type")]
    public AddressType? AddressType { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual LocalLevel? LocalLevel { get; set; }
}
