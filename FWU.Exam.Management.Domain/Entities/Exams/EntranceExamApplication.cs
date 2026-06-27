using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities.Exams;

public class EntranceExamApplication : IAuditable, ITenantScoped
{
    public int Id { get; set; }

    [Display(Name = "Academic Year")]
    public int AcademicYearId { get; set; }

    [Display(Name = "College")]
    public int CollegeId { get; set; }

    [Display(Name = "Program")]
    public int ProgramId { get; set; }

    [Required, MaxLength(80)]
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [MaxLength(30)]
    [Display(Name = "Middle Name")]
    public string? MiddleName { get; set; }

    [Required, MaxLength(30)]
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [MaxLength(100)]
    [Display(Name = "Nepali Name")]
    public string? NepaliName { get; set; }

    [Required, MaxLength(10)]
    [Display(Name = "Date of Birth (BS)")]
    public string? DateOfBirthBS { get; set; }

    [Display(Name = "Date of Birth (AD)")]
    public string? DateOfBirthAD { get; set; }

    [Display(Name = "Gender")]
    public int GenderId { get; set; }

    [EmailAddress, MaxLength(50)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [MaxLength(15)]
    [Display(Name = "Contact Number")]
    public string? ContactNumber { get; set; }

    [MaxLength(15)]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [Display(Name = "Permanent Address")]
    public int? PermanentAddressId { get; set; }

    [MaxLength(100)]
    [Display(Name = "Father Name")]
    public string? FatherName { get; set; }

    [MaxLength(15)]
    [Display(Name = "Father Contact")]
    public string? FatherContact { get; set; }

    [MaxLength(100)]
    [Display(Name = "Mother Name")]
    public string? MotherName { get; set; }

    [MaxLength(15)]
    [Display(Name = "Mother Contact")]
    public string? MotherContact { get; set; }

    [MaxLength(100)]
    [Display(Name = "Guardian Email")]
    public string? GuardianEmail { get; set; }

    [MaxLength(100)]
    [Display(Name = "Father Profession")]
    public string? FatherProfession { get; set; }

    [MaxLength(100)]
    [Display(Name = "Mother Profession")]
    public string? MotherProfession { get; set; }

    [MaxLength(50)]
    [Display(Name = "Citizenship No")]
    public string? CitizenshipNo { get; set; }

    [Display(Name = "Citizenship District")]
    public int? CitizenshipDistrictId { get; set; }

    [MaxLength(10)]
    [Display(Name = "Citizenship Issue Date (BS)")]
    public string? CitizenshipIssueDateBs { get; set; }

    [Display(Name = "Citizenship Issue Date (AD)")]
    public string? CitizenshipIssueDateAd { get; set; }

    [MaxLength(5)]
    [Display(Name = "Blood Group")]
    public string? BloodGroup { get; set; }

    [MaxLength(100)]
    [Display(Name = "Birth Place")]
    public string? BirthPlace { get; set; }

    [MaxLength(100)]
    [Display(Name = "Country")]
    public string? Country { get; set; }

    [MaxLength(20)]
    [Display(Name = "Postal Code")]
    public string? PostalCode { get; set; }

    [MaxLength(500)]
    [Display(Name = "Photo Path")]
    public string? PhotoPath { get; set; }

    [MaxLength(500)]
    [Display(Name = "Documents Path")]
    public string? DocumentsPath { get; set; }

    [MaxLength(500)]
    [Display(Name = "Voucher Path")]
    public string? VoucherPath { get; set; }

    [MaxLength(200)]
    [Display(Name = "Previous School/College")]
    public string? PreviousSchoolCollege { get; set; }

    [Display(Name = "Previous Level")]
    public int? PreviousLevelId { get; set; }

    [MaxLength(10)]
    [Display(Name = "Previous Passed Year")]
    public string? PreviousPassedYear { get; set; }

    [MaxLength(50)]
    [Display(Name = "Previous Symbol Number")]
    public string? PreviousSymbolNumber { get; set; }

    [Display(Name = "Previous GPA")]
    public decimal? PreviousGPA { get; set; }

    [MaxLength(10)]
    [Display(Name = "Previous Division")]
    public string? PreviousDivision { get; set; }

    [Display(Name = "Previous Level 2")]
    public int? PreviousLevel2Id { get; set; }

    [MaxLength(200)]
    [Display(Name = "Previous School/College 2")]
    public string? PreviousSchoolCollege2 { get; set; }

    [MaxLength(50)]
    [Display(Name = "Previous Board 2")]
    public string? PreviousBoard2 { get; set; }

    [MaxLength(50)]
    [Display(Name = "Previous Symbol Number 2")]
    public string? PreviousSymbolNumber2 { get; set; }

    [MaxLength(10)]
    [Display(Name = "Previous Passed Year 2")]
    public string? PreviousPassedYear2 { get; set; }

    [Display(Name = "Previous GPA 2")]
    public decimal? PreviousGPA2 { get; set; }

    [MaxLength(10)]
    [Display(Name = "Previous Division 2")]
    public string? PreviousDivision2 { get; set; }

    [Display(Name = "Previous Level 3")]
    public int? PreviousLevel3Id { get; set; }

    [MaxLength(200)]
    [Display(Name = "Previous School/College 3")]
    public string? PreviousSchoolCollege3 { get; set; }

    [MaxLength(50)]
    [Display(Name = "Previous Board 3")]
    public string? PreviousBoard3 { get; set; }

    [MaxLength(50)]
    [Display(Name = "Previous Symbol Number 3")]
    public string? PreviousSymbolNumber3 { get; set; }

    [MaxLength(10)]
    [Display(Name = "Previous Passed Year 3")]
    public string? PreviousPassedYear3 { get; set; }

    [Display(Name = "Previous GPA 3")]
    public decimal? PreviousGPA3 { get; set; }

    [MaxLength(10)]
    [Display(Name = "Previous Division 3")]
    public string? PreviousDivision3 { get; set; }

    [Display(Name = "Application Voucher")]
    public int? ApplicationVoucherId { get; set; }

    [Display(Name = "Payment Verified")]
    public bool PaymentVerified { get; set; }

    [Display(Name = "Status")]
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;

    [Display(Name = "Reviewed By")]
    public string? ReviewedBy { get; set; }

    [Display(Name = "Review Date")]
    public DateTime? ReviewDate { get; set; }

    [MaxLength(500)]
    [Display(Name = "Review Remarks")]
    public string? ReviewRemarks { get; set; }

    [Display(Name = "Tenant")]
    public int TenantId { get; set; }

    public virtual Tenant? Tenant { get; set; }

    [Display(Name = "Created At")]
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
