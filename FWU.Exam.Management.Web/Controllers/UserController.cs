using System.Security.Claims;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure.Data;
using FWU.Exam.Management.Infrastructure.Data.Models;


namespace FWU.Exam.Management.Web.Controllers;

[RequirePermission("users.view")]
public class UserController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context, IUserContext userContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        IQueryable<AppUser> usersQuery = userManager.Users
            .Include(u => u.Faculty)
            .Include(u => u.College);
        usersQuery = usersQuery.ApplyScope(userContext);

        var users = await usersQuery.ToListAsync();
        var model = new List<UserListItemViewModel>();
        foreach (var user in users)
        {
            model.Add(new UserListItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                FacultyName = user.Faculty?.Name,
                CollegeName = user.College?.Name,
                IsActive = user.IsActive,
                Roles = await userManager.GetRolesAsync(user)
            });
        }
        return View(model);
    }

    public async Task<IActionResult> Details(string id)
    {
        if (id == null) return NotFound();

        var user = await userManager.Users
            .Include(u => u.Faculty)
            .Include(u => u.College)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound();

        ViewBag.Roles = await userManager.GetRolesAsync(user);
        return View(user);
    }

    [RequirePermission("users.create")]
    public async Task<IActionResult> Create()
    {
        var roles = await roleManager.Roles.Select(r => r.Name).ToListAsync();
        ViewBag.RolesList = User.IsInRole(Role.SuperAdmin)
            ? roles
            : roles.Where(r => r != Role.SuperAdmin && r != Role.FacultyAdmin);
        ViewBag.Faculties = new SelectList(await context.Faculties.ToListAsync(), "Id", "Name");
        ViewBag.Colleges = new SelectList(await context.Colleges.ToListAsync(), "Id", "Name");
        return View(new CreateUserViewModel());
    }

    [RequirePermission("users.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (model.SelectedRole == Role.SuperAdmin)
            ModelState.AddModelError(nameof(model.SelectedRole), "Cannot create a Super Admin user.");

        if (model.SelectedRole == Role.FacultyAdmin && !User.IsInRole(Role.SuperAdmin))
            ModelState.AddModelError(nameof(model.SelectedRole), "Only Super Admin can create a Faculty Admin user.");

        if (ModelState.IsValid)
        {
            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                IsActive = true
            };

            if (model.SelectedRole is Role.FacultyAdmin)
                user.FacultyId = model.FacultyId;

            if (model.SelectedRole is Role.CollegeAdmin or Role.Student)
                user.CollegeId = model.CollegeId;

            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (await roleManager.RoleExistsAsync(model.SelectedRole))
                    await userManager.AddToRoleAsync(user, model.SelectedRole);
                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        var roles = await roleManager.Roles.Select(r => r.Name).ToListAsync();
        ViewBag.RolesList = User.IsInRole(Role.SuperAdmin)
            ? roles
            : roles.Where(r => r != Role.SuperAdmin && r != Role.FacultyAdmin);
        ViewBag.Faculties = new SelectList(await context.Faculties.AsNoTracking().ToListAsync(), "Id", "Name", model.FacultyId);
        ViewBag.Colleges = new SelectList(await context.Colleges.AsNoTracking().ToListAsync(), "Id", "Name", model.CollegeId);
        return View(model);
    }

    [RequirePermission("users.edit")]
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null) return NotFound();

        var user = await userManager.Users
            .Include(u => u.College)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        var roles = await userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? string.Empty;

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            FacultyId = user.FacultyId,
            CollegeId = user.CollegeId
        };

        ViewBag.PrimaryRole = primaryRole;
        ViewBag.Faculties = new SelectList(await context.Faculties.AsNoTracking().ToListAsync(), "Id", "Name", model.FacultyId);
        ViewBag.Colleges = new SelectList(await context.Colleges.AsNoTracking().ToListAsync(), "Id", "Name", model.CollegeId);
        return View(model);
    }

    [RequirePermission("users.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, EditUserViewModel model)
    {
        if (id != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.Email = model.Email;
            user.UserName = model.Email;
            user.FullName = model.FullName;
            user.FacultyId = model.FacultyId;
            user.CollegeId = model.CollegeId;

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        var roles = await userManager.GetRolesAsync(await userManager.FindByIdAsync(id));
        ViewBag.PrimaryRole = roles.FirstOrDefault() ?? string.Empty;
        ViewBag.Faculties = new SelectList(await context.Faculties.AsNoTracking().ToListAsync(), "Id", "Name", model.FacultyId);
        ViewBag.Colleges = new SelectList(await context.Colleges.AsNoTracking().ToListAsync(), "Id", "Name", model.CollegeId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.IsActive = !user.IsActive;
        await userManager.UpdateAsync(user);

        TempData["SuccessMessage"] = $"User status updated to {(user.IsActive ? "active" : "inactive")}.";
        return RedirectToAction(nameof(Index));
    }

    [RequirePermission("users.delete")]
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null) return NotFound();

        var user = await userManager.Users
            .Include(u => u.Faculty)
            .Include(u => u.College)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound();

        return View(user);
    }

    [RequirePermission("users.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        try
        {
            var user = await userManager.FindByIdAsync(id);
            if (user != null)
                await userManager.DeleteAsync(user);

            TempData["SuccessMessage"] = "User deleted successfully!";
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

    [RequirePermission("users.assign.roles")]
    public async Task<IActionResult> AssignRoles(string id)
    {
        if (id == null) return NotFound();

        var user = await userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var allRoles = await roleManager.Roles.ToListAsync();
        var userRoles = await userManager.GetRolesAsync(user);

        var model = new AssignRolesViewModel
        {
            UserId = user.Id,
            UserEmail = user.Email ?? string.Empty,
            Roles = allRoles.Select(r => new RoleAssignmentItem
            {
                RoleName = r.Name ?? string.Empty,
                IsAssigned = userRoles.Contains(r.Name ?? string.Empty)
            }).ToList()
        };

        return View(model);
    }

    [RequirePermission("users.assign.roles")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRoles(AssignRolesViewModel model)
    {
        var user = await userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFound();

        var currentRoles = await userManager.GetRolesAsync(user);
        var selectedRoles = model.Roles
            .Where(r => r.IsAssigned)
            .Select(r => r.RoleName)
            .ToList();

        var toAdd = selectedRoles.Except(currentRoles).ToList();
        var toRemove = currentRoles.Except(selectedRoles)
            .Where(r => r != Role.SuperAdmin)
            .ToList();

        if (toAdd.Count > 0)
            await userManager.AddToRolesAsync(user, toAdd);

        if (toRemove.Count > 0)
            await userManager.RemoveFromRolesAsync(user, toRemove);

        TempData["SuccessMessage"] = "User roles updated successfully!";
        return RedirectToAction(nameof(Index));
    }
    [RequirePermission("users.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(string id)
    {
        try
        {
            var user = await userManager.FindByIdAsync(id);
            if (user != null)
                await userManager.DeleteAsync(user);
            return Json(new { success = true, message = "User deleted successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [RequirePermission("users.create")]
    [HttpGet]
    public async Task<IActionResult> GetStudentsWithoutUsers(int? collegeId, int? facultyId)
    {
        var existingUserEmails = (await context.Users
            .Where(u => u.Email != null)
            .Select(u => u.Email!)
            .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingUserNames = (await context.Users
            .Where(u => u.UserName != null)
            .Select(u => u.UserName!)
            .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var query = context.StudentRegistrations
            .Include(s => s.College)
            .Include(s => s.Faculty)
            .Where(s => s.IsActive);

        if (collegeId.HasValue)
            query = query.Where(s => s.CollegeId == collegeId.Value);
        if (facultyId.HasValue)
            query = query.Where(s => s.FacultyId == facultyId.Value);

        var allStudents = await query
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync();

        var students = allStudents
            .Where(s =>
            {
                var hasEmail = !string.IsNullOrWhiteSpace(s.Email);
                var hasRegNo = !string.IsNullOrWhiteSpace(s.RegistrationNumber);

                var emailMatch = hasEmail && existingUserEmails.Contains(s.Email!);
                var regNoMatch = hasRegNo && existingUserNames.Contains(s.RegistrationNumber!);

                return !emailMatch && !regNoMatch;
            })
            .Select(s => new
            {
                s.Id,
                FullName = s.FirstName + " " + s.LastName,
                s.Email,
                s.RegistrationNumber,
                CollegeName = s.College?.Name ?? "",
                FacultyName = s.Faculty?.Name ?? "",
                s.DateOfBirthBS,
                HasEmail = !string.IsNullOrWhiteSpace(s.Email)
            })
            .ToList();

        return Json(students);
    }

    [RequirePermission("users.create")]
    [HttpPost]
    public async Task<IActionResult> CreateUsersFromRegistrations([FromBody] List<int> registrationIds)
    {
        try
        {
            if (registrationIds == null || registrationIds.Count == 0)
                return Json(new { success = false, message = "No registrations selected." });

            var results = new List<object>();

            foreach (var id in registrationIds)
            {
                try
                {
                    var reg = await context.StudentRegistrations
                        .Include(s => s.Faculty)
                        .Include(s => s.College)
                        .FirstOrDefaultAsync(s => s.Id == id);

                    if (reg == null)
                    {
                        results.Add(new { id, success = false, name = $"ID {id}", message = "Registration not found." });
                        continue;
                    }

                    var loginId = !string.IsNullOrWhiteSpace(reg.Email)
                        ? reg.Email
                        : reg.RegistrationNumber;

                    if (string.IsNullOrWhiteSpace(loginId))
                    {
                        results.Add(new { id, success = false, name = $"{reg.FirstName} {reg.LastName}", message = "No email or registration number found." });
                        continue;
                    }

                    var existingUser = await userManager.FindByEmailAsync(loginId);
                    if (existingUser == null)
                        existingUser = await userManager.Users.FirstOrDefaultAsync(u => u.UserName == loginId);

                    if (existingUser != null)
                    {
                        results.Add(new { id, success = false, name = $"{reg.FirstName} {reg.LastName}", message = "User already exists." });
                        continue;
                    }

                    var user = new AppUser
                    {
                        UserName = loginId,
                        Email = loginId,
                        EmailConfirmed = true,
                        FullName = $"{reg.FirstName} {reg.LastName}".Trim(),
                        IsActive = true,
                        FacultyId = reg.FacultyId,
                        CollegeId = reg.CollegeId
                    };

                    var createResult = await userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        results.Add(new { id, success = false, name = $"{reg.FirstName} {reg.LastName}", message = string.Join(", ", createResult.Errors.Select(e => e.Description)) });
                        continue;
                    }

                    var password = reg.DateOfBirthBS;
                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        SetPasswordHashDirectly(user, password);
                        await userManager.UpdateAsync(user);
                    }

                    if (!await userManager.IsInRoleAsync(user, Role.Student))
                        await userManager.AddToRoleAsync(user, Role.Student);

                    await userManager.AddClaimAsync(user, new Claim("must_change_password", "true"));

                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    await userManager.ConfirmEmailAsync(user, token);

                    results.Add(new { id, success = true, name = $"{reg.FirstName} {reg.LastName}", message = $"Created (login: {loginId})." });
                }
                catch (Exception ex)
                {
                    results.Add(new { id, success = false, name = $"ID {id}", message = ex.Message });
                }
            }

            var created = results.Count(r => (bool)r.GetType().GetProperty("success")!.GetValue(r)!);
            var failed = results.Count - created;

            return Json(new { created, failed, results });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    private static void SetPasswordHashDirectly(AppUser user, string password)
    {
        var hasher = new PasswordHasher<AppUser>();
        user.PasswordHash = hasher.HashPassword(user, password);
    }
}
