using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Payments;

public class ESewaConfiguration
{
    public int Id { get; set; }

    [Required]
    [MaxLength(256)]
    [Display(Name = "Post URL")]
    public string? PostUrl { get; set; }

    [Required]
    [MaxLength(50)]
    [Display(Name = "Product Code")]
    public string? ProductCode { get; set; }

    [Required]
    [MaxLength(256)]
    [Display(Name = "Secret Key")]
    public string? SecretKey { get; set; }

    [Required]
    [MaxLength(256)]
    [Display(Name = "Success URL")]
    public string? SuccessUrl { get; set; }

    [Display(Name = "Service Charge Amount")]
    public decimal ServiceChargeAmount { get; set; }

    [Required]
    [MaxLength(256)]
    [Display(Name = "Verify URL")]
    public string? VerifyUrl { get; set; }
}
