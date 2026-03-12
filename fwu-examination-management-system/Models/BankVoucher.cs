using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    // 5. BankVoucher
    [Table("BankVoucher")]
    public class BankVoucher
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BankVoucherID { get; set; }
        public int AcademicYearID { get; set; }
        public int CollegeID { get; set; }
        public int BillTitleId { get; set; }
        public int BankID { get; set; }
        [StringLength(100)]
        public string? BankAddress { get; set; }
        public DateTime VoucherDate { get; set; }
        [StringLength(50)]
        public string? VoucherNo { get; set; }
        public decimal VoucherAmount { get; set; }
        [StringLength(255)]
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? BankVoucherUserAttachmentId { get; set; }
        public int ExamScheduleParentId { get; set; }
    }
}
