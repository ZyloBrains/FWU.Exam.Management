using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Subjects;

public class CurriculumVersion
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public int ProgramId { get; set; }
    public int EffectiveAcademicYearId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public virtual Program? Program { get; set; }
    public virtual AcademicYear? EffectiveAcademicYear { get; set; }
}
