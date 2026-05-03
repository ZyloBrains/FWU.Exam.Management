using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Location;

public class Province
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string? ProvinceName { get; set; }

    [MaxLength(10)]
    public string? ProvinceCode { get; set; }

    public bool IsActive { get; set; }
        public virtual ICollection<District>? Districts { get; set; }
}
