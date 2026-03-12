using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    // 12. ExamSchedule
    [Table("ExamSchedule")]
    public class ExamSchedule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExamScheduleID { get; set; }
        public int AcademicYearID { get; set; }
        public int LevelID { get; set; }
        public int YearPartID { get; set; }
        public int ExamTypeID { get; set; }
        [Required]
        [StringLength(50)]
        public string ExamScheduleName { get; set; } = string.Empty;
        public DateTime? StartFromAD { get; set; }
        public DateTime? EndToAD { get; set; }
        [StringLength(10)]
        public string? StartFromBS { get; set; }
        [StringLength(10)]
        public string? EndToBS { get; set; }
        public DateTime? PublishedDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        [StringLength(255)]
        public string? Remarks { get; set; }
        public bool Active { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ExamScheduleParentId { get; set; }
        public int? NegativeMark { get; set; }
        [StringLength(500)]
        public string? ProgramIds { get; set; }
        [StringLength(500)]
        public string? RegularBatchIds { get; set; }
        [StringLength(500)]
        public string? PartialBatchIds { get; set; }
        public DateTime? ExtendedDate { get; set; }
        public decimal? ExtendedDateCharge { get; set; }
        public DateTime? CollegeApproveDate { get; set; }
        public DateTime? AdmissionCardReleaseDate { get; set; }
        [StringLength(50)]
        public string? ExamScheduleCode { get; set; }
    }

}
