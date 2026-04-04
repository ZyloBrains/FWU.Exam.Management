using fwu_examination_management_system.Data.Models.Exams;
using fwu_examination_management_system.Data.Models.Students;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models;

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
    [ValidateNever]
    public virtual AcademicYear? AcademicYear { get; set; }
    [ValidateNever]
    public virtual ICollection<StudentAdmission>? StudentAdmissions { get; set; }
    [ValidateNever]
    public virtual ICollection<ExamScheduleBatch>? ExamScheduleBatches { get; set; }
}
