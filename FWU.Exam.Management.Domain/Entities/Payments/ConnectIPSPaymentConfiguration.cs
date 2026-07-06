using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Payments;

public class ConnectIpsPaymentConfiguration
{
    public int Id { get; set; }

    [Required, MaxLength(1024)]
    [Display(Name = "Gateway URL")]
    public string? GatewayUrl { get; set; }

    [Required, MaxLength(1024)]
    [Display(Name = "Merchant ID")]
    public string? MerchantId { get; set; }

    [Required, MaxLength(1024)]
    [Display(Name = "App ID")]
    public string? AppId { get; set; }

    [Required, MaxLength(1024)]
    [Display(Name = "App Name")]
    public string? AppName { get; set; }

    [Required, MaxLength(1024)]
    [Display(Name = "Validation API URL")]
    public string? ValidationApiUrl { get; set; }

    [Required, MaxLength(1024)]
    [Display(Name = "Username For Validation API")]
    public string? UsernameForValidationApi { get; set; }

    [Required, MaxLength(1024)]
    [Display(Name = "Password For Validation API")]
    public string? PasswordForValidationApi { get; set; }

    [Required, MaxLength(1024)]
    [Display(Name = "Password For Creditor PFX")]
    public string? PasswordForCreditorPfx { get; set; }

    [MaxLength(10)]
    [Display(Name = "Transaction Currency")]
    public string? TransactionCurrency { get; set; }
}
