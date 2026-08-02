using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities.Location;

public class Province
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Province Name")]
    public string ProvinceName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? ProvinceCode { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    public virtual ICollection<District> Districts { get; set; } = [];
}
