using FWU.Exam.Management.Domain.Entities.Subjects;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Semesters;

public class Semester
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Number")]
    public int Number { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    [Display(Name = "Code")]
    public string Code { get; set; } = string.Empty;

    [MaxLength(50)]
    [Display(Name = "Remark")]
    public string? Remark { get; set; }

    public virtual ICollection<SubjectOffering> SubjectOfferings { get; set; } = [];
    public virtual ICollection<ProgramSemester> ProgramSemesters { get; set; } = [];
}
