using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.ViewModels
{
    public class VoucherVerificationViewModel
    {
        [Required(ErrorMessage = "Voucher Number is required.")]
        [Display(Name = "Voucher No:")]
        [MaxLength(50)]
        public string VoucherNumber { get; set; } = string.Empty;

        public bool HasSearched { get; set; }

        public VoucherVerificationResultViewModel? Result { get; set; }
    }

    public class VoucherVerificationResultViewModel
    {
        public string VoucherNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentGateway { get; set; } = string.Empty;
        public DateTime? RequestedTime { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string CollegeName { get; set; } = string.Empty;
        public string RollNo { get; set; } = string.Empty;
        public string ContactNo { get; set; } = string.Empty;
        public string ExamName { get; set; } = string.Empty;
    }
}
