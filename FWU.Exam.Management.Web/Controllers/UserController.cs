using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure.Data.Models;


namespace FWU.Exam.Management.Web.Controllers;

[Authorize(Roles = "SuperAdmin,FacultyAdmin,CollegeAdmin,DepartmentAdmin")]
public class UserController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var currentUser = await userManager.GetUserAsync(User);
        var isSuperAdmin = User.IsInRole(Role.SuperAdmin);

        var usersQuery = userManager.Users
            .Include(u => u.Faculty)
            .Include(u => u.College);

        IQueryable<AppUser> filteredQuery;

        if (isSuperAdmin)
        {
            filteredQuery = usersQuery;
        }
        else if (User.IsInRole(Role.FacultyAdmin) && currentUser?.FacultyId != null)
        {
            filteredQuery = usersQuery.Where(u => u.FacultyId == currentUser.FacultyId);
        }
        else if (User.IsInRole(Role.CollegeAdmin) && currentUser?.CollegeId != null)
        {
            filteredQuery = usersQuery.Where(u => u.CollegeId == currentUser.CollegeId);
        }
        else
        {
            filteredQuery = usersQuery;
        }

        var users = await filteredQuery.ToListAsync();
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

    public async Task<IActionResult> Create()
    {
        var roles = await roleManager.Roles.Select(r => r.Name).ToListAsync();
        ViewBag.RolesList = User.IsInRole(Role.SuperAdmin)
            ? roles
            : roles.Where(r => r != Role.SuperAdmin && r != Role.FacultyAdmin);
        ViewBag.Faculties = new SelectList(await context.Faculties.ToListAsync(), "Id", "Name");
        ViewBag.Colleges = new SelectList(await context.Colleges.ToListAsync(), "Id", "Name");
        ViewBag.Departments = new SelectList(await context.Departments.ToListAsync(), "Id", "DepartmentName");
        return View(new CreateUserViewModel());
    }

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

            if (model.SelectedRole is Role.CollegeAdmin or Role.DepartmentAdmin or Role.Student)
                user.CollegeId = model.CollegeId;

            if (model.SelectedRole is Role.DepartmentAdmin)
                user.DepartmentId = model.DepartmentId;

            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                if (await roleManager.RoleExistsAsync(model.SelectedRole))
                    await userManager.AddToRoleAsync(user, model.SelectedRole);
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
        ViewBag.Departments = new SelectList(await context.Departments.AsNoTracking().ToListAsync(), "Id", "DepartmentName", model.DepartmentId);
        return View(model);
    }

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
            CollegeId = user.CollegeId,
            DepartmentId = user.DepartmentId
        };

        ViewBag.PrimaryRole = primaryRole;
        ViewBag.Faculties = new SelectList(await context.Faculties.AsNoTracking().ToListAsync(), "Id", "Name", model.FacultyId);
        ViewBag.Colleges = new SelectList(await context.Colleges.AsNoTracking().ToListAsync(), "Id", "Name", model.CollegeId);
        ViewBag.Departments = new SelectList(await context.Departments.AsNoTracking().ToListAsync(), "Id", "DepartmentName", model.DepartmentId);
        return View(model);
    }

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
            user.DepartmentId = model.DepartmentId;

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
                return RedirectToAction(nameof(Index));

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        var roles = await userManager.GetRolesAsync(await userManager.FindByIdAsync(id));
        ViewBag.PrimaryRole = roles.FirstOrDefault() ?? string.Empty;
        ViewBag.Faculties = new SelectList(await context.Faculties.AsNoTracking().ToListAsync(), "Id", "Name", model.FacultyId);
        ViewBag.Colleges = new SelectList(await context.Colleges.AsNoTracking().ToListAsync(), "Id", "Name", model.CollegeId);
        ViewBag.Departments = new SelectList(await context.Departments.AsNoTracking().ToListAsync(), "Id", "DepartmentName", model.DepartmentId);
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

        return RedirectToAction(nameof(Index));
    }

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

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user != null)
            await userManager.DeleteAsync(user);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> AssignRoles(string id)
    {
        if (id == null) return NotFound();

        var user = await userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var allRoles = await roleManager.Roles
            .Where(r => r.Name != "Student" && r.Name != Role.SuperAdmin)
            .ToListAsync();
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRoles(AssignRolesViewModel model)
    {
        var user = await userManager.FindByIdAsync(model.UserId);
        if (user == null) return NotFound();

        var currentRoles = await userManager.GetRolesAsync(user);
        var selectedRoles = model.Roles
            .Where(r => r.IsAssigned
                && !string.Equals(r.RoleName, "Student", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(r.RoleName, Role.SuperAdmin, StringComparison.OrdinalIgnoreCase))
            .Select(r => r.RoleName)
            .ToList();

        if (!selectedRoles.Contains("Student", StringComparer.OrdinalIgnoreCase))
            selectedRoles.Add("Student");

        var toAdd = selectedRoles.Except(currentRoles).ToList();
        var toRemove = currentRoles.Except(selectedRoles)
            .Where(r => r != Role.SuperAdmin)
            .ToList();

        if (toAdd.Count > 0)
            await userManager.AddToRolesAsync(user, toAdd);

        if (toRemove.Count > 0)
            await userManager.RemoveFromRolesAsync(user, toRemove);

        return RedirectToAction(nameof(Index));
    }
}
