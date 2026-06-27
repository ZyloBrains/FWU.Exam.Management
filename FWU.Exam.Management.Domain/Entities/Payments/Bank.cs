using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities.Payments;

public class Bank
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    [Display(Name = "Bank Name")]
    public string? BankName { get; set; }

    [MaxLength(16)]
    [Display(Name = "Bank Code")]
    public string? BankCode { get; set; }

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }
}
