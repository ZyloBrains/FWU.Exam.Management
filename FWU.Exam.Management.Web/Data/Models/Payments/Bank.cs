using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Payments;

public class Bank
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string? BankName { get; set; }

    [MaxLength(25)]
    public string? BankCode { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }
    public bool IsActive { get; set; }
    public virtual ICollection<BankVoucher>? BankVouchers { get; set; }
}
