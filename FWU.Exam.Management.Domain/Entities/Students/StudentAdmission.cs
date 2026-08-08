using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Subjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities.Students;

public class StudentAdmission : IAuditable, ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Program")]
    public int ProgramsId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "College")]
    public int CollegeId { get; set; }

    [Range(1, int.MaxValue)]
    [Display(Name = "Academic Year")]
    public int AcademicYearId { get; set; }

    [Display(Name = "Admission Date")]
    public DateTime AdmissionDate { get; set; }

    [Display(Name = "Checked By")]
    public int? CheckedBy { get; set; }

    [Display(Name = "Is Completed")]
    public bool IsCompleted { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    [MaxLength(50)]
    [Display(Name = "College Roll Number")]
    public string? CollegeRollNumber { get; set; }

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

    [MaxLength(10)]
    [Display(Name = "Date of Birth (BS)")]
    public string? DateOfBirthBS { get; set; }

    [MaxLength(20)]
    [Display(Name = "Date of Birth (AD)")]
    public string? DateOfBirthAD { get; set; }

    [Display(Name = "Gender")]
    public int? GenderId { get; set; }

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

    [Display(Name = "Has Fee Exemption")]
    public bool HasFeeExemption { get; set; }

    [Display(Name = "App User")]
    public string? AppUserId { get; set; }

    public virtual Program? Program { get; set; }
    public virtual College? College { get; set; }
    public virtual AcademicYear? AcademicYear { get; set; }
    public virtual StudentRegistration? StudentRegistration { get; set; }
}
