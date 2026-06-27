using FWU.Exam.Management.Domain.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities;

public class Tenant
{
    public int Id { get; set; }
    [Required, MaxLength(200)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;
    [MaxLength(16)]
    [Display(Name = "Office Code")]
    public string OfficeCode { get; set; } = string.Empty;
    [MaxLength(50)]
    [Display(Name = "Contact Number")]
    public string ContactNumber { get; set; } = string.Empty;
    [MaxLength(255)]
    [Display(Name = "Address")]
    public string Address { get; set; } = string.Empty;
    [MaxLength(100)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
    [MaxLength(500)]
    [Display(Name = "Logo Path")]
    public string? LogoPath { get; set; }
    [Display(Name = "Tenant Type")]
    public TenantType TenantType { get; set; } = TenantType.Standard;
    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;
}
