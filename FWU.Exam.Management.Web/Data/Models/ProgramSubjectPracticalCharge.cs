using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models;

public class ProgramSubjectPracticalCharge
{
    [Key]
    public int ProgramSubjectPracticalChargeId { get; set; }

    public int ProgramsId { get; set; }
    public decimal PracticalSubjectCharge { get; set; }

    [ForeignKey(nameof(ProgramsId))]
    [ValidateNever]
    public virtual Programs Program { get; set; }
}
