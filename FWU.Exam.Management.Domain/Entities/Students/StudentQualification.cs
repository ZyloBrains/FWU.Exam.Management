using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities.Students;

public class StudentQualification : ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Display(Name = "Student Registration")]
    public int StudentRegistrationId { get; set; }

    [Display(Name = "Board")]
    public int BoardId { get; set; }

    [Display(Name = "Previous Level")]
    public int PreviousLevelId { get; set; }

    [MaxLength(255)]
    [Display(Name = "Program Name")]
    public string? ProgramName { get; set; }

    [Required, MaxLength(255)]
    [Display(Name = "Institute Name")]
    public string? InstituteName { get; set; }

    [MaxLength(50)]
    [Display(Name = "Passed Year")]
    public string? PassedYear { get; set; }

    [MaxLength(255)]
    [Display(Name = "Specialization")]
    public string? Specialization { get; set; }

    [Display(Name = "Percentage")]
    public decimal? Percentage { get; set; }

    [MaxLength(50)]
    [Display(Name = "Total Credits")]
    public string? TotalCredits { get; set; }

    [MaxLength(50)]
    [Display(Name = "Remarks")]
    public string? Remarks { get; set; }

    [Display(Name = "Is Higher Degree")]
    public bool IsHigherDegree { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    [MaxLength(500)]
    [Display(Name = "Document Path")]
    public string? DocumentPath { get; set; }

    [MaxLength(500)]
    [Display(Name = "Exam Roll Number")]
    public string? ExamRollNumber { get; set; }

    public virtual StudentRegistration? StudentRegistration { get; set; }

    public virtual Board? Board { get; set; }

    public virtual PreviousLevel? PreviousLevel { get; set; }
}
