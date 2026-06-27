using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities.Subjects;

public class SubjectType
{
    public int Id { get; set; }

    [Required, MaxLength(16)]
    [Display(Name = "Code")]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Max Allowed Subjects")]
    public int? MaxAllowedSubjects { get; set; }

    [Display(Name = "Is Default")]
    public bool IsDefault { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    public virtual ICollection<SubjectCatalog>? SubjectCatalogs { get; set; }
}
