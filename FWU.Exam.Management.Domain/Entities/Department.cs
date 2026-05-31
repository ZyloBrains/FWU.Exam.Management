using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Department
{
    public int Id { get; set; }

    [Required, MaxLength(10)]
    public string DepartmentCode { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string DepartmentName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ShortName { get; set; }

    [MaxLength(500)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Program>? Programs { get; set; }
    public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
