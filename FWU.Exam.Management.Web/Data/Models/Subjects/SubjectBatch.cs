using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models.Subjects;

public class SubjectBatch
{
    public int Id { get; set; }

    [Required, MaxLength(255)]
    public string SubjectBatchName { get; set; }

    public int EffectiveAcademicYearId { get; set; }
    public int ProgramsId { get; set; }

    [MaxLength(1024)]
    public string? Remarks { get; set; }

    [ForeignKey(nameof(EffectiveAcademicYearId))]
    [ValidateNever]
    public virtual AcademicYear AcademicYear { get; set; }

    [ForeignKey(nameof(ProgramsId))]
    [ValidateNever]
    public virtual Programs Program { get; set; }
}
