using fwu_examination_management_system.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fwu_examination_management_system.Controllers;

[Authorize(Roles = "SystemAdmin")]
public class RoleController : Controller
{
    private static readonly HashSet<string> ProtectedRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "SystemAdmin",
        "Admin",
        "CollegeAdmin",
        "ReportAdmin",
        "Student"
    };

    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;

    public RoleController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    // GET: Role
    public async Task<IActionResult> Index()
    {
        return View(await _roleManager.Roles.ToListAsync());
    }

    // GET: Role/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Role/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            ModelState.AddModelError(string.Empty, "Role name is required.");
            return View();
        }

        roleName = roleName.Trim();

        if (ProtectedRoles.Contains(roleName))
        {
            ModelState.AddModelError(string.Empty, "This is a built-in role and cannot be created manually.");
            return View();
        }

        if (await _roleManager.RoleExistsAsync(roleName))
        {
            ModelState.AddModelError(string.Empty, "Role already exists.");
            return View();
        }

        var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
        if (result.Succeeded)
            return RedirectToAction(nameof(Index));

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View();
    }

    // GET: Role/Edit/id
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null)
            return NotFound();

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound();

        return View(role);
    }

    // POST: Role/Edit/id
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, string roleName)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound();

        if (ProtectedRoles.Contains(role.Name ?? string.Empty))
        {
            ModelState.AddModelError(string.Empty, "Built-in roles cannot be renamed.");
            return View(role);
        }

        if (string.IsNullOrWhiteSpace(roleName))
        {
            ModelState.AddModelError(string.Empty, "Role name is required.");
            return View(role);
        }

        roleName = roleName.Trim();

        if (ProtectedRoles.Contains(roleName))
        {
            ModelState.AddModelError(string.Empty, "This is a built-in role name and cannot be used for custom roles.");
            return View(role);
        }

        var existingRole = await _roleManager.FindByNameAsync(roleName);
        if (existingRole != null && existingRole.Id != role.Id)
        {
            ModelState.AddModelError(string.Empty, "Role name already exists.");
            return View(role);
        }

        role.Name = roleName;
        var result = await _roleManager.UpdateAsync(role);
        if (result.Succeeded)
            return RedirectToAction(nameof(Index));

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(role);
    }

    // GET: Role/Delete/id
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null)
            return NotFound();

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound();

        return View(role);
    }

    // POST: Role/Delete/id
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return RedirectToAction(nameof(Index));

        if (ProtectedRoles.Contains(role.Name ?? string.Empty))
        {
            TempData["Error"] = "Built-in roles cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        if (usersInRole.Any())
        {
            TempData["Error"] = $"Role '{role.Name}' cannot be deleted while users are assigned to it.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Index));
    }
}
