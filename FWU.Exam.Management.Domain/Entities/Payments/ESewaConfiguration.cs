using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Payments;

public class ESewaConfiguration
{
    public int Id { get; set; }

    [MaxLength(256)]
    public string? PostUrl { get; set; }

    [MaxLength(50)]
    public string? ProductCode { get; set; }

    [MaxLength(256)]
    public string? SecretKey { get; set; }

    [MaxLength(256)]
    public string? SuccessUrl { get; set; }

    public decimal ServiceChargeAmount { get; set; }

    [MaxLength(256)]
    public string? VerifyUrl { get; set; }
}
