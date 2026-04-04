using fwu_examination_management_system.Data.Models.Students;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class EntryFormat
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string? EntryFormatName { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    [ValidateNever]
    public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
