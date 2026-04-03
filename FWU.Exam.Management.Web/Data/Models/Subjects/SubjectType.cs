using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Subjects;

public class SubjectType
{
    [Key]
    public int SubjectTypeId { get; set; }

    [Required, MaxLength(50)]
    public string SubjectTypeName { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public int? MaxAllowedSubjects { get; set; }
    [ValidateNever]
    public virtual ICollection<SubjectDetail> SubjectDetails { get; set; }
}
