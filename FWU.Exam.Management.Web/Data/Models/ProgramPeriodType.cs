using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class ProgramPeriodType
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string ProgramPeriodTypeName { get; set; }
    public decimal? NumberOfMonths { get; set; }
    [ValidateNever]
    public virtual ICollection<Programs> Programs { get; set; }
    [ValidateNever]
    public virtual ICollection<YearPart> YearParts { get; set; }
}
