using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class FiscalYear : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Required, MaxLength(50)]
    public string? FiscalYearName { get; set; }

    [Required, MaxLength(10)]
    public string? StartDate { get; set; }

    [Required, MaxLength(10)]
    public string? EndDate { get; set; }

    public bool IsRunning { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    [MaxLength(4)]
    public string? FiscalYearCode { get; set; }
}
