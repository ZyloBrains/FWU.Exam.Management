using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FWU.Exam.Management.Domain.Interfaces;

namespace FWU.Exam.Management.Domain.Entities.Payments;

public class KhaltiConfiguration : ITenantScoped
{
    public int Id { get; set; }
    public int? TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    [Required]
    [MaxLength(400)]
    [Display(Name = "Return URL")]
    public string ReturnUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(400)]
    [Display(Name = "Website URL")]
    public string WebsiteUrl { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    [Display(Name = "Amount")]
    public decimal? Amount { get; set; }

    [Required]
    [MaxLength(400)]
    [Display(Name = "Product Name")]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    [MaxLength(400)]
    [Display(Name = "Authorization Key")]
    public string AuthorizationKey { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    [Display(Name = "Service Charge")]
    public int ServiceCharge { get; set; }

    [Required]
    [MaxLength(400)]
    [Display(Name = "Post URL")]
    public string PostUrl { get; set; } = string.Empty;

    [Required]
    [MaxLength(400)]
    [Display(Name = "Verify URL")]
    public string VerifyUrl { get; set; } = string.Empty;
}
