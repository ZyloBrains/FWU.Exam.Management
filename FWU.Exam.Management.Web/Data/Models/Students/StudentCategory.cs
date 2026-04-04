using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Students;

public class StudentCategory
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string? StudentCategoryName { get; set; }

    public bool IsActive { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    [ValidateNever]
    public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
