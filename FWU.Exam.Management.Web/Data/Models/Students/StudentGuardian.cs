using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Data.Models.Students;

public class StudentGuardian
{
    [Key]
    public int StudentGuardianId { get; set; }

    public int StudentRegistrationId { get; set; }

    [Required, MaxLength(50)]
    public string FatherName { get; set; }

    [MaxLength(50)]
    public string? FatherContactNumber { get; set; }

    [MaxLength(50)]
    public string? FatherPhone { get; set; }

    [MaxLength(50)]
    public string? FatherEmail { get; set; }

    [MaxLength(50)]
    public string? FatherQualification { get; set; }

    [MaxLength(50)]
    public string? FatherProfession { get; set; }

    [MaxLength(100)]
    public string? FatherAddress { get; set; }

    [MaxLength(50)]
    public string? FatherOrganization { get; set; }

    [MaxLength(50)]
    public string? FatherOrganizationAddress { get; set; }

    [Required, MaxLength(50)]
    public string MotherName { get; set; }

    [MaxLength(50)]
    public string? MotherContactNumber { get; set; }

    [MaxLength(50)]
    public string? MotherPhone { get; set; }

    [MaxLength(50)]
    public string? MotherEmail { get; set; }

    [MaxLength(50)]
    public string? MotherQualification { get; set; }

    [MaxLength(50)]
    public string? MotherProfession { get; set; }

    [MaxLength(100)]
    public string? MotherAddress { get; set; }

    [MaxLength(50)]
    public string? MotherOrganization { get; set; }

    [MaxLength(50)]
    public string? MotherOrganizationAddress { get; set; }

    [Required, MaxLength(50)]
    public string GuardianName { get; set; }

    [MaxLength(50)]
    public string? GuardianContactNumber { get; set; }

    [MaxLength(50)]
    public string? GuardianPhone { get; set; }

    [MaxLength(50)]
    public string? GuardianEmail { get; set; }

    [MaxLength(50)]
    public string? GuardianQualification { get; set; }

    [MaxLength(50)]
    public string? GuardianProfession { get; set; }

    [MaxLength(100)]
    public string? GuardianAddress { get; set; }

    [MaxLength(50)]
    public string? GuardianOrganization { get; set; }

    [MaxLength(50)]
    public string? GuardianOrganizationAddress { get; set; }

    [MaxLength(50)]
    public string? RelationWithStudent { get; set; }

    [ForeignKey(nameof(StudentRegistrationId))]
    [ValidateNever]
    public virtual StudentRegistration StudentRegistration { get; set; }
}
