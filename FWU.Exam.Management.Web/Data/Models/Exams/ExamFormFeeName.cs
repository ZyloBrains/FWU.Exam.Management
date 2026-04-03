using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Exams;

public class ExamFormFeeName
{
    [Key]
    public int ExamFormFeeNameId { get; set; }

    [Required, MaxLength(400)]
    public string Name { get; set; }

    public bool? IsCollegeFee { get; set; }
    [ValidateNever]
    public virtual ICollection<ExamFormFeeRate> ExamFormFeeRates { get; set; }
}
