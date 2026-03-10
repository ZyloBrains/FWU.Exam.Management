using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    // 4. Bank
    [Table("Bank")]
    public class Bank
    {
        [Key]
        public int BankID { get; set; }
        [Required]
        [StringLength(100)]
        public string BankName { get; set; }
        [StringLength(25)]
        public string BankCode { get; set; }
        [StringLength(255)]
        public string Remarks { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
