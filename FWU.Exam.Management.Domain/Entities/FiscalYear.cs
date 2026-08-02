using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class FiscalYear
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Fiscal Year Name")]
    public string FiscalYearName { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    [Display(Name = "Start Date")]
    public string StartDate { get; set; } = string.Empty;

    [Required, MaxLength(10)]
    [Display(Name = "End Date")]
    public string EndDate { get; set; } = string.Empty;

    [Display(Name = "Is Running")]
    public bool IsRunning { get; set; }

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    [MaxLength(30)]
    [Display(Name = "Fiscal Year Code")]
    public string? FiscalYearCode { get; set; }
}

