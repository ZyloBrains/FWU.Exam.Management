using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FWU.Exam.Management.Domain.Interfaces;

namespace FWU.Exam.Management.Domain.Entities.Payments;

public class ESewaConfiguration : ITenantScoped
{
    public int Id { get; set; }
    public int? TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    [Required]
    [MaxLength(256)]
    [Display(Name = "Post URL")]
    public string PostUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Display(Name = "Product Code")]
    public string ProductCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    [Display(Name = "Secret Key")]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    [Display(Name = "Success URL")]
    public string SuccessUrl { get; set; } = string.Empty;

    [Display(Name = "Service Charge Amount")]
    public decimal ServiceChargeAmount { get; set; }

    [Required]
    [MaxLength(256)]
    [Display(Name = "Verify URL")]
    public string VerifyUrl { get; set; } = string.Empty;
}
