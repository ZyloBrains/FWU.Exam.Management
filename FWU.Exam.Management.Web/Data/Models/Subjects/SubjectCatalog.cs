using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Subjects;

public class SubjectCatalog
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string SubjectCode { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string SubjectName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? ShortName { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? CreditHours { get; set; }
    public int SubjectTypeId { get; set; }
    public bool IsActive { get; set; }

    public virtual SubjectType? SubjectType { get; set; }
    public virtual ICollection<SubjectOffering>? SubjectOfferings { get; set; }
}
