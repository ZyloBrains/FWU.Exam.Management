using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    // 3. ApplicationVoucher
    [Table("ApplicationVoucher")]
    public class ApplicationVoucher
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ApplicationVoucherId { get; set; }
        [Required]
        [StringLength(50)]
        public string VoucherNo { get; set; }
        [Required]
        [StringLength(1024)]
        public string StudentName { get; set; }
        public DateTime? DOBAD { get; set; }
        [StringLength(50)]
        public string DOBBS { get; set; }
        public decimal Amount { get; set; }
        public DateTime? VoucherDate { get; set; }
        public DateTime? Timestamp { get; set; }
        [Required]
        [StringLength(1024)]
        public string ContactNo { get; set; }
        [StringLength(1024)]
        public string Branch { get; set; }
        public int ExamScheduleId { get; set; }
        public int? StudentRegistrationId { get; set; }
    }

}
