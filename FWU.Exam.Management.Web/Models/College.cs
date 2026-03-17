using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{

    public class College
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CollegeId { get; set; }

        [Required, MaxLength(50)]
        public string CollegeCode { get; set; }

        [Required, MaxLength(500)]
        public string CollegeName { get; set; }

        [MaxLength(500)]
        public string CollegeNameNepali { get; set; }

        [MaxLength(500)]
        public string ShortName { get; set; }

        public DateTime? EstablishedDate { get; set; }
        public DateTime? ClosedDate { get; set; }

        public int DistrictId { get; set; }

        [MaxLength(255)]
        public string MunicipalityVdc { get; set; }

        [MaxLength(50)]
        public string WardNumber { get; set; }

        [MaxLength(50)]
        public string HouseNumber { get; set; }

        [MaxLength(50)]
        public string Website { get; set; }

        [MaxLength(50)]
        public string Email { get; set; }

        [MaxLength(150)]
        public string Phone1 { get; set; }

        [MaxLength(15)]
        public string Phone2 { get; set; }

        [MaxLength(255)]
        public string PrincipalName { get; set; }

        [MaxLength(50)]
        public string PrincipalContactNumber { get; set; }

        [MaxLength(15)]
        public string Fax { get; set; }

        [MaxLength(255)]
        public string Remarks { get; set; }

        public bool IsExamCenterOnly { get; set; }
        public bool IsActive { get; set; }

        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public int? CollegeTypeId { get; set; }
        public decimal? AllocatedAmount { get; set; }
        public int AreaId { get; set; }
        public int? DisplayOrder { get; set; }
        public int? QuestionSetId { get; set; }

        [ForeignKey(nameof(DistrictId))]
        public virtual District District { get; set; }

        [ForeignKey(nameof(CollegeTypeId))]
        public virtual CollegeType CollegeType { get; set; }

        [ForeignKey(nameof(AreaId))]
        public virtual Area Area { get; set; }

        [ForeignKey(nameof(QuestionSetId))]
        public virtual QuestionSet QuestionSet { get; set; }

        public virtual CollegeProfile CollegeProfile { get; set; }

        public virtual ICollection<BankVoucher> BankVouchers { get; set; }
        public virtual ICollection<CollegeProgram> CollegePrograms { get; set; }
        public virtual ICollection<ExamCenter> ExamCenters { get; set; }
        public virtual ICollection<ExamCenterDetail> ExamCenterDetails { get; set; }
        public virtual ICollection<ExamRegistration> ExamRegistrations { get; set; }
        public virtual ICollection<StudentAdmission> StudentAdmissions { get; set; }
        public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; }
        public virtual ICollection<AppUser> Users { get; set; }
    }
}
