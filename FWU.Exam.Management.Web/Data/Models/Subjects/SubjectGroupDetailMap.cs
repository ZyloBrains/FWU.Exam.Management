using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Subjects;

public class SubjectGroupDetailMap
{
    public int SubjectGroupId { get; set; }

    public int SubjectDetailId { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public virtual SubjectGroup? SubjectGroup { get; set; }

    public virtual SubjectDetail? SubjectDetail { get; set; }
}
