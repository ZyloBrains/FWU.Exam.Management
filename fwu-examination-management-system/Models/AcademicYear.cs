using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    [Table("AcademicYear")]
    public class AcademicYear
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AcademicYearID { get; set; }
        public int AcademicYearCode { get; set; }
        [StringLength(50)]
        public string AcademicYearCodeNep { get; set; }
        [Required]
        [StringLength(50)]
        public string AcademicYearName { get; set; }
        [Required]
        [StringLength(50)]
        public string AcademicYearNameNep { get; set; }
        [StringLength(50)]
        public string Remark { get; set; }
        public bool IsRunning { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }


}
