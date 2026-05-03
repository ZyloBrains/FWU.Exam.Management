using fwu_examination_management_system.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Location;

public class Address
{
    public int Id { get; set; }

    public int LocalLevelId { get; set; }

    public int? WardNumber { get; set; }

    [MaxLength(50)]
    public string? HouseNumber { get; set; }

    [MaxLength(255)]
    public string? ToleStreet { get; set; }

    [MaxLength(500)]
    public string? FullAddress { get; set; }

    public AddressType? AddressType { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual LocalLevel? LocalLevel { get; set; }
}
