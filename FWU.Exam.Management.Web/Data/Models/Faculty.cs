using fwu_examination_management_system.Data.Models.Students;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class Faculty
{
    public int Id { get; set; }

    [Required, MaxLength(10)]
    public string? FacultyCode { get; set; }

    [Required, MaxLength(200)]
    public string? FacultyName { get; set; }

    [MaxLength(50)]
    public string? ShortName { get; set; }

    [MaxLength(100)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    [ValidateNever]
    public virtual ICollection<Program>? Programs { get; set; }
    [ValidateNever]
    public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
