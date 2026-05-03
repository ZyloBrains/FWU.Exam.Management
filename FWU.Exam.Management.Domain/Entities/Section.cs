using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Section
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string? SectionName { get; set; }

    public int? ProgramsId { get; set; }
    public int? BatchId { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public virtual Program? Programs { get; set; }

    public virtual Batch? Batch { get; set; }
    public virtual ICollection<StudentAdmission>? StudentAdmissions { get; set; }
}
