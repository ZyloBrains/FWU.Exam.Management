using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models;

public class UserProgramMap
{
    public int Id { get; set; }

    public string UserId { get; set; }
    public int ProgramId { get; set; }

    [ForeignKey(nameof(UserId))]
    [ValidateNever]
    public virtual AppUser User { get; set; }

    [ForeignKey(nameof(ProgramId))]
    [ValidateNever]
    public virtual Programs Program { get; set; }
}
