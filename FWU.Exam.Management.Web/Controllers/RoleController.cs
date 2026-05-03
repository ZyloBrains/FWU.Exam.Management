using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Controllers;

public class RoleController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleController(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
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

        var result = await _roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
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

        if (string.IsNullOrWhiteSpace(roleName))
        {
            ModelState.AddModelError(string.Empty, "Role name is required.");
            return View(role);
        }

        role.Name = roleName.Trim();
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
        if (role != null)
            await _roleManager.DeleteAsync(role);

        return RedirectToAction(nameof(Index));
    }
}
