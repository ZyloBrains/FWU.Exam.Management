using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Students;

public class StudentRegistration : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    public int LevelId { get; set; }
    public int DepartmentId { get; set; }
    public int CollegeId { get; set; }
    public int? FacultyId { get; set; }
    public int? ProgramId { get; set; }

    [MaxLength(50)]
    public string? RegistrationNumber { get; set; }

    [Required, MaxLength(80)]
    public string? FirstName { get; set; }

    [MaxLength(30)]
    public string? MiddleName { get; set; }

    [Required, MaxLength(30)]
    public string? LastName { get; set; }

    [MaxLength(100)]
    public string? NepaliName { get; set; }

    [MaxLength(15)]
    public string? ContactNumber { get; set; }

    [MaxLength(15)]
    public string? Phone { get; set; }
    [EmailAddress]
    [MaxLength(50)]
    public string? Email { get; set; }

    [MaxLength(10)]
    public string DateOfBirthBS { get; set; } = string.Empty;
    public string? DateOfBirthAD { get; set; }

    public int GenderId { get; set; }

    [MaxLength(5)]
    public string? BloodGroup { get; set; }

    [MaxLength(50)]
    public string? Nationality { get; set; }

    [MaxLength(50)]
    public string? Religion { get; set; }
    public int? PermanentAddressId { get; set; }
    public int? CurrentAddressId { get; set; }
    public bool IsActive { get; set; }
    public int StudentCategoryId { get; set; }
    public int? VerifiedBy { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public int? EthnicityId { get; set; }

    [MaxLength(50)]
    public string? EntranceRollNumber { get; set; }
    public bool? IsRegistrationNumberGenerated { get; set; }


    public int AcademicYearId { get; set; }
    public virtual AcademicYear? AcademicYear { get; set; }

    public virtual Level? Level { get; set; }
    public virtual Department? Department { get; set; }
    public virtual Faculty? Faculty { get; set; }
    public virtual College? College { get; set; }
    public virtual Program? Program { get; set; }
    public virtual Gender? Gender { get; set; }
    public virtual StudentCategory? StudentCategory { get; set; }
    public virtual Ethnicity? Ethnicity { get; set; }
    public virtual Address? PermanentAddress { get; set; }
    public virtual Address? CurrentAddress { get; set; }
    public virtual ICollection<ApplicationVoucher>? ApplicationVouchers { get; set; }
    public virtual ICollection<PaymentRequestLog>? PaymentRequestLogs { get; set; }
    public virtual ICollection<StudentAdmission>? StudentAdmissions { get; set; }
    public virtual ICollection<StudentGuardian>? StudentGuardians { get; set; }
    public virtual ICollection<StudentQualification>? StudentQualifications { get; set; }
}
