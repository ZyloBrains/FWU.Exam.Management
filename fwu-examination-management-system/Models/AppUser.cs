using Microsoft.AspNetCore.Identity;

namespace fwu_examination_management_system.Models;
public class AppUser: IdentityUser
{
    public int OrganizationId { get; set; }
    public Organization? Organization { get; set; }
}
