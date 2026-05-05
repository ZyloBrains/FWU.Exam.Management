using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Payments;

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
}
