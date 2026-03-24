using fwu_examination_management_system.Models;
using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.ViewModels
{
    public class VoucherVerificationViewModel
    {
        [Required(ErrorMessage = "Voucher Number is required.")]
        [Display(Name = "Voucher Number")]
        [MaxLength(50)]
        public string VoucherNumber { get; set; } = string.Empty;

        public bool HasSearched { get; set; }

        public List<BankVoucher> Results { get; set; } = new();
    }
}
