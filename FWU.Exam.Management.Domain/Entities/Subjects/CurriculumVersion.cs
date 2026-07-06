using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities.Subjects;

public class CurriculumVersion : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    [Display(Name = "Program")]
    public int ProgramId { get; set; }

    [Display(Name = "Effective Academic Year")]
    public int EffectiveAcademicYearId { get; set; }

    [MaxLength(500)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    public virtual Program? Program { get; set; }
    public virtual AcademicYear? EffectiveAcademicYear { get; set; }
}
