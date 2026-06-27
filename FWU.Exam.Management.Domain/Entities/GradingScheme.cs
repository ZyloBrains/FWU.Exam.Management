using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class GradingScheme : IAuditable
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    [Display(Name = "Program")]
    public int ProgramId { get; set; }

    [Display(Name = "Academic Year")]
    public int? AcademicYearId { get; set; }

    [MaxLength(500)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    public virtual Program? Program { get; set; }
    public virtual AcademicYear? AcademicYear { get; set; }
    public virtual ICollection<GradeDefinition>? GradeDefinitions { get; set; }
}
