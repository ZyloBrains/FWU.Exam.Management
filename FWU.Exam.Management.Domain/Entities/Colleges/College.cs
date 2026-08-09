using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Colleges;
public class College
{
    public int Id { get; set; }

    [Required, MaxLength(30)]
    [Display(Name = "Code")]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "College Name Nepali")]
    public string? CollegeNameNepali { get; set; }

    [MaxLength(500)]
    [Display(Name = "Short Name")]
    public string? ShortName { get; set; }

    [Display(Name = "Established Date")]
    public DateTime? EstablishedDate { get; set; }

    [Display(Name = "Closed Date")]
    public DateTime? ClosedDate { get; set; }

    [MaxLength(50)]
    [Display(Name = "Website")]
    public string? Website { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    [Display(Name = "Phone 1")]
    public string? Phone1 { get; set; }

    [MaxLength(20)]
    [Display(Name = "Phone 2")]
    public string? Phone2 { get; set; }

    [Required, MaxLength(255)]
    [Display(Name = "Principal Name")]
    public string PrincipalName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [Display(Name = "Principal Contact Number")]
    public string PrincipalContactNumber { get; set; } = string.Empty;

    [MaxLength(20)]
    [Display(Name = "Fax")]
    public string? Fax { get; set; }

    [MaxLength(255)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Exam Center Only")]
    public bool IsExamCenterOnly { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    [Display(Name = "Allocated Amount")]
    public decimal? AllocatedAmount { get; set; }

    [Display(Name = "Display Order")]
    public int? DisplayOrder { get; set; }

    [Display(Name = "Address")]
    public int? AddressId { get; set; }
    public virtual Address? Address { get; set; }

    [Display(Name = "College Type")]
    public int? CollegeTypeId { get; set; }
    public virtual CollegeType? CollegeType { get; set; }

    [Display(Name = "College Profile")]
    public int? CollegeProfileId { get; set; }
    public virtual CollegeProfile? CollegeProfile { get; set; }

    public virtual ICollection<CollegeFaculty> CollegeFaculties { get; set; } = [];
    public virtual ICollection<CollegeProgram> CollegePrograms { get; set; } = [];
    public virtual ICollection<ExamCenter> ExamCenters { get; set; } = [];
    public virtual ICollection<ExamRegistration> ExamRegistrations { get; set; } = [];
    public virtual ICollection<StudentAdmission> StudentAdmissions { get; set; } = [];
    public virtual ICollection<StudentRegistration> StudentRegistrations { get; set; } = [];
}

