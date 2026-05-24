using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Controllers;

[Authorize]
[Route("tenant/{tenantCode}")]
public class TenantDashboardController(
    ITenantService tenantService,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager) : Controller
{
    private static readonly string[] RolesRequiringStudent = ["SystemAdmin", "Admin", "Tenant"];

    private async Task<Tenant?> GetTenantAsync(string tenantCode) =>
        await tenantService.GetTenantByOfficeCodeAsync(tenantCode);

    private async Task<(Tenant? Tenant, IActionResult? DeniedResult)> GetAuthorizedTenantAsync(string tenantCode)
    {
        var tenant = await GetTenantAsync(tenantCode);
        if (tenant == null)
            return (null, NotFound());

        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
            return (null, Challenge());

        if (User.IsInRole(Role.SystemAdmin) || currentUser.TenantId == tenant.Id)
            return (tenant, null);

        return (null, Forbid());
    }

    // GET: /tenant/{tenantCode}
    [HttpGet("")]
    public async Task<IActionResult> Index(string tenantCode)
    {
        var auth = await GetAuthorizedTenantAsync(tenantCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var tenant = auth.Tenant!;
        ViewBag.Tenant = tenant;
        ViewBag.UserCount = await userManager.Users.CountAsync(u => u.TenantId == tenant.Id);
        return View("~/Views/Home/Index.cshtml");
    }

    // GET: /tenant/{tenantCode}/users
    [HttpGet("users")]
    public async Task<IActionResult> Users(string tenantCode)
    {
        var auth = await GetAuthorizedTenantAsync(tenantCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var tenant = auth.Tenant!;
        ViewBag.Tenant = tenant;

        var users = await userManager.Users
            .Where(u => u.TenantId == tenant.Id)
            .ToListAsync();

        var model = new List<UserListItemViewModel>();
        foreach (var user in users)
        {
            model.Add(new UserListItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                TenantName = tenant.Name,
                Roles = await userManager.GetRolesAsync(user)
            });
        }
        return View(model);
    }

    // GET: /tenant/{tenantCode}/users/create
    [HttpGet("users/create")]
    public async Task<IActionResult> CreateUser(string tenantCode)
    {
        var auth = await GetAuthorizedTenantAsync(tenantCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        ViewBag.Tenant = auth.Tenant!;
        return View(new CreateUserViewModel());
    }

    // POST: /tenant/{tenantCode}/users/create
    [HttpPost("users/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(string tenantCode, CreateUserViewModel model)
    {
        var auth = await GetAuthorizedTenantAsync(tenantCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var tenant = auth.Tenant!;

        if (ModelState.IsValid)
        {
            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                TenantId = tenant.Id,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (await roleManager.RoleExistsAsync("Student") && !await userManager.IsInRoleAsync(user, "Student"))
                    await userManager.AddToRoleAsync(user, "Student");

                return RedirectToAction(nameof(Users), new { tenantCode });
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        ViewBag.Tenant = tenant;
        return View(model);
    }

    // GET: /tenant/{tenantCode}/users/{userId}/edit
    [HttpGet("users/{userId}/edit")]
    public async Task<IActionResult> EditUser(string tenantCode, string userId)
    {
        var auth = await GetAuthorizedTenantAsync(tenantCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var tenant = auth.Tenant!;
        var user = await userManager.FindByIdAsync(userId);
        if (user == null || user.TenantId != tenant.Id) return NotFound();

        ViewBag.Tenant = tenant;
        return View(new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            TenantId = tenant.Id
        });
    }

    // POST: /tenant/{tenantCode}/users/{userId}/edit
    [HttpPost("users/{userId}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(string tenantCode, string userId, EditUserViewModel model)
    {
        var auth = await GetAuthorizedTenantAsync(tenantCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var tenant = auth.Tenant!;

        if (userId != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null || user.TenantId != tenant.Id) return NotFound();

            user.Email = model.Email;
            user.UserName = model.Email;

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
                return RedirectToAction(nameof(Users), new { tenantCode });

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        ViewBag.Tenant = tenant;
        return View(model);
    }

    // GET: /tenant/{tenantCode}/users/{userId}/delete
    [HttpGet("users/{userId}/delete")]
    public async Task<IActionResult> DeleteUser(string tenantCode, string userId)
    {
        var auth = await GetAuthorizedTenantAsync(tenantCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var tenant = auth.Tenant!;

        var user = await userManager.FindByIdAsync(userId);
        if (user == null || user.TenantId != tenant.Id) return NotFound();

        ViewBag.Tenant = tenant;
        return View(user);
    }

    // POST: /tenant/{tenantCode}/users/{userId}/delete
    [HttpPost("users/{userId}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(string tenantCode, string userId)
    {
        var auth = await GetAuthorizedTenantAsync(tenantCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var tenant = auth.Tenant!;

        var user = await userManager.FindByIdAsync(userId);
        if (user != null && user.TenantId == tenant.Id)
            await userManager.DeleteAsync(user);

        return RedirectToAction(nameof(Users), new { tenantCode });
    }

    // GET: /tenant/{tenantCode}/users/{userId}/roles
    [HttpGet("users/{userId}/roles")]
    public async Task<IActionResult> AssignRoles(string tenantCode, string userId)
    {
        var auth = await GetAuthorizedTenantAsync(tenantCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var tenant = auth.Tenant!;

        var user = await userManager.FindByIdAsync(userId);
        if (user == null || user.TenantId != tenant.Id) return NotFound();

        var allRoles = await roleManager.Roles
            .Where(r => r.Name != "Student")
            .ToListAsync();
        var userRoles = await userManager.GetRolesAsync(user);

        ViewBag.Tenant = tenant;
        return View(new AssignRolesViewModel
        {
            UserId = user.Id,
            UserEmail = user.Email ?? string.Empty,
            Roles = allRoles.Select(r => new RoleAssignmentItem
            {
                RoleName = r.Name ?? string.Empty,
                IsAssigned = userRoles.Contains(r.Name ?? string.Empty)
            }).ToList()
        });
    }

    // POST: /tenant/{tenantCode}/users/{userId}/roles
    [HttpPost("users/{userId}/roles")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRoles(string tenantCode, string userId, AssignRolesViewModel model)
    {
        var auth = await GetAuthorizedTenantAsync(tenantCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var tenant = auth.Tenant!;

        var user = await userManager.FindByIdAsync(model.UserId);
        if (user == null || user.TenantId != tenant.Id) return NotFound();

        var currentRoles = await userManager.GetRolesAsync(user);
        var selectedRoles = model.Roles
            .Where(r => r.IsAssigned && !string.Equals(r.RoleName, "Student", StringComparison.OrdinalIgnoreCase))
            .Select(r => r.RoleName)
            .ToList();

        if (!selectedRoles.Contains("Student", StringComparer.OrdinalIgnoreCase))
        {
            selectedRoles.Add("Student");
        }

        var toAdd = selectedRoles.Except(currentRoles).ToList();
        var toRemove = currentRoles.Except(selectedRoles).ToList();

        if (toAdd.Count > 0)
            await userManager.AddToRolesAsync(user, toAdd);

        if (toRemove.Count > 0)
            await userManager.RemoveFromRolesAsync(user, toRemove);

        return RedirectToAction(nameof(Users), new { tenantCode });
    }
}
