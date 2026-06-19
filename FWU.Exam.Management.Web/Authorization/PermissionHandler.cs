using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace FWU.Exam.Management.Web.Authorization;

public class PermissionHandler(IPermissionService permissionService, UserManager<AppUser> userManager)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        // SuperAdmin bypass: SuperAdmin has all permissions without checking the database
        if (context.User.IsInRole("SuperAdmin"))
        {
            context.Succeed(requirement);
            return;
        }

        var userId = userManager.GetUserId(context.User);
        if (string.IsNullOrEmpty(userId))
            return;

        var hasPermission = await permissionService.HasPermissionAsync(userId, requirement.Permission);
        if (hasPermission)
            context.Succeed(requirement);
    }
}
