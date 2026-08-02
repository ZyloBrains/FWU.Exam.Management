using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities.Students;

public class StudentRegistration : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Level")]
    public int LevelId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "College")]
    public int CollegeId { get; set; }

    [Display(Name = "Faculty")]
    public int? FacultyId { get; set; }

    [Display(Name = "Program")]
    public int? ProgramId { get; set; }

    [Display(Name = "Registration Number")]
    public string? RegistrationNumber { get; set; }

    [Required, MaxLength(80)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(30)]
    [Display(Name = "Middle Name")]
    public string? MiddleName { get; set; }

    [Required, MaxLength(30)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(100)]
    [Display(Name = "Nepali Name")]
    public string? NepaliName { get; set; }

    [MaxLength(15)]
    [Display(Name = "Contact Number")]
    public string? ContactNumber { get; set; }

    [MaxLength(15)]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(50)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [MaxLength(10)]
    [Display(Name = "Date of Birth (BS)")]
    public string DateOfBirthBS { get; set; } = string.Empty;

    [Display(Name = "Date of Birth (AD)")]
    public string? DateOfBirthAD { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Gender")]
    public int GenderId { get; set; }

    [MaxLength(5)]
    [Display(Name = "Blood Group")]
    public string? BloodGroup { get; set; }

    [MaxLength(50)]
    [Display(Name = "Nationality")]
    public string? Nationality { get; set; }

    [MaxLength(50)]
    [Display(Name = "Religion")]
    public string? Religion { get; set; }

    [Display(Name = "Permanent Address")]
    public int? PermanentAddressId { get; set; }

    [Display(Name = "Current Address")]
    public int? CurrentAddressId { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Student Category")]
    public int StudentCategoryId { get; set; }

    [Display(Name = "Verified By")]
    public int? VerifiedBy { get; set; }

    [Display(Name = "Verified Date")]
    public DateTime? VerifiedDate { get; set; }

    [Display(Name = "Ethnicity")]
    public int? EthnicityId { get; set; }

    [MaxLength(50)]
    [Display(Name = "Entrance Roll Number")]
    public string? EntranceRollNumber { get; set; }

    [Display(Name = "Is Registration Number Generated")]
    public bool? IsRegistrationNumberGenerated { get; set; }

    [Display(Name = "Registration Index")]
    public int? StudentRegistrationIndex { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Academic Year")]
    public int AcademicYearId { get; set; }
    public virtual AcademicYear? AcademicYear { get; set; }

    [Display(Name = "Student Admission")]
    public int? StudentAdmissionId { get; set; }

    public virtual Level? Level { get; set; }
    public virtual Faculty? Faculty { get; set; }
    public virtual College? College { get; set; }
    public virtual Program? Program { get; set; }
    public virtual Gender? Gender { get; set; }
    public virtual StudentCategory? StudentCategory { get; set; }
    public virtual Ethnicity? Ethnicity { get; set; }
    public virtual Address? PermanentAddress { get; set; }
    public virtual Address? CurrentAddress { get; set; }
    public virtual StudentAdmission? StudentAdmission { get; set; }
    public virtual ICollection<ApplicationVoucher> ApplicationVouchers { get; set; } = [];
    public virtual ICollection<PaymentRequestLog> PaymentRequestLogs { get; set; } = [];
    public virtual ICollection<StudentGuardian> StudentGuardians { get; set; } = [];
    public virtual ICollection<StudentQualification> StudentQualifications { get; set; } = [];
}
