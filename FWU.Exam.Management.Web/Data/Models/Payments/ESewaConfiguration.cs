using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Payments;

public class ESewaConfiguration
{
    [Key]
    public int ESewaConfigurationId { get; set; }

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
