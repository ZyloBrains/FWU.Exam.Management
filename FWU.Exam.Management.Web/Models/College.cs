using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{

    public class College:AuditBase
    {
        [Key]
        public int CollegeId { get; set; }

        [Required, MaxLength(50)]
        public string CollegeCode { get; set; }

        [Required, MaxLength(500)]
        public string CollegeName { get; set; }

        [MaxLength(500)]
        public string? CollegeNameNepali { get; set; }

        [MaxLength(500)]
        public string? ShortName { get; set; }

        public DateTime? EstablishedDate { get; set; }
        public DateTime? ClosedDate { get; set; }

        public int DistrictId { get; set; }

        [MaxLength(255)]
        public string? MunicipalityVdc { get; set; }

        [MaxLength(50)]
        public string? WardNumber { get; set; }

        [MaxLength(50)]
        public string? HouseNumber { get; set; }

        [MaxLength(50)]
        public string? Website { get; set; }

        [MaxLength(50)]
        public string? Email { get; set; }

        [MaxLength(150)]
        public string? Phone1 { get; set; }

        [MaxLength(15)]
        public string? Phone2 { get; set; }

        [MaxLength(255)]
        public string? PrincipalName { get; set; }

        [MaxLength(50)]
        public string? PrincipalContactNumber { get; set; }

        [MaxLength(15)]
        public string? Fax { get; set; }

        [MaxLength(255)]
        public string? Remarks { get; set; }

        public bool IsExamCenterOnly { get; set; }
        public bool IsActive { get; set; }

        public int? CollegeTypeId { get; set; }
        public decimal? AllocatedAmount { get; set; }
        public int AreaId { get; set; }
        public int? DisplayOrder { get; set; }
        public int? QuestionSetId { get; set; }

        [ForeignKey(nameof(DistrictId))]
        [ValidateNever]
        public virtual District District { get; set; }

        [ForeignKey(nameof(CollegeTypeId))]
        [ValidateNever]
        public virtual CollegeType CollegeType { get; set; }

        [ForeignKey(nameof(AreaId))]
        [ValidateNever]
        public virtual Area Area { get; set; }

        [ForeignKey(nameof(QuestionSetId))]
        [ValidateNever]
        public virtual QuestionSet QuestionSet { get; set; }

        [ValidateNever]
        public virtual CollegeProfile CollegeProfile { get; set; }

        [ValidateNever]
        public virtual ICollection<BankVoucher> BankVouchers { get; set; }

        [ValidateNever]
        public virtual ICollection<CollegeProgram> CollegePrograms { get; set; }

        [ValidateNever]
        public virtual ICollection<ExamCenter> ExamCenters { get; set; }

        [ValidateNever]
        public virtual ICollection<ExamCenterDetail> ExamCenterDetails { get; set; }

        [ValidateNever]
        public virtual ICollection<ExamRegistration> ExamRegistrations { get; set; }

        [ValidateNever]
        public virtual ICollection<StudentAdmission> StudentAdmissions { get; set; }

        [ValidateNever]
        public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; }

        [ValidateNever]
        public virtual ICollection<AppUser> Users { get; set; }
    }
}
