using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.EntranceExams;

public class EntranceExamApplication : IAuditable
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

    [MaxLength(200)]
    public string? PreviousSchoolCollege { get; set; }

    public int? PreviousLevelId { get; set; }

    [MaxLength(10)]
    public string? PreviousPassedYear { get; set; }

    [MaxLength(50)]
    public string? PreviousSymbolNumber { get; set; }

    public decimal? PreviousGPA { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;

    public string? ReviewedBy { get; set; }

    public DateTime? ReviewDate { get; set; }

    [MaxLength(500)]
    public string? ReviewRemarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual AcademicYear? AcademicYear { get; set; }
    public virtual College? College { get; set; }
    public virtual Program? Program { get; set; }
    public virtual Gender? Gender { get; set; }
    public virtual Address? PermanentAddress { get; set; }
    public virtual PreviousLevel? PreviousLevel { get; set; }
}
