using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace fwu_examination_management_system.Models;
public class AppUser: IdentityUser
{
    public  string? ProfilePath { get; set; }
    public int? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    // Fields from old User class
    public string? FullName { get; set; }
    public string? Designation { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? NtUser { get; set; }
    public string? Remarks { get; set; }
    public bool Active { get; set; }          // consider merging with IsActive
    public int CreatedBy { get; set; }         // this may refer to another user – keep as int if not a FK
    public DateTime CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public int? PasswordResetLogId { get; set; }
    public string? ContactNumber { get; set; } // Identity already has PhoneNumber
    public int? StudentRegistrationId { get; set; }
    public DateTime? LastPasswordChanged { get; set; }

    // Navigation properties
    [ForeignKey(nameof(StudentRegistrationId))]
    public virtual StudentRegistration? StudentRegistration { get; set; }

    [ForeignKey(nameof(PasswordResetLogId))]
    public virtual PasswordResetLog? PasswordResetLog { get; set; }

    public virtual ICollection<UserProgramMap> UserProgramMaps { get; set; }

    public int? CollegeId { get; set; }
    [ForeignKey(nameof(CollegeId))]
    public virtual College? College { get; set; }

    public virtual ICollection<PasswordResetLog> PasswordResetLogs { get; set; } = new List<PasswordResetLog>();


}
