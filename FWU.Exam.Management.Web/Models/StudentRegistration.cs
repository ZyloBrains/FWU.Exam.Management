using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models
{
    public class StudentRegistration
    {
        [Key]
        public int StudentRegistrationId { get; set; }

        public int AcademicYearId { get; set; }
        public int LevelId { get; set; }
        public int FacultyId { get; set; }
        public int CollegeId { get; set; }

        [MaxLength(50)]
        public string RegistrationNumber { get; set; }

        [Required, MaxLength(80)]
        public string FirstName { get; set; }

        [MaxLength(30)]
        public string MiddleName { get; set; }

        [Required, MaxLength(30)]
        public string LastName { get; set; }

        [MaxLength(100)]
        public string NepaliName { get; set; }

        [MaxLength(15)]
        public string ContactNumber { get; set; }

        [MaxLength(15)]
        public string Phone { get; set; }

        [MaxLength(50)]
        public string Email { get; set; }

        [Required, MaxLength(10)]
        public string DateOfBirthBs { get; set; }

        public DateTime DateOfBirthAd { get; set; }

        public int GenderId { get; set; }
        public int? IndexGroupId { get; set; }

        [MaxLength(5)]
        public string BloodGroup { get; set; }

        [MaxLength(50)]
        public string Nationality { get; set; }

        [MaxLength(50)]
        public string Religion { get; set; }

        public int DistrictId { get; set; }

        [MaxLength(100)]
        public string MunicipalityVdc { get; set; }

        [MaxLength(50)]
        public string WardNumber { get; set; }

        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public int? StudentRegistrationIndex { get; set; }
        public int StudentCategoryId { get; set; }
        public int? VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }
        public int? PhotoAttachmentId { get; set; }
        public int? EthnicityId { get; set; }

        [MaxLength(50)]
        public string EntranceRollNumber { get; set; }

        public int? EntryFormatId { get; set; }
        public bool? IsRegistrationNumberGenerated { get; set; }

        [MaxLength(50)]
        public string RowIndex { get; set; }

        [MaxLength(50)]
        public string PreviousAcademicYear { get; set; }

        [MaxLength(50)]
        public string PreviousSymbolNumber { get; set; }

        public int? StudentRegistrationSearchId { get; set; }
        public int? LocalLevelId { get; set; }

        [ForeignKey(nameof(AcademicYearId))]
        public virtual AcademicYear AcademicYear { get; set; }

        [ForeignKey(nameof(LevelId))]
        public virtual Level Level { get; set; }

        [ForeignKey(nameof(FacultyId))]
        public virtual Faculty Faculty { get; set; }

        [ForeignKey(nameof(CollegeId))]
        public virtual College College { get; set; }

        [ForeignKey(nameof(GenderId))]
        public virtual Gender Gender { get; set; }

        [ForeignKey(nameof(DistrictId))]
        public virtual District District { get; set; }

        [ForeignKey(nameof(StudentCategoryId))]
        public virtual StudentCategory StudentCategory { get; set; }

        [ForeignKey(nameof(EthnicityId))]
        public virtual Ethnicity Ethnicity { get; set; }

        [ForeignKey(nameof(LocalLevelId))]
        public virtual LocalLevel LocalLevel { get; set; }

        [ForeignKey(nameof(IndexGroupId))]
        public virtual IndexGroup IndexGroup { get; set; }

        [ForeignKey(nameof(EntryFormatId))]
        public virtual EntryFormat EntryFormat { get; set; }

        [ForeignKey(nameof(PhotoAttachmentId))]
        public virtual UserAttachment PhotoAttachment { get; set; }

        [ForeignKey(nameof(StudentRegistrationSearchId))]
        public virtual StudentRegistrationSearch StudentRegistrationSearch { get; set; }

        public virtual ICollection<ApplicationVoucher> ApplicationVouchers { get; set; }
        public virtual ICollection<PaymentRequestLog> PaymentRequestLogs { get; set; }
        public virtual ICollection<StudentAdmission> StudentAdmissions { get; set; }
        public virtual ICollection<StudentGuardian> StudentGuardians { get; set; }
        public virtual ICollection<StudentQualification> StudentQualifications { get; set; }
        public virtual ICollection<AppUser> Users { get; set; }
    }
}
