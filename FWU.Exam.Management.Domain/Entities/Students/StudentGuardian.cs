using FWU.Exam.Management.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace FWU.Exam.Management.Domain.Entities.Students;

public class StudentGuardian: IAuditable, ITenantScoped
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public virtual Tenant? Tenant { get; set; }

    [Display(Name = "Student Registration")]
    public int StudentRegistrationId { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Father Name")]
    public string? FatherName { get; set; }

    [MaxLength(50)]
    [Display(Name = "Father Contact Number")]
    public string? FatherContactNumber { get; set; }

    [MaxLength(50)]
    [Display(Name = "Father Phone")]
    public string? FatherPhone { get; set; }

    [MaxLength(50)]
    [Display(Name = "Father Email")]
    public string? FatherEmail { get; set; }

    [MaxLength(50)]
    [Display(Name = "Father Qualification")]
    public string? FatherQualification { get; set; }

    [MaxLength(50)]
    [Display(Name = "Father Profession")]
    public string? FatherProfession { get; set; }

    [MaxLength(100)]
    [Display(Name = "Father Address")]
    public string? FatherAddress { get; set; }

    [MaxLength(50)]
    [Display(Name = "Father Organization")]
    public string? FatherOrganization { get; set; }

    [MaxLength(50)]
    [Display(Name = "Father Organization Address")]
    public string? FatherOrganizationAddress { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Mother Name")]
    public string? MotherName { get; set; }

    [MaxLength(50)]
    [Display(Name = "Mother Contact Number")]
    public string? MotherContactNumber { get; set; }

    [MaxLength(50)]
    [Display(Name = "Mother Phone")]
    public string? MotherPhone { get; set; }

    [MaxLength(50)]
    [Display(Name = "Mother Email")]
    public string? MotherEmail { get; set; }

    [MaxLength(50)]
    [Display(Name = "Mother Qualification")]
    public string? MotherQualification { get; set; }

    [MaxLength(50)]
    [Display(Name = "Mother Profession")]
    public string? MotherProfession { get; set; }

    [MaxLength(100)]
    [Display(Name = "Mother Address")]
    public string? MotherAddress { get; set; }

    [MaxLength(50)]
    [Display(Name = "Mother Organization")]
    public string? MotherOrganization { get; set; }

    [MaxLength(50)]
    [Display(Name = "Mother Organization Address")]
    public string? MotherOrganizationAddress { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Guardian Name")]
    public string? GuardianName { get; set; }

    [MaxLength(50)]
    [Display(Name = "Guardian Contact Number")]
    public string? GuardianContactNumber { get; set; }

    [MaxLength(50)]
    [Display(Name = "Guardian Phone")]
    public string? GuardianPhone { get; set; }

    [MaxLength(50)]
    [Display(Name = "Guardian Email")]
    public string? GuardianEmail { get; set; }

    [MaxLength(50)]
    [Display(Name = "Guardian Qualification")]
    public string? GuardianQualification { get; set; }

    [MaxLength(50)]
    [Display(Name = "Guardian Profession")]
    public string? GuardianProfession { get; set; }

    [MaxLength(100)]
    [Display(Name = "Guardian Address")]
    public string? GuardianAddress { get; set; }

    [MaxLength(50)]
    [Display(Name = "Guardian Organization")]
    public string? GuardianOrganization { get; set; }

    [MaxLength(50)]
    [Display(Name = "Guardian Organization Address")]
    public string? GuardianOrganizationAddress { get; set; }

    [MaxLength(50)]
    [Display(Name = "Relation With Student")]
    public string? RelationWithStudent { get; set; }

    public virtual StudentRegistration? StudentRegistration { get; set; }
}
