using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    // 9. College
    [Table("College")]
    public class College
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CollegeID { get; set; }
        [Required]
        [StringLength(50)]
        public string CollegeCode { get; set; }
        [Required]
        [StringLength(500)]
        public string CollegeName { get; set; }
        [StringLength(500)]
        public string CollegeNameNep { get; set; }
        [StringLength(500)]
        public string ShortName { get; set; }
        public DateTime? EstablishedDate { get; set; }
        public DateTime? CollapseDate { get; set; }
        public int DistrictID { get; set; }
        [StringLength(255)]
        public string MunicipalityVdc { get; set; }
        [StringLength(50)]
        public string WardNo { get; set; }
        [StringLength(50)]
        public string HouseNo { get; set; }
        [StringLength(50)]
        public string WebAddress { get; set; }
        [StringLength(50)]
        public string EmailAddress { get; set; }
        [StringLength(150)]
        public string Phone1 { get; set; }
        [StringLength(15)]
        public string Phone2 { get; set; }
        [StringLength(255)]
        public string PrincipalName { get; set; }
        [StringLength(50)]
        public string PrincipalContactNo { get; set; }
        [StringLength(15)]
        public string Fax { get; set; }
        [StringLength(255)]
        public string Remarks { get; set; }
        public bool IsExamCentreOnly { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? CollegeTypeID { get; set; }
        public decimal? AllocatedAmount { get; set; }
        public int AreaId { get; set; }
        public int? DisplayOrder { get; set; }
        public int? QuestionSetId { get; set; }
    }



}
