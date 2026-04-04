using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Subjects;

public class SubjectType
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string? SubjectTypeName { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public int? MaxAllowedSubjects { get; set; }
    public virtual ICollection<SubjectDetail>? SubjectDetails { get; set; }
}
