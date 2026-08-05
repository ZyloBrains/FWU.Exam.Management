using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities.Permissions;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Colleges.Controllers;

[Area("Colleges")]
[RequirePermission(Permissions.PermissionsManage)]
public class ManagePermissionsController(
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IPermissionService permissionService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var isSuperAdmin = await userManager.IsInRoleAsync(user, Role.SuperAdmin);
        var userPermissions = await permissionService.GetUserPermissionsAsync(user.Id);
        var userPermSet = new HashSet<string>(userPermissions);

        // Roles this user can manage (SuperAdmin sees all, CollegeAdmin sees FacultyAdmin + Student)
        var targetRoleNames = isSuperAdmin
            ? new[] { Role.FacultyAdmin, Role.CollegeAdmin, Role.Student }
            : new[] { Role.FacultyAdmin, Role.Student };

        var targetRoles = await roleManager.Roles
            .Where(r => targetRoleNames.Contains(r.Name ?? ""))
            .OrderBy(r => r.Name)
            .ToListAsync();

        var rolePermCounts = new Dictionary<string, int>();
        foreach (var role in targetRoles)
        {
            var assignedIds = await permissionService.GetRolePermissionIdsAsync(role.Id);
            var assignedPerms = await GetPermissionNamesAsync(assignedIds);
            rolePermCounts[role.Id] = assignedPerms.Count(p => userPermSet.Contains(p));
        }

        ViewBag.UserPermSet = userPermSet;
        ViewBag.RolePermCounts = rolePermCounts;
        ViewBag.IsSuperAdmin = isSuperAdmin;
        return View(targetRoles);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var role = await roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();

        var isSuperAdmin = await userManager.IsInRoleAsync(user, Role.SuperAdmin);
        var userPermissions = await permissionService.GetUserPermissionsAsync(user.Id);
        var userPermSet = new HashSet<string>(userPermissions);

        // Only allow managing FacultyAdmin or Student for CollegeAdmin
        if (!isSuperAdmin && role.Name != Role.FacultyAdmin && role.Name != Role.Student)
            return Forbid();

        var allPermissions = await permissionService.GetAllPermissionsAsync();
        var assignedIds = await permissionService.GetRolePermissionIdsAsync(role.Id);

        var groups = allPermissions
            .Where(p => isSuperAdmin || userPermSet.Contains(p.Name))
            .GroupBy(p => p.Group)
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

        ViewBag.IsSuperAdmin = isSuperAdmin;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, List<int> permissionIds)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var role = await roleManager.FindByIdAsync(id);
        if (role == null) return NotFound();

        var isSuperAdmin = await userManager.IsInRoleAsync(user, Role.SuperAdmin);

        if (!isSuperAdmin && role.Name != Role.FacultyAdmin && role.Name != Role.Student)
            return Forbid();

        // CollegeAdmin can only grant permissions they themselves have
        if (!isSuperAdmin)
        {
            var userPermissions = await permissionService.GetUserPermissionsAsync(user.Id);
            var userPermSet = new HashSet<string>(userPermissions);

            var allPerms = await permissionService.GetAllPermissionsAsync();
            var allowedIds = allPerms
                .Where(p => userPermSet.Contains(p.Name))
                .Select(p => p.Id)
                .ToHashSet();

            permissionIds = permissionIds.Where(pid => allowedIds.Contains(pid)).ToList();
        }

        await permissionService.UpdateRolePermissionsAsync(role.Id, permissionIds);

        TempData["Success"] = $"Permissions updated for role '{role.Name}'.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<string>> GetPermissionNamesAsync(List<int> ids)
    {
        if (ids.Count == 0) return [];
        var allPermissions = await permissionService.GetAllPermissionsAsync();
        return allPermissions.Where(p => ids.Contains(p.Id)).Select(p => p.Name).ToList();
    }

    private static string FormatGroupName(string group)
    {
        return string.Join(" ", System.Text.RegularExpressions.Regex
            .Split(group, @"(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])|_")
            .Select(w => char.ToUpper(w[0]) + w[1..]));
    }
}
