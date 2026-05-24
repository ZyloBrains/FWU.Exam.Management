using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FWU.Exam.Management.Domain.Entities;

public class Batch : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    public int AcademicYearId { get; set; }

    [Required, MaxLength(50)]
    public string? BatchName { get; set; }

    [MaxLength(50)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey(nameof(AcademicYearId))]
    public virtual AcademicYear? AcademicYear { get; set; }
    public virtual ICollection<StudentAdmission>? StudentAdmissions { get; set; }
    public virtual ICollection<ExamSlot>? ExamSlots { get; set; }
}
