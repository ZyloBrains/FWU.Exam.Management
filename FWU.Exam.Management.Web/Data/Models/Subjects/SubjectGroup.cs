using fwu_examination_management_system.Data.Models.Students;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models.Subjects;

public class SubjectGroup
{
    [Key]
    public int SubjectGroupId { get; set; }

    public int ProgramsId { get; set; }

    [Required, MaxLength(250)]
    public string SubjectGroupName { get; set; }

    [Required, MaxLength(250)]
    public string SubjectGroupShortName { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public int YearPartId { get; set; }
    public bool? IsExtraAllowed { get; set; }
    public bool? IsCompulsory { get; set; }

    [ForeignKey(nameof(ProgramsId))]
    [ValidateNever]
    public virtual Programs Program { get; set; }

    [ForeignKey(nameof(YearPartId))]
    [ValidateNever]
    public virtual YearPart YearPart { get; set; }
    [ValidateNever]
    public virtual ICollection<StudentAdmission> StudentAdmissions { get; set; }
    [ValidateNever]
    public virtual ICollection<SubjectDetail> SubjectDetails { get; set; }
    [ValidateNever]
    public virtual ICollection<SubjectGroupDetailMap> SubjectGroupDetailMaps { get; set; }
}
