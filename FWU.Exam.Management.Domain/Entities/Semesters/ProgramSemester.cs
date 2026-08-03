using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Semesters;

public class ProgramSemester
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Program")]
    public int ProgramId { get; set; }
    public virtual Program? Program { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Semester")]
    public int SemesterId { get; set; }
    public virtual Semester? Semester { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    [Range(0, int.MaxValue)]
    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; }
}
