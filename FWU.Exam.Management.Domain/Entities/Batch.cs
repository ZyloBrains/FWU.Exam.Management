using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FWU.Exam.Management.Domain.Entities;

public class Batch
{
    public int Id { get; set; }

    public int AcademicYearId { get; set; }

    [Required, MaxLength(50)]
    public string? BatchName { get; set; } 

    [MaxLength(50)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }
    [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear? AcademicYear { get; set; }
        public virtual ICollection<StudentAdmission>? StudentAdmissions { get; set; }
}
