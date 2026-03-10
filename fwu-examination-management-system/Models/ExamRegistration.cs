using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    // 11. ExamRegistration
    [Table("ExamRegistration")]
    public class ExamRegistration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExamRegistrationID { get; set; }
        public int StudentProgramYearPartID { get; set; }
        public int AcademicYearID { get; set; }
        public int? ExamCenterID { get; set; }
        public int CollegeID { get; set; }
        [StringLength(20)]
        public string ExamRollNo { get; set; }
        public long? ExamRollNoCoding { get; set; }
        public decimal? FeeEnclosed { get; set; }
        public decimal? AttendancePercentage { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public bool? IsVerifiedByCollege { get; set; }
        public int? VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public bool? IsWithHeld { get; set; }
        [StringLength(50)]
        public string SGPA { get; set; }
        [StringLength(255)]
        public string Remarks { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsExamRegistered { get; set; }
        public int? TypeID { get; set; }
        public int ExamScheduleId { get; set; }
        public int? RollNoIndex { get; set; }
        public bool? IsAppliedByStudent { get; set; }
        public int? ProgramId { get; set; }
        public int? ApplicationVoucherId { get; set; }
        public int? AdminVerifiedBy { get; set; }
        public DateTime? AdminVerifiedDate { get; set; }
    }

}
