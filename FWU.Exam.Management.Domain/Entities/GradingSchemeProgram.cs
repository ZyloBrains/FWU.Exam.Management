using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class GradingSchemeProgram
{
    [Display(Name = "Grading Scheme")]
    public int GradingSchemeId { get; set; }

    [Display(Name = "Program")]
    public int ProgramId { get; set; }

    [Display(Name = "Academic Year")]
    public int? AcademicYearId { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    public virtual GradingScheme? GradingScheme { get; set; }
    public virtual Program? Program { get; set; }
    public virtual AcademicYear? AcademicYear { get; set; }
}
