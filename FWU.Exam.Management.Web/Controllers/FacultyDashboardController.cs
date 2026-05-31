using FWU.Exam.Management.Application.DTOs;
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
[Route("faculty/{officeCode}")]
public class FacultyDashboardController(
    IFacultyService facultyService,
    IFacultyResolver facultyResolver,
    IDashboardService dashboardService,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager) : Controller
{
    private async Task<Faculty?> GetFacultyAsync(string officeCode) =>
        await facultyService.GetFacultyByOfficeCodeAsync(officeCode);

    private async Task<(Faculty? Faculty, IActionResult? DeniedResult)> GetAuthorizedFacultyAsync(string officeCode)
    {
        var currentFaculty = HttpContext.Items["CurrentFaculty"] as CurrentFaculty;
        Faculty? faculty;

        if (currentFaculty?.OfficeCode == officeCode)
        {
            faculty = new Faculty
            {
                Id = currentFaculty.Id ?? 0,
                Name = currentFaculty.Name ?? string.Empty,
                OfficeCode = currentFaculty.OfficeCode ?? string.Empty,
                LogoPath = currentFaculty.LogoPath
            };
        }
        else
        {
            faculty = await GetFacultyAsync(officeCode);
        }

        if (faculty == null)FWU.Exam.Management.Web/Controllers/TenantDashboardController.cs
            return (null, NotFound());

        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
            return (null, Challenge());

        if (User.IsInRole(Role.SuperAdmin) || User.IsInRole(Role.FacultyAdmin) || currentUser.FacultyId == faculty.Id)
            return (faculty, null);FWU.Exam.Management.Web/Controllers/TenantDashboardController.cs

        return (null, Forbid());
    }

    // GET: /faculty/{officeCode}FWU.Exam.Management.Web/Controllers/TenantDashboardController.cs
    [HttpGet("")]
    public async Task<IActionResult> Index(string tenantCode)
    {
        var auth = await GetAuthorizedFacultyAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var faculty = auth.Faculty!;
        ViewBag.Faculty = faculty;
        ViewBag.UserCount = await userManager.Users.CountAsync(u => u.FacultyId == faculty.Id);

        var stats = await dashboardService.GetFacultyDashboardStatsAsync(faculty.Id);
        var vm = new DashboardViewModel
        {
            CurrentRole = "FacultyAdmin",
            UserName = User.Identity?.Name ?? "User",
            TotalFaculties = stats.TotalFaculties,
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

    // GET: /faculty/{officeCode}/usersFWU.Exam.Management.Web/Controllers/TenantDashboardController.cs
    [HttpGet("users")]
    public async Task<IActionResult> Users(string tenantCode)
    {
        var auth = await GetAuthorizedFacultyAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var faculty = auth.Faculty!;
        ViewBag.Faculty = faculty;

        var users = await userManager.Users
            .Where(u => u.FacultyId == faculty.Id)FWU.Exam.Management.Web/Controllers/TenantDashboardController.cs
            .ToListAsync();

        var model = new List<UserListItemViewModel>();
        foreach (var user in users)
        {
            model.Add(new UserListItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FacultyName = faculty.Name,FWU.Exam.Management.Web/Controllers/TenantDashboardController.cs
                Roles = await userManager.GetRolesAsync(user)
            });
        }
        return View(model);
    }

    // GET: /faculty/{officeCode}/users/createFWU.Exam.Management.Web/Controllers/TenantDashboardController.cs
    [HttpGet("users/create")]
    public async Task<IActionResult> CreateUser(string tenantCode)
    {
        var auth = await GetAuthorizedFacultyAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        ViewBag.Faculty = auth.Faculty!;
        return View(new CreateUserViewModel());
    }

    // POST: /faculty/{officeCode}/users/createFWU.Exam.Management.Web/Controllers/TenantDashboardController.cs
    [HttpPost("users/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(string tenantCode, CreateUserViewModel model)
    {
        var auth = await GetAuthorizedFacultyAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var faculty = auth.Faculty!;FWU.Exam.Management.Web/Controllers/TenantDashboardController.cs

        if (ModelState.IsValid)
        {
            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FacultyId = faculty.Id,FWU.Exam.Management.Web/Controllers/TenantDashboardController.cs
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

        ViewBag.Faculty = faculty;
        return View(model);
    }

    // GET: /faculty/{officeCode}/users/{userId}/editFWU.Exam.Management.Web/Controllers/TenantDashboardController.cs
    [HttpGet("users/{userId}/edit")]
    public async Task<IActionResult> EditUser(string tenantCode, string userId)
    {
        var auth = await GetAuthorizedFacultyAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var faculty = auth.Faculty!;
        var user = await userManager.FindByIdAsync(userId);
        if (user == null || user.FacultyId != faculty.Id) return NotFound();

        ViewBag.Faculty = faculty;FWU.Exam.Management.Web/Controllers/TenantDashboardController.cs
        return View(new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FacultyId = faculty.Id
        });
    }

    // POST: /faculty/{officeCode}/users/{userId}/editFWU.Exam.Management.Web/Controllers/TenantDashboardController.cs
    [HttpPost("users/{userId}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(string tenantCode, string userId, EditUserViewModel model)
    {
        var auth = await GetAuthorizedFacultyAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var faculty = auth.Faculty!;FWU.Exam.Management.Web/Controllers/TenantDashboardController.cs

        if (userId != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null || user.FacultyId != faculty.Id) return NotFound();FWU.Exam.Management.Web/Controllers/TenantDashboardController.cs

            user.Email = model.Email;
            user.UserName = model.Email;

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
                return RedirectToAction(nameof(Users), new { tenantCode });

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        ViewBag.Faculty = faculty;
        return View(model);
    }

    // GET: /faculty/{officeCode}/users/{userId}/deleteFWU.Exam.Management.Web/Controllers/TenantDashboardController.cs
    [HttpGet("users/{userId}/delete")]
    public async Task<IActionResult> DeleteUser(string tenantCode, string userId)
    {
        var auth = await GetAuthorizedFacultyAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var faculty = auth.Faculty!;

        var user = await userManager.FindByIdAsync(userId);
        if (user == null || user.FacultyId != faculty.Id) return NotFound();

        ViewBag.Faculty = faculty;
        return View(user);
    }

    // POST: /faculty/{officeCode}/users/{userId}/deleteFWU.Exam.Management.Web/Controllers/TenantDashboardController.cs
    [HttpPost("users/{userId}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUserConfirmed(string tenantCode, string userId)
    {
        var auth = await GetAuthorizedFacultyAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var faculty = auth.Faculty!;

        var user = await userManager.FindByIdAsync(userId);
        if (user != null && user.FacultyId == faculty.Id)FWU.Exam.Management.Web/Controllers/TenantDashboardController.cs
            await userManager.DeleteAsync(user);

        return RedirectToAction(nameof(Users), new { tenantCode });
    }

    // GET: /faculty/{officeCode}/users/{userId}/rolesFWU.Exam.Management.Web/Controllers/TenantDashboardController.cs
    [HttpGet("users/{userId}/roles")]
    public async Task<IActionResult> AssignRoles(string tenantCode, string userId)
    {
        var auth = await GetAuthorizedFacultyAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var faculty = auth.Faculty!;

        var user = await userManager.FindByIdAsync(userId);
        if (user == null || user.FacultyId != faculty.Id) return NotFound();FWU.Exam.Management.Web/Controllers/TenantDashboardController.cs

        var allRoles = await roleManager.Roles
            .Where(r => r.Name != "Student")
            .ToListAsync();
        var userRoles = await userManager.GetRolesAsync(user);

        ViewBag.Faculty = faculty;FWU.Exam.Management.Web/Controllers/TenantDashboardController.cs
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

    // POST: /faculty/{officeCode}/users/{userId}/rolesFWU.Exam.Management.Web/Controllers/TenantDashboardController.cs
    [HttpPost("users/{userId}/roles")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRoles(string tenantCode, string userId, AssignRolesViewModel model)
    {
        var auth = await GetAuthorizedFacultyAsync(officeCode);
        if (auth.DeniedResult != null) return auth.DeniedResult;

        var faculty = auth.Faculty!;

        var user = await userManager.FindByIdAsync(model.UserId);
        if (user == null || user.FacultyId != faculty.Id) return NotFound();FWU.Exam.Management.Web/Controllers/TenantDashboardController.cs

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
