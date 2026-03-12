using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    // 10. StudentRegistration
    [Table("StudentRegistration")]
    public class StudentRegistration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudentRegistrationID { get; set; }
        public int AcademicYearID { get; set; }
        public int LevelID { get; set; }
        public int FacultyID { get; set; }
        public int CollegeID { get; set; }
        [StringLength(50)]
        public string? RegistrationNo { get; set; }
        [Required]
        [StringLength(80)]
        public string FirstName { get; set; } = string.Empty;
        [StringLength(30)]
        public string? MiddleName { get; set; }
        [Required]
        [StringLength(30)]
        public string LastName { get; set; } = string.Empty;
        [StringLength(100)]
        public string? NepaliName { get; set; }
        [StringLength(15)]
        public string? ContactNo { get; set; }
        [StringLength(15)]
        public string? Phone { get; set; }
        [StringLength(50)]
        public string? Email { get; set; }
        [Required]
        [StringLength(10)]
        public string BirthDateBS { get; set; } = string.Empty;
        public DateTime BirthDateAD { get; set; }
        public int GenderID { get; set; }
        public int? IndexGroupID { get; set; }
        [StringLength(5)]
        public string? BloodGroup { get; set; }
        [StringLength(50)]
        public string? Nationality { get; set; }
        [StringLength(50)]
        public string? Religion { get; set; }
        public int DistrictID { get; set; }
        [StringLength(100)]
        public string? MunVDC { get; set; }
        [StringLength(50)]
        public string? WardNo { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? StudentRegistrationIndex { get; set; }
        public int StudentCategoryID { get; set; }
        public int? VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public int? PhotoAttachmentId { get; set; }
        public int? EthnicityID { get; set; }
        [StringLength(50)]
        public string? EntranceRollNo { get; set; }
        public int? EntryFormatID { get; set; }
        public bool? IsRegNoGenerated { get; set; }
        [StringLength(50)]
        public string? RowIndex { get; set; }
        [StringLength(50)]
        public string? PreviousYear { get; set; }
        [StringLength(50)]
        public string? PreviousSymbolNo { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? FullName { get; private set; }

        public int? StudentRegistrationSearchId { get; set; }
        public int? LocalLevelId { get; set; }
    }

}
