using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FWU.Exam.Management.Domain.Entities;

public class Batch
{
    public int Id { get; set; }

    [Range(1, int.MaxValue)]
    public int AcademicYearId { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Batch Name")]
    public string BatchName { get; set; } = string.Empty;

    [MaxLength(50)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    [ForeignKey(nameof(AcademicYearId))]
    public virtual AcademicYear? AcademicYear { get; set; }
    public virtual ICollection<StudentAdmission> StudentAdmissions { get; set; } = [];
    public virtual ICollection<ExamSlot> ExamSlots { get; set; } = [];
}
