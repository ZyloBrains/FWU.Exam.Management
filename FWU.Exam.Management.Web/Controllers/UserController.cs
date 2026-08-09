using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Web.ViewModels;
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
    IPermissionService permissionService,
    IBulkUserCreationService bulkUserCreationService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "email", string sortDir = "asc", int pageSize = 10)
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

        var user = await LoadScopedUserAsync(id);

        if (user == null) return NotFound();

        ViewBag.Roles = await userManager.GetRolesAsync(user);
        return View(user);
    }

    [RequirePermission("users.create")]
    public async Task<IActionResult> Create()
    {
        var assignableRoles = await GetAssignableRolesAsync();
        var roles = (await roleManager.Roles.Select(r => r.Name).ToListAsync())
            .Where(r => r != null && assignableRoles.Contains(r));
        ViewBag.RolesList = roles;
        ViewBag.Faculties = new SelectList(await context.Faculties.ApplyScope(userContext).ToListAsync(), "Id", "Name");
        ViewBag.Colleges = new SelectList(await context.Colleges.ApplyScope(userContext).ToListAsync(), "Id", "Name");
        await SetCreateFiltersAsync();
        return View(new CreateUserViewModel());
    }

    [RequirePermission("users.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        var callerRole = await GetCallerRoleAsync();

        if (model.SelectedRole == Role.SuperAdmin)
            ModelState.AddModelError(nameof(model.SelectedRole), "Cannot create a Super Admin user.");

        if (model.SelectedRole == Role.FacultyAdmin && callerRole != Role.SuperAdmin)
            ModelState.AddModelError(nameof(model.SelectedRole), "Only Super Admin can create a Faculty Admin user.");

        if (model.SelectedRole != null && !(await GetAssignableRolesAsync()).Contains(model.SelectedRole))
            ModelState.AddModelError(nameof(model.SelectedRole), "You are not allowed to assign this role.");

        if (model.SelectedRole == Role.FacultyAdmin && !model.FacultyId.HasValue)
            ModelState.AddModelError(nameof(model.FacultyId), "Faculty is required for a Faculty Admin user.");

        if (model.SelectedRole is Role.CollegeAdmin or Role.Student && !IsCollegeInScope(model.CollegeId))
            ModelState.AddModelError(nameof(model.CollegeId), "The selected college is not within your access.");

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
                var selectedRole = model.SelectedRole;
                if (!string.IsNullOrEmpty(selectedRole) && await roleManager.RoleExistsAsync(selectedRole))
                    await userManager.AddToRoleAsync(user, selectedRole);
                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        var assignableRoles = await GetAssignableRolesAsync();
        var roles = (await roleManager.Roles.Select(r => r.Name).ToListAsync())
            .Where(r => r != null && assignableRoles.Contains(r));
        ViewBag.RolesList = roles;
        ViewBag.Faculties = new SelectList(await context.Faculties.ApplyScope(userContext).AsNoTracking().ToListAsync(), "Id", "Name", model.FacultyId);
        ViewBag.Colleges = new SelectList(await context.Colleges.ApplyScope(userContext).AsNoTracking().ToListAsync(), "Id", "Name", model.CollegeId);
        await SetCreateFiltersAsync();
        return View(model);
    }

    private async Task SetCreateFiltersAsync()
    {
        ViewData["ShowCollegeFilter"] = userContext.IsSuperAdmin || userContext.IsFacultyAdmin;
        ViewData["ShowFacultyFilter"] = userContext.IsSuperAdmin;
        ViewData["ShowProgramFilter"] = userContext.IsSuperAdmin || userContext.IsFacultyAdmin || userContext.IsCollegeAdmin;
        ViewBag.FilterColleges = userContext.IsSuperAdmin
            ? new SelectList(Array.Empty<College>(), "Id", "Name")
            : new SelectList(await context.Colleges.ApplyScope(userContext).AsNoTracking().ToListAsync(), "Id", "Name");
        ViewBag.DefaultFacultyId = userContext.IsFacultyAdmin ? userContext.FacultyId : null;
        ViewBag.CurrentCollegeId = userContext.IsCollegeAdmin ? userContext.CollegeId : null;
    }

    [RequirePermission("users.edit")]
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null) return NotFound();

        var user = await LoadScopedUserAsync(id);
        if (user == null) return NotFound();

        if (!await CanManageTargetAsync(user))
            return Forbid();

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
        ViewBag.Faculties = new SelectList(await context.Faculties.ApplyScope(userContext).AsNoTracking().ToListAsync(), "Id", "Name", model.FacultyId);
        ViewBag.Colleges = new SelectList(await context.Colleges.ApplyScope(userContext).AsNoTracking().ToListAsync(), "Id", "Name", model.CollegeId);
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
            var user = await LoadScopedUserAsync(id);
            if (user == null) return NotFound();

            if (!await CanManageTargetAsync(user))
                return Forbid();

            if (model.CollegeId.HasValue && !IsCollegeInScope(model.CollegeId))
                ModelState.AddModelError(nameof(model.CollegeId), "The selected college is not within your access.");

            if (!IsFacultyInScope(model.FacultyId))
                ModelState.AddModelError(nameof(model.FacultyId), "The selected faculty is not within your access.");

            if (!ModelState.IsValid)
                return await ReloadEditViewAsync(id, model);

            user.Email = model.Email;
            user.UserName = model.Email;
            user.FullName = model.FullName;
            user.CollegeId = model.CollegeId;
            // CollegeAdmin cannot manage faculty assignments; preserve the existing value.
            if (!User.IsInRole(Role.CollegeAdmin))
                user.FacultyId = model.FacultyId;

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        return await ReloadEditViewAsync(id, model);
    }

    private async Task<IActionResult> ReloadEditViewAsync(string id, EditUserViewModel model)
    {
        var editUser = await LoadScopedUserAsync(id);
        if (editUser == null) return NotFound();
        var roles = await userManager.GetRolesAsync(editUser);
        ViewBag.PrimaryRole = roles.FirstOrDefault() ?? string.Empty;
        ViewBag.Faculties = new SelectList(await context.Faculties.ApplyScope(userContext).AsNoTracking().ToListAsync(), "Id", "Name", model.FacultyId);
        ViewBag.Colleges = new SelectList(await context.Colleges.ApplyScope(userContext).AsNoTracking().ToListAsync(), "Id", "Name", model.CollegeId);
        return View(model);
    }

    [RequirePermission("users.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        var user = await LoadScopedUserAsync(id);
        if (user == null) return NotFound();

        if (!await CanManageTargetAsync(user))
            return Forbid();

        user.IsActive = !user.IsActive;
        await userManager.UpdateAsync(user);

        TempData["SuccessMessage"] = $"User status updated to {(user.IsActive ? "active" : "inactive")}.";
        return RedirectToAction(nameof(Index));
    }

    [RequirePermission("users.delete")]
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null) return NotFound();

        var user = await LoadScopedUserAsync(id);

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
            var user = await LoadScopedUserAsync(id);
            if (user != null && await CanManageTargetAsync(user))
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

        var user = await LoadScopedUserAsync(id);
        if (user == null) return NotFound();

        if (!await CanManageTargetAsync(user))
            return Forbid();

        var assignableRoles = await GetAssignableRolesAsync();
        var allRoles = await roleManager.Roles.ToListAsync();
        var userRoles = await userManager.GetRolesAsync(user);

        var model = new AssignRolesViewModel
        {
            UserId = user.Id,
            UserEmail = user.Email ?? string.Empty,
            Roles = allRoles
                .Where(r => r.Name != null && assignableRoles.Contains(r.Name))
                .Select(r => new RoleAssignmentItem
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
        var user = await LoadScopedUserAsync(model.UserId);
        if (user == null) return NotFound();

        if (!await CanManageTargetAsync(user))
            return Forbid();

        var assignableRoles = await GetAssignableRolesAsync();
        var currentRoles = await userManager.GetRolesAsync(user);
        var selectedRoles = model.Roles
            .Where(r => r.IsAssigned)
            .Select(r => r.RoleName)
            .ToList();

        var toAdd = selectedRoles.Intersect(assignableRoles).Except(currentRoles).ToList();
        var toRemove = currentRoles.Intersect(assignableRoles).Except(selectedRoles).ToList();

        if (toAdd.Count > 0)
            await userManager.AddToRolesAsync(user, toAdd);

        if (toRemove.Count > 0)
            await userManager.RemoveFromRolesAsync(user, toRemove);

        TempData["SuccessMessage"] = "User roles updated successfully!";
        return RedirectToAction(nameof(Index));
    }
    [RequirePermission("users.edit")]
    public async Task<IActionResult> ResetPassword(string? userId, string? search, int page = 1, int pageSize = 10)
    {
        UserResetPasswordViewModel? selectedUser = null;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            selectedUser = await LoadSelectedUserAsync(userId);
            if (selectedUser == null)
                TempData["ErrorMessage"] = "User not found or you do not have access to this user.";
            else
            {
                var target = await userManager.FindByIdAsync(userId);
                if (target != null && !await CanManageTargetAsync(target))
                {
                    selectedUser = null;
                    TempData["ErrorMessage"] = "You do not have access to reset this user's password.";
                }
            }
        }

        var model = await BuildResetPasswordPageAsync(selectedUser, search, page, pageSize);
        return View(model);
    }

    [RequirePermission("users.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(UserResetPasswordPageViewModel model)
    {
        var userId = model.SelectedUser?.UserId;
        if (string.IsNullOrWhiteSpace(userId))
        {
            TempData["ErrorMessage"] = "Please select a user to reset.";
            return RedirectToAction(nameof(ResetPassword), new { search = model.Search, page = model.Page, pageSize = model.PageSize });
        }

        var selectedUser = await LoadSelectedUserAsync(userId);
        if (selectedUser == null)
        {
            TempData["ErrorMessage"] = "User not found or you do not have access to this user.";
            return RedirectToAction(nameof(ResetPassword), new { search = model.Search, page = model.Page, pageSize = model.PageSize });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(ResetPassword), new { search = model.Search, page = model.Page, pageSize = model.PageSize });
        }

        if (!await CanManageTargetAsync(user))
        {
            TempData["ErrorMessage"] = "You do not have access to reset this user's password.";
            return RedirectToAction(nameof(ResetPassword), new { search = model.Search, page = model.Page, pageSize = model.PageSize });
        }

        if (!ModelState.IsValid)
        {
            var pageModel = await BuildResetPasswordPageAsync(selectedUser, model.Search, model.Page, model.PageSize);
            return View(pageModel);
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, model.SelectedUser!.NewPassword);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = $"Password reset successfully for '{(user.FullName ?? user.Email)}'. The user must use the new password on their next login.";
            return RedirectToAction(nameof(ResetPassword), new { userId = user.Id, search = model.Search, page = model.Page, pageSize = model.PageSize });
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        var reloaded = await BuildResetPasswordPageAsync(selectedUser, model.Search, model.Page, model.PageSize);
        return View(reloaded);
    }

    private async Task<UserResetPasswordViewModel?> LoadSelectedUserAsync(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var user = await userManager.Users
            .Include(u => u.Faculty)
            .Include(u => u.College)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return null;

        var accessibleIds = await userManager.Users
            .ApplyScope(userContext)
            .Select(u => u.Id)
            .ToListAsync();

        if (!accessibleIds.Contains(user.Id))
            return null;

        return new UserResetPasswordViewModel
        {
            UserId = user.Id,
            UserEmail = user.Email ?? string.Empty,
            FullName = user.FullName,
            FacultyName = user.Faculty?.Name,
            CollegeName = user.College?.Name,
            IsActive = user.IsActive
        };
    }

    private async Task<AppUser?> LoadScopedUserAsync(string? id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;

        var scopedIds = userManager.Users.ApplyScope(userContext).Select(u => u.Id);
        return await userManager.Users
            .Include(u => u.Faculty)
            .Include(u => u.College)
            .FirstOrDefaultAsync(u => u.Id == id && scopedIds.Contains(u.Id));
    }

    private async Task<bool> CanManageTargetAsync(AppUser user)
    {
        var callerRole = await GetCallerRoleAsync();
        if (callerRole != null)
            return RoleRules.CanManageTarget(callerRole, await userManager.GetRolesAsync(user));

        // Caller holds only dynamic/custom roles: permission-subset rule —
        // they may manage a user only if the target's permission set is a
        // subset of the caller's own, preventing privilege escalation.
        var callerId = userManager.GetUserId(User);
        if (callerId == null) return false;
        var callerPermissions = new HashSet<string>(await permissionService.GetUserPermissionsAsync(callerId));
        var targetPermissions = await permissionService.GetUserPermissionsAsync(user.Id);
        return targetPermissions.All(callerPermissions.Contains);
    }

    private async Task<string?> GetCallerRoleAsync()
    {
        var caller = await userManager.GetUserAsync(User);
        if (caller == null) return null;
        return RoleRules.FromRoles(await userManager.GetRolesAsync(caller));
    }

    private async Task<IReadOnlySet<string>> GetAssignableRolesAsync()
    {
        var callerRole = await GetCallerRoleAsync();
        if (callerRole != null)
            return RoleRules.AssignableRoles(callerRole);

        // Caller holds only dynamic/custom roles: permission-subset rule —
        // they may only assign roles whose permission set is a subset of
        // their own, matching the guard used by ManagePermissionsController.
        var callerId = userManager.GetUserId(User);
        if (callerId == null) return new HashSet<string>();
        var callerPermissions = new HashSet<string>(await permissionService.GetUserPermissionsAsync(callerId));
        var assignable = new HashSet<string>();
        foreach (var role in await roleManager.Roles.ToListAsync())
        {
            if (role.Name == null) continue;
            var rolePermissions = await permissionService.GetRolePermissionsAsync(role.Id);
            if (rolePermissions.All(callerPermissions.Contains))
                assignable.Add(role.Name);
        }
        return assignable;
    }

    private bool IsCollegeInScope(int? collegeId)
    {
        if (!collegeId.HasValue) return false;
        if (User.IsInRole(Role.SuperAdmin)) return true;
        if (User.IsInRole(Role.CollegeAdmin))
            return userContext.CollegeId == collegeId.Value;
        if (User.IsInRole(Role.FacultyAdmin))
            return userContext.FacultyCollegeIds.Contains(collegeId.Value);
        return false;
    }

    private bool IsFacultyInScope(int? facultyId)
    {
        if (!facultyId.HasValue) return true;
        if (User.IsInRole(Role.SuperAdmin)) return true;
        if (User.IsInRole(Role.CollegeAdmin)) return false;
        return userContext.FacultyId == facultyId.Value;
    }

    private async Task<UserResetPasswordPageViewModel> BuildResetPasswordPageAsync(
        UserResetPasswordViewModel? selectedUser, string? search, int page, int pageSize)
    {
        IQueryable<AppUser> usersQuery = userManager.Users
            .Include(u => u.Faculty)
            .Include(u => u.College)
            .ApplyScope(userContext);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            usersQuery = usersQuery.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(s)) ||
                (u.FullName != null && u.FullName.ToLower().Contains(s)) ||
                (u.Faculty != null && u.Faculty.Name != null && u.Faculty.Name.ToLower().Contains(s)) ||
                (u.College != null && u.College.Name != null && u.College.Name.ToLower().Contains(s)));
        }

        var totalCount = await usersQuery.CountAsync();

        var users = await usersQuery
            .OrderBy(u => u.Email ?? "")
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

        var items = users.Select(u => new UserListItemViewModel
        {
            Id = u.Id,
            Email = u.Email ?? string.Empty,
            FullName = u.FullName,
            FacultyName = u.Faculty?.Name,
            CollegeName = u.College?.Name,
            IsActive = u.IsActive,
            Roles = roleLookup.GetValueOrDefault(u.Id, new List<string>())
        }).ToList();

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search ?? string.Empty;

        return new UserResetPasswordPageViewModel
        {
            SelectedUser = selectedUser,
            Users = items,
            Search = search,
            Page = page,
            PageSize = pageSize
        };
    }

    [RequirePermission("users.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(string id)
    {
        try
        {
            var user = await LoadScopedUserAsync(id);
            if (user != null && await CanManageTargetAsync(user))
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
    public async Task<JsonResult> GetCollegesByFaculty(int? facultyId)
    {
        if (!facultyId.HasValue)
            return Json(new List<SelectOption>());

        var colleges = await context.Colleges
            .ApplyScope(userContext)
            .Where(c => c.CollegeFaculties!.Any(cf => cf.FacultyId == facultyId.Value))
            .OrderBy(c => c.Name)
            .Select(c => new SelectOption { Id = c.Id, Name = c.Name })
            .AsNoTracking()
            .ToListAsync();
        return Json(colleges);
    }

    [RequirePermission("users.create")]
    [HttpGet]
    public async Task<JsonResult> GetProgramsByCollege(int collegeId)
    {
        var programs = await context.CollegePrograms
            .ApplyScope(userContext)
            .Where(cp => cp.CollegeId == collegeId && cp.Program != null && cp.Program.ProgramName != null)
            .Select(cp => new SelectOption { Id = cp.Program!.Id, Name = cp.Program.ProgramName })
            .OrderBy(p => p.Name)
            .AsNoTracking()
            .ToListAsync();
        return Json(programs);
    }

    [RequirePermission("users.create")]
    [HttpGet]
    public async Task<IActionResult> GetStudentsWithoutUsers(
        int? collegeId, int? facultyId, int? programId, int page = 1, int pageSize = 50)
    {
        var (data, totalCount) = await bulkUserCreationService.GetStudentsWithoutUsersAsync(
            collegeId, facultyId, programId, page, pageSize);
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
            filters.CollegeId, filters.FacultyId, filters.ProgramId, userId);
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
        public int? ProgramId { get; set; }
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
