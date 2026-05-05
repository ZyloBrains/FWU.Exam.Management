using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Payments;

public class ConnectIpsPaymentConfiguration
{
    public int Id { get; set; }

    [Required, MaxLength(1024)]
    public string? GatewayUrl { get; set; }

    [Required, MaxLength(1024)]
    public string? MerchantId { get; set; }

    [Required, MaxLength(1024)]
    public string? AppId { get; set; }

    [Required, MaxLength(1024)]
    public string? AppName { get; set; }

    [Required, MaxLength(1024)]
    public string? ValidationApiUrl { get; set; }

    [Required, MaxLength(1024)]
    public string? UsernameForValidationApi { get; set; }

    [Required, MaxLength(1024)]
    public string? PasswordForValidationApi { get; set; }

    [Required, MaxLength(1024)]
    public string? PasswordForCreditorPfx { get; set; }

    [MaxLength(10)]
    public string? TransactionCurrency { get; set; }
}
