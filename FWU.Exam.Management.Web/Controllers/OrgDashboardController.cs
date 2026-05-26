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
[Route("org/{officeCode}")]
public class OrgDashboardController(
    IOrganizationService organizationService,
    IDashboardService dashboardService,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager) : Controller
{
    private async Task<Organization?> GetOrgAsync(string officeCode) =>
        await organizationService.GetOrganizationByOfficeCodeAsync(officeCode);

    private async Task<(Organization? Org, IActionResult? DeniedResult)> GetAuthorizedOrgAsync(string officeCode)
    {
        var org = await GetOrgAsync(officeCode);
        if (org == null)
            return (null, NotFound());

        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
            return (null, Challenge());

        if (User.IsInRole(Role.SuperAdmin) || User.IsInRole(Role.FacultyAdmin) || currentUser.OrganizationId == org.Id)
            return (org, null);

        return (null, Forbid());
    }

    // GET: /org/{officeCode}
    [HttpGet("")]
    public async Task<IActionResult> Index(string officeCode)
    {
        var auth = await GetAuthorizedOrgAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var org = auth.Org!;
        ViewBag.Organization = org;
        ViewBag.UserCount = await userManager.Users.CountAsync(u => u.OrganizationId == org.Id);

        var stats = await dashboardService.GetOrgDashboardStatsAsync(org.Id);
        var vm = new DashboardViewModel
        {
            CurrentRole = "FacultyAdmin",
            UserName = User.Identity?.Name ?? "User",
            TotalOrganizations = stats.TotalOrganizations,
            TotalUsers = stats.TotalUsers,
            TotalRoles = stats.TotalRoles,
            TotalColleges = stats.TotalColleges,
            TotalPrograms = stats.TotalPrograms,
            TotalStudents = stats.TotalStudents,
            TotalExamSchedules = stats.TotalExamSchedules,
            TotalExamRegistrations = stats.TotalExamRegistrations,
            TotalSubjects = stats.TotalSubjects,
            TotalAcademicYears = stats.TotalAcademicYears,
            TotalBanks = stats.TotalBanks,
            TotalBoards = stats.TotalBoards,
            TotalBatches = stats.TotalBatches,
            ActiveColleges = stats.ActiveColleges,
            ActivePrograms = stats.ActivePrograms,
            ActiveStudents = stats.ActiveStudents,
            ActiveExamSchedules = stats.ActiveExamSchedules
        };

        return View("~/Views/Dashboard/FacultyAdmin.cshtml", vm);
    }

    // GET: /org/{officeCode}/users
    [HttpGet("users")]
    public async Task<IActionResult> Users(string officeCode)
    {
        var auth = await GetAuthorizedOrgAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var org = auth.Org!;
        ViewBag.Organization = org;

        var users = await userManager.Users
            .Where(u => u.OrganizationId == org.Id)
            .ToListAsync();

        var model = new List<UserListItemViewModel>();
        foreach (var user in users)
        {
            model.Add(new UserListItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                OrganizationName = org.Name,
                Roles = await userManager.GetRolesAsync(user)
            });
        }
        return View(model);
    }

    // GET: /org/{officeCode}/users/create
    [HttpGet("users/create")]
    public async Task<IActionResult> CreateUser(string officeCode)
    {
        var auth = await GetAuthorizedOrgAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        ViewBag.Organization = auth.Org!;
        return View(new CreateUserViewModel());
    }

    // POST: /org/{officeCode}/users/create
    [HttpPost("users/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(string officeCode, CreateUserViewModel model)
    {
        var auth = await GetAuthorizedOrgAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var org = auth.Org!;

        if (ModelState.IsValid)
        {
            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                OrganizationId = org.Id,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (await roleManager.RoleExistsAsync("Student") && !await userManager.IsInRoleAsync(user, "Student"))
                    await userManager.AddToRoleAsync(user, "Student");

                return RedirectToAction(nameof(Users), new { officeCode });
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        ViewBag.Organization = org;
        return View(model);
    }

    // GET: /org/{officeCode}/users/{userId}/edit
    [HttpGet("users/{userId}/edit")]
    public async Task<IActionResult> EditUser(string officeCode, string userId)
    {
        var auth = await GetAuthorizedOrgAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var org = auth.Org!;
        var user = await userManager.FindByIdAsync(userId);
        if (user == null || user.OrganizationId != org.Id) return NotFound();

        ViewBag.Organization = org;
        return View(new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            OrganizationId = org.Id
        });
    }

    // POST: /org/{officeCode}/users/{userId}/edit
    [HttpPost("users/{userId}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(string officeCode, string userId, EditUserViewModel model)
    {
        var auth = await GetAuthorizedOrgAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var org = auth.Org!;

        if (userId != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null || user.OrganizationId != org.Id) return NotFound();

            user.Email = model.Email;
            user.UserName = model.Email;

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
                return RedirectToAction(nameof(Users), new { officeCode });

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        ViewBag.Organization = org;
        return View(model);
    }

    // GET: /org/{officeCode}/users/{userId}/delete
    [HttpGet("users/{userId}/delete")]
    public async Task<IActionResult> DeleteUser(string officeCode, string userId)
    {
        var auth = await GetAuthorizedOrgAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var org = auth.Org!;

        var user = await userManager.FindByIdAsync(userId);
        if (user == null || user.OrganizationId != org.Id) return NotFound();

        ViewBag.Organization = org;
        return View(user);
    }

    // POST: /org/{officeCode}/users/{userId}/delete
    [HttpPost("users/{userId}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(string officeCode, string userId)
    {
        var auth = await GetAuthorizedOrgAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var org = auth.Org!;

        var user = await userManager.FindByIdAsync(userId);
        if (user != null && user.OrganizationId == org.Id)
            await userManager.DeleteAsync(user);

        return RedirectToAction(nameof(Users), new { officeCode });
    }

    // GET: /org/{officeCode}/users/{userId}/roles
    [HttpGet("users/{userId}/roles")]
    public async Task<IActionResult> AssignRoles(string officeCode, string userId)
    {
        var auth = await GetAuthorizedOrgAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var org = auth.Org!;

        var user = await userManager.FindByIdAsync(userId);
        if (user == null || user.OrganizationId != org.Id) return NotFound();

        var allRoles = await roleManager.Roles
            .Where(r => r.Name != "Student")
            .ToListAsync();
        var userRoles = await userManager.GetRolesAsync(user);

        ViewBag.Organization = org;
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

    // POST: /org/{officeCode}/users/{userId}/roles
    [HttpPost("users/{userId}/roles")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRoles(string officeCode, string userId, AssignRolesViewModel model)
    {
        var auth = await GetAuthorizedOrgAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var org = auth.Org!;

        var user = await userManager.FindByIdAsync(model.UserId);
        if (user == null || user.OrganizationId != org.Id) return NotFound();

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

        return RedirectToAction(nameof(Users), new { officeCode });
    }
}
