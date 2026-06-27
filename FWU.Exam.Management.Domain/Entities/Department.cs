using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Department
{
    public int Id { get; set; }

    [Required, MaxLength(16)]
    [Display(Name = "Department Code")]
    public string DepartmentCode { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    [Display(Name = "Department Name")]
    public string DepartmentName { get; set; } = string.Empty;

    [MaxLength(50)]
    [Display(Name = "Short Name")]
    public string? ShortName { get; set; }

    [MaxLength(500)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Faculty Id")]
    public int? FacultyId { get; set; }
    public virtual Faculty? Faculty { get; set; }

    public virtual ICollection<Program>? Programs { get; set; }
    public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
