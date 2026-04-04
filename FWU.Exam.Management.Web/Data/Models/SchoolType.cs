using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models;

public class SchoolType
{
    public int Id { get; set; }

    public int PreviousLevelId { get; set; }

    [Required, MaxLength(255)]
    public string SchoolTypeName { get; set; }

    [ForeignKey(nameof(PreviousLevelId))]
    [ValidateNever]
    public virtual PreviousLevel PreviousLevel { get; set; }
}
