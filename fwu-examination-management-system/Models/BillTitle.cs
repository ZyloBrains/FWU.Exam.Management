using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    // 7. BillTitle
    [Table("BillTitle")]
    public class BillTitle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BillTitleId { get; set; }
        [Required]
        [StringLength(255)]
        public string BillTitleName { get; set; } = string.Empty;
        [StringLength(1024)]
        public string? Category { get; set; }
        public bool Active { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int? ModifiedByUserId { get; set; }
        public DateTime? ModifedDateTime { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? ThroughDate { get; set; }
        public DateTime? ApplicableDate { get; set; }
        public int? ExamScheduleId { get; set; }
    }

}
