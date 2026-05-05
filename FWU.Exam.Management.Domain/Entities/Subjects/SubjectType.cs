using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Subjects;

public class SubjectType
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    public int? MaxAllowedSubjects { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }

    public virtual ICollection<SubjectCatalog>? SubjectCatalogs { get; set; }
}
