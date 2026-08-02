using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using FWU.Exam.Management.Domain.Interfaces;

namespace FWU.Exam.Management.Domain.Entities.Payments;

public class Bank : ITenantScoped
{
    public int Id { get; set; }
    public int? TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Bank Name")]
    public string BankName { get; set; } = string.Empty;

    [MaxLength(30)]
    [Display(Name = "Bank Code")]
    public string? BankCode { get; set; }

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }
}

