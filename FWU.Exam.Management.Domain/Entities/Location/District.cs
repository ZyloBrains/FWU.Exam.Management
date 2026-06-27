using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities.Location;

public class District
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Province")]
    public int ProvinceId { get; set; }

    [MaxLength(16)]
    [Display(Name = "District Code")]
    public string? DistrictCode { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "District Name")]
    public string? DistrictName { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    public virtual Province? Province { get; set; }

    public virtual ICollection<College?>? Colleges { get; set; }
    public virtual ICollection<LocalLevel?>? LocalLevels { get; set; }
    public virtual ICollection<StudentRegistration?>? StudentRegistrations { get; set; }
}
