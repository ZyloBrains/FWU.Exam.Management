using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class PeriodType
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Period Type Name")]
    public string PeriodTypeName { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    [Display(Name = "Number Of Months")]
    public decimal? NumberOfMonths { get; set; }

    [Display(Name = "Is Active")]
    public bool? IsActive { get; set; }

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

}
