using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Students;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Colleges;
public class College
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? CollegeNameNepali { get; set; }

    [MaxLength(500)]
    public string? ShortName { get; set; }

    public DateTime? EstablishedDate { get; set; }
    public DateTime? ClosedDate { get; set; }

    [MaxLength(50)]
    public string? Website { get; set; }

    [MaxLength(50)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone1 { get; set; }

    [MaxLength(20)]
    public string? Phone2 { get; set; }

    [MaxLength(255)]
    public string? PrincipalName { get; set; }

    [MaxLength(50)]
    public string? PrincipalContactNumber { get; set; }

    [MaxLength(20)]
    public string? Fax { get; set; }

    [MaxLength(255)]
    public string? Remarks { get; set; }
    public bool IsExamCenterOnly { get; set; }
    public bool IsActive { get; set; }
    public decimal? AllocatedAmount { get; set; }
    public int? DisplayOrder { get; set; }

    public int? AddressId { get; set; }
    public virtual Address? Address { get; set; }

    public int? CollegeTypeId { get; set; }
    public virtual CollegeType? CollegeType { get; set; }

    public int? CollegeProfileId { get; set; }
    public virtual CollegeProfile? CollegeProfile { get; set; }

    public virtual ICollection<CollegeProgram>? CollegePrograms { get; set; }
    public virtual ICollection<ExamCenter>? ExamCenters { get; set; }
    public virtual ICollection<ExamRegistration>? ExamRegistrations { get; set; }
    public virtual ICollection<StudentAdmission>? StudentAdmissions { get; set; }
    public virtual ICollection<StudentRegistration>? StudentRegistrations { get; set; }
}
