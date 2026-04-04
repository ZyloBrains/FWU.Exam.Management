using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models.Subjects;

public class SubjectGroupDetailMap
{
    public int SubjectGroupId { get; set; }

    public int SubjectDetailId { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    [ForeignKey(nameof(SubjectGroupId))]
    [ValidateNever]
    public virtual SubjectGroup SubjectGroup { get; set; }

    [ForeignKey(nameof(SubjectDetailId))]
    [ValidateNever]
    public virtual SubjectDetail SubjectDetail { get; set; }
}
