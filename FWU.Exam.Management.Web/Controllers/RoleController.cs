using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Controllers;

[RequirePermission("roles.view")]
public class RoleController(RoleManager<IdentityRole> roleManager, IAuditLogWriter auditLogWriter) : Controller
{

    // GET: Role
    public async Task<IActionResult> Index()
    {
        return View(await roleManager.Roles.ToListAsync());
    }

    // GET: Role/Details/id
    public async Task<IActionResult> Details(string id)
    {
        if (id == null)
            return NotFound();

        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound();

        return View(role);
    }

    // GET: Role/Create
    [RequirePermission("roles.create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Role/Create
    [RequirePermission("roles.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            ModelState.AddModelError(string.Empty, "Role name is required.");
            return View();
        }

        var result = await roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
        if (result.Succeeded)
        {
            await auditLogWriter.LogAsync(ActivityTypes.RoleCreated,
                $"Created role {roleName.Trim()}",
                new { roleName = roleName.Trim() });
            TempData["SuccessMessage"] = "Role created successfully!";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View();
    }

    // GET: Role/Edit/id
    [RequirePermission("roles.edit")]
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null)
            return NotFound();

        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound();

        return View(role);
    }

    // POST: Role/Edit/id
    [RequirePermission("roles.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, string roleName)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(roleName))
        {
            ModelState.AddModelError(string.Empty, "Role name is required.");
            return View(role);
        }

        role.Name = roleName.Trim();
        var result = await roleManager.UpdateAsync(role);
        if (result.Succeeded)
        {
            await auditLogWriter.LogAsync(ActivityTypes.RoleUpdated,
                $"Updated role {role.Name}",
                new { roleId = role.Id, roleName = role.Name });
            TempData["SuccessMessage"] = "Role updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(role);
    }

    // GET: Role/Delete/id
    [RequirePermission("roles.delete")]
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null)
            return NotFound();

        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound();

        return View(role);
    }

    // POST: Role/Delete/id
    [RequirePermission("roles.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        try
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role != null)
            {
                await roleManager.DeleteAsync(role);
                await auditLogWriter.LogAsync(ActivityTypes.RoleDeleted,
                    $"Deleted role {role.Name}",
                    new { roleId = role.Id, roleName = role.Name });
            }

            TempData["SuccessMessage"] = "Role deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            TempData["ErrorMessage"] = "Cannot delete this record because it is referenced by other records. Please remove or reassign dependent records first.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while deleting: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
        [RequirePermission("roles.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(string id)
    {
        try
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role != null)
                await roleManager.DeleteAsync(role);
            return Json(new { success = true, message = "Role deleted successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

}
