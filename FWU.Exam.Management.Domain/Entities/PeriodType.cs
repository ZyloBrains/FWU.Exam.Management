using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class PeriodType
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string? PeriodTypeName { get; set; }

    public decimal? NumberOfMonths { get; set; }
    public bool? IsActive { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

}
