using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using FWU.Exam.Management.Domain.Entities.Permissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Admin.Controllers;

[Area("Admin")]
[RequirePermission(Permissions.PermissionsManage)]
public class RolePermissionManagerController(
    RoleManager<IdentityRole> roleManager,
    UserManager<AppUser> userManager,
    AppDbContext context,
    IPermissionService permissionService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var roles = await roleManager.Roles.OrderBy(r => r.Name).ToListAsync();

        var user = await userManager.GetUserAsync(User);
        var userRoles = user != null ? await userManager.GetRolesAsync(user) : [];
        var isSuperAdmin = userRoles.Any(r => r == "SuperAdmin" || r == "SystemAdmin");

        var allowedRoles = isSuperAdmin
            ? roles
            : roles.Where(r => r.Name != "SuperAdmin" && r.Name != "SystemAdmin" && r.Name != "CollegeAdmin" && r.Name != "Admin").ToList();

        var rolePermCounts = new Dictionary<string, int>();
        foreach (var role in allowedRoles)
        {
            var count = await context.RolePermissions!
                .CountAsync(rp => rp.RoleId == role.Id);
            rolePermCounts[role.Id] = count;
        }

        ViewBag.RolePermCounts = rolePermCounts;
        return View(allowedRoles);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();

        var user = await userManager.GetUserAsync(User);
        var userRoles = user != null ? await userManager.GetRolesAsync(user) : [];
        var isSuperAdmin = userRoles.Any(r => r == "SuperAdmin" || r == "SystemAdmin");

        if (!isSuperAdmin && (role.Name == "SuperAdmin" || role.Name == "SystemAdmin" || role.Name == "CollegeAdmin" || role.Name == "Admin"))
            return Forbid();

        var allPermissions = await permissionService.GetAllPermissionsAsync();
        var assignedIds = await permissionService.GetRolePermissionIdsAsync(role.Id);

        var userPermNames = isSuperAdmin
            ? null
            : await permissionService.GetUserPermissionsAsync(user!.Id);

        var filteredPermissions = isSuperAdmin
            ? allPermissions
            : allPermissions.Where(p => userPermNames!.Contains(p.Name)).ToList();

        var groups = filteredPermissions
            .GroupBy(p => p.Group)
            .Where(g => g.Any())
            .Select(g => new PermissionGroupViewModel
            {
                GroupName = g.Key,
                GroupDisplayName = FormatGroupName(g.Key),
                Permissions = g.Select(p => new PermissionItemViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    DisplayName = p.DisplayName ?? p.Name,
                    Description = p.Description,
                    IsAssigned = assignedIds.Contains(p.Id)
                }).ToList()
            })
            .ToList();

        var vm = new RolePermissionViewModel
        {
            RoleId = role.Id,
            RoleName = role.Name ?? "",
            Groups = groups
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, List<int> permissionIds)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();

        var user = await userManager.GetUserAsync(User);
        var userRoles = user != null ? await userManager.GetRolesAsync(user) : [];
        var isSuperAdmin = userRoles.Any(r => r == "SuperAdmin" || r == "SystemAdmin");

        if (!isSuperAdmin && (role.Name == "SuperAdmin" || role.Name == "SystemAdmin" || role.Name == "CollegeAdmin" || role.Name == "Admin"))
            return Forbid();

        if (!isSuperAdmin && permissionIds.Count > 0)
        {
            var userPermNames = await permissionService.GetUserPermissionsAsync(user!.Id);
            var allPerms = await permissionService.GetAllPermissionsAsync();
            var allowedIds = allPerms.Where(p => userPermNames.Contains(p.Name)).Select(p => p.Id).ToHashSet();
            permissionIds = permissionIds.Where(id => allowedIds.Contains(id)).ToList();
        }

        await permissionService.UpdateRolePermissionsAsync(role.Id, permissionIds);

        TempData["Success"] = $"Permissions updated for role '{role.Name}' successfully.";
        return RedirectToAction(nameof(Index));
    }

    private static string FormatGroupName(string group)
    {
        return string.Join(" ", System.Text.RegularExpressions.Regex
            .Split(group, @"(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])|_")
            .Select(w => char.ToUpper(w[0]) + w[1..]));
    }
}
