using fwu_examination_management_system.Data.Models.Students;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Subjects;

public class SubjectGroup
{
    public int Id { get; set; }

    public int ProgramsId { get; set; }

    [Required, MaxLength(250)]
    public string? SubjectGroupName { get; set; }

    [Required, MaxLength(250)]
    public string? SubjectGroupShortName { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public int YearPartId { get; set; }
    public bool? IsExtraAllowed { get; set; }
    public bool? IsCompulsory { get; set; }

    public virtual Program? Program { get; set; }

    public virtual YearPart? YearPart { get; set; }
    public virtual ICollection<StudentAdmission>? StudentAdmissions { get; set; }
    public virtual ICollection<SubjectDetail>? SubjectDetails { get; set; }
    public virtual ICollection<SubjectGroupDetailMap>? SubjectGroupDetailMaps { get; set; }
}
