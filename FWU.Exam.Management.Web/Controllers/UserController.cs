using System.Security.Claims;
using FWU.Exam.Management.Application.Interfaces;
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
public class UserController(
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    AppDbContext context,
    IUserContext userContext,
    IBulkUserCreationService bulkUserCreationService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "email", string sortDir = "asc", int pageSize = 10)
    {
        IQueryable<AppUser> usersQuery = userManager.Users
            .Include(u => u.Faculty)
            .Include(u => u.College);
        usersQuery = usersQuery.ApplyScope(userContext);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            usersQuery = usersQuery.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(s)) ||
                (u.FullName != null && u.FullName.ToLower().Contains(s)) ||
                (u.Faculty != null && u.Faculty.Name != null && u.Faculty.Name.ToLower().Contains(s)) ||
                (u.College != null && u.College.Name != null && u.College.Name.ToLower().Contains(s)));
        }

        usersQuery = sortDir.ToLower() == "desc"
            ? usersQuery.OrderByDescending(GetUserSortProperty(sort))
            : usersQuery.OrderBy(GetUserSortProperty(sort));

        var totalCount = await usersQuery.CountAsync();

        var users = await usersQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userIds = users.Select(u => u.Id).ToList();
        var userRoles = await context.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
            .ToListAsync();

        var roleLookup = userRoles
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => (IList<string>)g.Select(x => x.Name).ToList());

        var model = users.Select(user => new UserListItemViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            FacultyName = user.Faculty?.Name,
            CollegeName = user.College?.Name,
            IsActive = user.IsActive,
            Roles = roleLookup.GetValueOrDefault(user.Id, new List<string>())
        }).ToList();

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(model);
    }

    private static System.Linq.Expressions.Expression<Func<AppUser, object>> GetUserSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "email" => u => u.Email ?? "",
            "fullname" => u => u.FullName ?? "",
            "isactive" => u => u.IsActive,
            "faculty" => u => u.Faculty != null ? u.Faculty.Name ?? "" : "",
            "college" => u => u.College != null ? u.College.Name ?? "" : "",
            _ => u => u.Email ?? ""
        };
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
    public async Task<IActionResult> GetStudentsWithoutUsers(
        int? collegeId, int? facultyId, int page = 1, int pageSize = 50)
    {
        var (data, totalCount) = await bulkUserCreationService.GetStudentsWithoutUsersAsync(
            collegeId, facultyId, page, pageSize);
        return Json(new { data, totalCount });
    }

    [RequirePermission("users.create")]
    [HttpPost]
    public async Task<IActionResult> CreateUsersFromRegistrations([FromBody] List<int> registrationIds)
    {
        if (registrationIds == null || registrationIds.Count == 0)
            return Json(new { success = false, message = "No registrations selected." });

        var userId = userManager.GetUserId(User) ?? "unknown";
        var job = await bulkUserCreationService.StartJobAsync(registrationIds, userId);

        return Json(new
        {
            success = true,
            jobId = job.Id,
            totalStudents = job.TotalStudents,
            message = $"Background job started. Processing {job.TotalStudents} students."
        });
    }

    [RequirePermission("users.create")]
    [HttpPost]
    public async Task<IActionResult> CreateUsersFromFilters([FromBody] FilterModel filters)
    {
        var userId = userManager.GetUserId(User) ?? "unknown";
        var job = await bulkUserCreationService.StartJobFromFiltersAsync(
            filters.CollegeId, filters.FacultyId, userId);

        return Json(new
        {
            success = true,
            jobId = job.Id,
            totalStudents = job.TotalStudents,
            message = $"Background job started. Processing {job.TotalStudents} students."
        });
    }

    public class FilterModel
    {
        public int? CollegeId { get; set; }
        public int? FacultyId { get; set; }
    }

    [RequirePermission("users.create")]
    [HttpGet]
    public async Task<IActionResult> GetBulkJobStatus(int jobId)
    {
        var job = await bulkUserCreationService.GetJobStatusAsync(jobId);
        if (job == null) return NotFound();

        return Json(new
        {
            job.Status,
            job.TotalStudents,
            job.ProcessedCount,
            job.SuccessCount,
            job.FailedCount,
            percentage = job.TotalStudents > 0
                ? (int)(job.ProcessedCount * 100.0 / job.TotalStudents)
                : 0,
            job.ErrorMessage,
            completedAt = job.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss")
        });
    }
}
