using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Subjects;

public class SubjectBatch
{
    public int Id { get; set; }

    [Required, MaxLength(255)]
    public string? SubjectBatchName { get; set; }

    public int EffectiveAcademicYearId { get; set; }
    public int ProgramsId { get; set; }

    [MaxLength(1024)]
    public string? Remarks { get; set; }

    public virtual AcademicYear? AcademicYear { get; set; }

    public virtual Program? Program { get; set; }
}
