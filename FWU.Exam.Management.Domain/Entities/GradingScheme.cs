using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class GradingScheme : IAuditable
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int ProgramId { get; set; }
    public int? AcademicYearId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public virtual Program? Program { get; set; }
    public virtual AcademicYear? AcademicYear { get; set; }
    public virtual ICollection<GradeDefinition>? GradeDefinitions { get; set; }
}
