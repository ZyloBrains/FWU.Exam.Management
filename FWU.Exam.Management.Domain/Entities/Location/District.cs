using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Location;

public class District
{
    public int Id { get; set; }

    public int ProvinceId { get; set; }

    [MaxLength(50)]
    public string? DistrictCode { get; set; }

    [Required, MaxLength(255)]
    public string? DistrictName { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual Province? Province { get; set; }

    public virtual ICollection<College?>? Colleges { get; set; }
    public virtual ICollection<LocalLevel?>? LocalLevels { get; set; }
    public virtual ICollection<StudentRegistration?>? StudentRegistrations { get; set; }
}
