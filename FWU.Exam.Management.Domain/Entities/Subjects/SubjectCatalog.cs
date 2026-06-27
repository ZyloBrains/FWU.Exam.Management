using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Subjects;

public class SubjectCatalog
{
    public int Id { get; set; }

    [Required, MaxLength(16)]
    [Display(Name = "Subject Code")]
    public string? SubjectCode { get; set; }

    [Required, MaxLength(150)]
    [Display(Name = "Subject Name")]
    public string SubjectName { get; set; } = string.Empty;

    [MaxLength(50)]
    [Display(Name = "Short Name")]
    public string? ShortName { get; set; }

    [MaxLength(500)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Credit Hours")]
    public int? CreditHours { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Subject Type")]
    public int SubjectTypeId { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    public virtual SubjectType? SubjectType { get; set; }
    public virtual ICollection<SubjectOffering>? SubjectOfferings { get; set; }
}
