using fwu_examination_management_system.Data.Models.Colleges;
using fwu_examination_management_system.Data.Models.Payments;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models.Students;

public class StudentRegistration
{
    public int Id { get; set; }

    public int LevelId { get; set; }
    public int FacultyId { get; set; }
    public int CollegeId { get; set; }

    [MaxLength(50)]
    public string? RegistrationNumber { get; set; }

    [Required, MaxLength(80)]
    public string FirstName { get; set; }

    [MaxLength(30)]
    public string? MiddleName { get; set; }

    [Required, MaxLength(30)]
    public string LastName { get; set; }

    [MaxLength(100)]
    public string? NepaliName { get; set; }

    [MaxLength(15)]
    public string? ContactNumber { get; set; }

    [MaxLength(15)]
    public string? Phone { get; set; }
    [EmailAddress]
    [MaxLength(50)]
    public string? Email { get; set; }

    [Required, MaxLength(10)]
    public string DateOfBirthBs { get; set; }

    public DateTime DateOfBirthAd { get; set; }

    public int GenderId { get; set; }
    public int? IndexGroupId { get; set; }

    [MaxLength(5)]
    public string? BloodGroup { get; set; }

    [MaxLength(50)]
    public string? Nationality { get; set; }

    [MaxLength(50)]
    public string? Religion { get; set; }

    public int DistrictId { get; set; }

    [MaxLength(100)]
    public string? MunicipalityVdc { get; set; }

    [MaxLength(50)]
    public string? WardNumber { get; set; }

    public bool IsActive { get; set; }

    public int? StudentRegistrationIndex { get; set; }
    public int StudentCategoryId { get; set; }
    public int? VerifiedBy { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public int? PhotoAttachmentId { get; set; }
    public int? EthnicityId { get; set; }

    [MaxLength(50)]
    public string? EntranceRollNumber { get; set; }

    public int? EntryFormatId { get; set; }
    public bool? IsRegistrationNumberGenerated { get; set; }

    [MaxLength(50)]
    public string? RowIndex { get; set; }

    [MaxLength(50)]
    public string? PreviousAcademicYear { get; set; }

    [MaxLength(50)]
    public string? PreviousSymbolNumber { get; set; }

    public int? StudentRegistrationSearchId { get; set; }
    public int? LocalLevelId { get; set; }

    public int AcademicYearId { get; set; }
    public virtual AcademicYear AcademicYear { get; set; }

    [ForeignKey(nameof(LevelId))]
    [ValidateNever]
    public virtual Level Level { get; set; }

    [ForeignKey(nameof(FacultyId))]
    [ValidateNever]
    public virtual Faculty Faculty { get; set; }

    [ForeignKey(nameof(CollegeId))]
    [ValidateNever]
    public virtual College College { get; set; }

    [ForeignKey(nameof(GenderId))]
    [ValidateNever]
    public virtual Gender Gender { get; set; }

    [ForeignKey(nameof(DistrictId))]
    [ValidateNever]
    public virtual District District { get; set; }

    [ForeignKey(nameof(StudentCategoryId))]
    [ValidateNever]
    public virtual StudentCategory StudentCategory { get; set; }

    [ForeignKey(nameof(EthnicityId))]
    [ValidateNever]
    public virtual Ethnicity Ethnicity { get; set; }

    [ForeignKey(nameof(LocalLevelId))]
    [ValidateNever]
    public virtual LocalLevel LocalLevel { get; set; }

    [ForeignKey(nameof(IndexGroupId))]
    [ValidateNever]
    public virtual IndexGroup IndexGroup { get; set; }

    [ForeignKey(nameof(EntryFormatId))]
    [ValidateNever]
    public virtual EntryFormat EntryFormat { get; set; }

    [ForeignKey(nameof(PhotoAttachmentId))]
    [ValidateNever]
    public virtual UserAttachment PhotoAttachment { get; set; }

    [ForeignKey(nameof(StudentRegistrationSearchId))]
    [ValidateNever]
    public virtual StudentRegistrationSearch StudentRegistrationSearch { get; set; }

    [ValidateNever]
    public virtual ICollection<ApplicationVoucher> ApplicationVouchers { get; set; }
    [ValidateNever]
    public virtual ICollection<PaymentRequestLog> PaymentRequestLogs { get; set; }
    [ValidateNever]
    public virtual ICollection<StudentAdmission> StudentAdmissions { get; set; }
    [ValidateNever]
    public virtual ICollection<StudentGuardian> StudentGuardians { get; set; }
    [ValidateNever]
    public virtual ICollection<StudentQualification> StudentQualifications { get; set; }
    [ValidateNever]
    public virtual ICollection<AppUser> Users { get; set; }
}
