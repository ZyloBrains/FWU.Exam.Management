using Microsoft.AspNetCore.Authorization;

namespace FWU.Exam.Management.Web.Authorization;

public class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission_";

    public RequirePermissionAttribute(string permission)
    {
        Permission = permission;
        Policy = $"{PolicyPrefix}{permission}";
    }

    public string Permission { get; }
}
