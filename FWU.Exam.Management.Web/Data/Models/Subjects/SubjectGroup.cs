using fwu_examination_management_system.Data.Models.Students;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Subjects;

public class SubjectGroup
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? ShortName { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<SubjectOffering>? SubjectOfferings { get; set; }
    public virtual ICollection<StudentAdmission>? StudentAdmissions { get; set; }
    public virtual ICollection<SubjectDetail>? SubjectDetails { get; set; }
    public virtual ICollection<SubjectGroupDetailMap>? SubjectGroupDetailMaps { get; set; }
}
