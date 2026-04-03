using fwu_examination_management_system.Data.Models.Students;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models;

public class Section
{
    [Key]
    public int SectionId { get; set; }

    [Required, MaxLength(100)]
    public string SectionName { get; set; }

    public int? ProgramsId { get; set; }
    public int? BatchId { get; set; }

    [MaxLength(255)]
    public string Remarks { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey(nameof(ProgramsId))]
    [ValidateNever]
    public virtual Programs Programs { get; set; }

    [ForeignKey(nameof(BatchId))]
    [ValidateNever]
    public virtual Batch Batch { get; set; }
    [ValidateNever]
    public virtual ICollection<StudentAdmission> StudentAdmissions { get; set; }
}
