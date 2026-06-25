using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class EntranceExamApplication : IAuditable, ITenantScoped
{
    public int Id { get; set; }

    public int AcademicYearId { get; set; }
    public int CollegeId { get; set; }
    public int ProgramId { get; set; }

    [Required, MaxLength(80)]
    public string? FirstName { get; set; }

    [MaxLength(30)]
    public string? MiddleName { get; set; }

    [Required, MaxLength(30)]
    public string? LastName { get; set; }

    [MaxLength(100)]
    public string? NepaliName { get; set; }

    [Required, MaxLength(10)]
    public string? DateOfBirthBS { get; set; }

    public string? DateOfBirthAD { get; set; }

    public int GenderId { get; set; }

    [EmailAddress, MaxLength(50)]
    public string? Email { get; set; }

    [MaxLength(15)]
    public string? ContactNumber { get; set; }

    [MaxLength(15)]
    public string? Phone { get; set; }

    public int? PermanentAddressId { get; set; }

    [MaxLength(100)]
    public string? FatherName { get; set; }

    [MaxLength(15)]
    public string? FatherContact { get; set; }

    [MaxLength(100)]
    public string? MotherName { get; set; }

    [MaxLength(15)]
    public string? MotherContact { get; set; }

    [MaxLength(100)]
    public string? GuardianEmail { get; set; }

    [MaxLength(100)]
    public string? FatherProfession { get; set; }

    [MaxLength(100)]
    public string? MotherProfession { get; set; }

    [MaxLength(50)]
    public string? CitizenshipNo { get; set; }

    public int? CitizenshipDistrictId { get; set; }

    [MaxLength(10)]
    public string? CitizenshipIssueDateBs { get; set; }

    public string? CitizenshipIssueDateAd { get; set; }

    [MaxLength(5)]
    public string? BloodGroup { get; set; }

    [MaxLength(100)]
    public string? BirthPlace { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(500)]
    public string? PhotoPath { get; set; }

    [MaxLength(500)]
    public string? DocumentsPath { get; set; }

    [MaxLength(500)]
    public string? VoucherPath { get; set; }

    [MaxLength(200)]
    public string? PreviousSchoolCollege { get; set; }

    public int? PreviousLevelId { get; set; }

    [MaxLength(10)]
    public string? PreviousPassedYear { get; set; }

    [MaxLength(50)]
    public string? PreviousSymbolNumber { get; set; }

    public decimal? PreviousGPA { get; set; }

    [MaxLength(10)]
    public string? PreviousDivision { get; set; }

    public int? PreviousLevel2Id { get; set; }

    [MaxLength(200)]
    public string? PreviousSchoolCollege2 { get; set; }

    [MaxLength(50)]
    public string? PreviousBoard2 { get; set; }

    [MaxLength(50)]
    public string? PreviousSymbolNumber2 { get; set; }

    [MaxLength(10)]
    public string? PreviousPassedYear2 { get; set; }

    public decimal? PreviousGPA2 { get; set; }

    [MaxLength(10)]
    public string? PreviousDivision2 { get; set; }

    public int? PreviousLevel3Id { get; set; }

    [MaxLength(200)]
    public string? PreviousSchoolCollege3 { get; set; }

    [MaxLength(50)]
    public string? PreviousBoard3 { get; set; }

    [MaxLength(50)]
    public string? PreviousSymbolNumber3 { get; set; }

    [MaxLength(10)]
    public string? PreviousPassedYear3 { get; set; }

    public decimal? PreviousGPA3 { get; set; }

    [MaxLength(10)]
    public string? PreviousDivision3 { get; set; }

    public int? ApplicationVoucherId { get; set; }
    public bool PaymentVerified { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;

    public string? ReviewedBy { get; set; }

    public DateTime? ReviewDate { get; set; }

    [MaxLength(500)]
    public string? ReviewRemarks { get; set; }

    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual AcademicYear? AcademicYear { get; set; }
    public virtual College? College { get; set; }
    public virtual Program? Program { get; set; }
    public virtual Gender? Gender { get; set; }
    public virtual Address? PermanentAddress { get; set; }
    public virtual PreviousLevel? PreviousLevel { get; set; }
    public virtual District? CitizenshipDistrict { get; set; }
    public virtual PreviousLevel? PreviousLevel2 { get; set; }
    public virtual PreviousLevel? PreviousLevel3 { get; set; }
    public virtual ApplicationVoucher? ApplicationVoucher { get; set; }
}
