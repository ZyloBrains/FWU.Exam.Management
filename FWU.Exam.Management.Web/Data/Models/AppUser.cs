using fwu_examination_management_system.Data.Auditing;
using fwu_examination_management_system.Data.Models.Colleges;
using fwu_examination_management_system.Data.Models.Students;
using Microsoft.AspNetCore.Identity;

namespace fwu_examination_management_system.Data.Models;
public class AppUser: IdentityUser, IAuditable
{ 
    public  string? ProfilePath { get; set; }

    // Fields from old User class
    public string? FullName { get; set; }
    public string? Designation { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Remarks { get; set; }

    public int? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public int? StudentRegistrationId { get; set; }
    public virtual StudentRegistration? StudentRegistration { get; set; }

    public virtual ICollection<Program>? Programs { get; set; }

    public int? CollegeId { get; set; }
    public virtual College? College { get; set; }
}
