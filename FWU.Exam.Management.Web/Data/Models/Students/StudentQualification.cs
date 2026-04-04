using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models.Students;

public class StudentQualification
{
    public int Id { get; set; }

    public int StudentRegistrationId { get; set; }
    public int BoardId { get; set; }
    public int PreviousLevelId { get; set; }

    [MaxLength(255)]
    public string? ProgramName { get; set; }

    [Required, MaxLength(255)]
    public string? InstituteName { get; set; }

    [MaxLength(50)]
    public string? PassedYear { get; set; }

    [MaxLength(255)]
    public string? Specialization { get; set; }

    public decimal? Percentage { get; set; }

    [MaxLength(50)]
    public string? TotalCredits { get; set; }

    [MaxLength(50)]
    public string? Remarks { get; set; }

    public bool IsHigherDegree { get; set; }
    public bool IsActive { get; set; }

    [MaxLength(500)]
    public string? ExamRollNumber { get; set; }

    public virtual StudentRegistration? StudentRegistration { get; set; }

    public virtual Board? Board { get; set; }

    public virtual PreviousLevel? PreviousLevel { get; set; }
}
