using Microsoft.AspNetCore.Authorization;

namespace FWU.Exam.Management.Web.Authorization;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
