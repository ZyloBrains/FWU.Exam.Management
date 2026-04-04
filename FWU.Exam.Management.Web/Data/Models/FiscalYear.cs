using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models;

public class FiscalYear
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string FiscalYearName { get; set; }

    [Required, MaxLength(10)]
    public string StartDate { get; set; }

    [Required, MaxLength(10)]
    public string EndDate { get; set; }

    public bool IsRunning { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    [MaxLength(4)]
    public string FiscalYearCode { get; set; }
}
