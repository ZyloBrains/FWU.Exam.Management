using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Payments;

public class KhaltiConfiguration
{
    public int Id { get; set; }

    [MaxLength(400)]
    public string? ReturnUrl { get; set; }

    [MaxLength(400)]
    public string? WebsiteUrl { get; set; }

    public decimal? Amount { get; set; }

    [MaxLength(400)]
    public string? ProductName { get; set; }

    [MaxLength(400)]
    public string? AuthorizationKey { get; set; }

    public int ServiceCharge { get; set; }

    [MaxLength(400)]
    public string? PostUrl { get; set; }

    [MaxLength(400)]
    public string? VerifyUrl { get; set; }
}
