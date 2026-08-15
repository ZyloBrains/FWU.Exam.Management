using System.IO;
using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Extensions;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Web.ViewModels;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using FWU.Exam.Management.Infrastructure.Data;
using FWU.Exam.Management.Infrastructure.Data.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FWU.Exam.Management.Web.Controllers;

[RequirePermission("users.view")]
public class UserController(
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    AppDbContext context,
    IWebHostEnvironment env,
    IUserContext userContext,
    ITenantContext tenantContext,
    IMemoryCache cache,
    IPermissionService permissionService,
    IBulkUserCreationService bulkUserCreationService,
    IAuditLogWriter auditLogWriter) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string? role = null, bool? isActive = null, string sort = "email", string sortDir = "asc", int pageSize = 10)
    {
        IQueryable<AppUser> scopedUsersQuery = userManager.Users
            .AsNoTracking()
            .Include(u => u.Faculty)
            .Include(u => u.College);
        scopedUsersQuery = scopedUsersQuery.ApplyScope(userContext);

        var assignableRoles = await GetAssignableRolesAsync();
        var rolesPresentInScope = await GetRolesPresentInScopeAsync(scopedUsersQuery);
        ViewBag.RolesList = rolesPresentInScope
            .Where(assignableRoles.Contains)
            .OrderBy(n => n);
        ViewBag.RoleFilter = role;
        ViewBag.IsActive = isActive;

        var usersQuery = await BuildFilteredUsersAsync(search, role, isActive, assignableRoles);
        usersQuery = ApplySort(usersQuery, sort, sortDir);

        var totalCount = await usersQuery.CountAsync();

        var users = await usersQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = await ToViewModelsAsync(users);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(model);
    }

    private async Task<IQueryable<AppUser>> BuildFilteredUsersAsync(
        string? search, string? role, bool? isActive, IReadOnlySet<string> assignableRoles)
    {
        IQueryable<AppUser> usersQuery = userManager.Users
            .AsNoTracking()
            .Include(u => u.Faculty)
            .Include(u => u.College);
        usersQuery = usersQuery.ApplyScope(userContext);

        if (!string.IsNullOrWhiteSpace(role) && assignableRoles.Contains(role))
        {
            var roleId = await context.Roles
                .AsNoTracking()
                .Where(r => r.Name == role)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();
            if (roleId != null)
                usersQuery = usersQuery.Where(u =>
                    context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == roleId));
        }

        if (isActive.HasValue)
            usersQuery = usersQuery.Where(u => u.IsActive == isActive.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            usersQuery = usersQuery.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(s)) ||
                (u.FullName != null && u.FullName.ToLower().Contains(s)) ||
                (u.Faculty != null && u.Faculty.Name != null && u.Faculty.Name.ToLower().Contains(s)) ||
                (u.College != null && u.College.Name != null && u.College.Name.ToLower().Contains(s)));
        }

        return usersQuery;
    }

    private static IQueryable<AppUser> ApplySort(IQueryable<AppUser> query, string sort, string sortDir)
    {
        return sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetUserSortProperty(sort))
            : query.OrderBy(GetUserSortProperty(sort));
    }

    private async Task<List<UserListItemViewModel>> ToViewModelsAsync(IEnumerable<AppUser> users)
    {
        var userIds = users.Select(u => u.Id).ToList();
        var userRoles = await context.UserRoles
            .AsNoTracking()
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(context.Roles.AsNoTracking(), ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
            .ToListAsync();

        var roleLookup = userRoles
            .GroupBy(x => x.UserId)
            .ToDictionary(g => g.Key, g => (IList<string>)g.Select(x => x.Name).ToList());

        return users.Select(user => new UserListItemViewModel
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            FacultyName = user.Faculty?.Name,
            CollegeName = user.College?.Name,
            IsActive = user.IsActive,
            Roles = roleLookup.GetValueOrDefault(user.Id, new List<string>())
        }).ToList();
    }

    private async Task<(List<UserListItemViewModel> Items, int TotalCount)> GetFilteredPageAsync(
        int page, int pageSize, string? search, string? role, bool? isActive, string sort, string sortDir)
    {
        var assignableRoles = await GetAssignableRolesAsync();
        var usersQuery = await BuildFilteredUsersAsync(search, role, isActive, assignableRoles);
        usersQuery = ApplySort(usersQuery, sort, sortDir);

        var totalCount = await usersQuery.CountAsync();
        var users = await usersQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var items = await ToViewModelsAsync(users);
        return (items, totalCount);
    }

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string? search = null, string? role = null, bool? isActive = null, string sort = "email", string sortDir = "asc")
    {
        var (items, _) = await GetFilteredPageAsync(page, pageSize, search, role, isActive, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Full Name,Email,Roles,Faculty,College,Status");

        foreach (var u in items)
        {
            sb.AppendLine($"{(u.FullName ?? "").EscapeCsv()}," +
                           $"{u.Email.EscapeCsv()}," +
                           $"{string.Join("; ", u.Roles).EscapeCsv()}," +
                           $"{(u.FacultyName ?? "-").EscapeCsv()}," +
                           $"{(u.CollegeName ?? "-").EscapeCsv()}," +
                           $"{(u.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"Users_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string? search = null, string? role = null, bool? isActive = null, string sort = "email", string sortDir = "asc")
    {
        var (items, totalCount) = await GetFilteredPageAsync(page, pageSize, search, role, isActive, sort, sortDir);
        var (officeName, officeAddress, logoBytes) = await ResolveLetterheadAsync();

        var document = Document.Create(container =>
        {
            container.Page(pageCfg =>
            {
                pageCfg.Size(PageSizes.A4);
                pageCfg.Margin(30);
                pageCfg.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                pageCfg.Header().Column(header =>
                {
                    header.Item().ShowIf(c => c.PageNumber == 1).Column(letterhead =>
                    {
                        if (logoBytes != null)
                            letterhead.Item().AlignCenter().Width(70).Image(logoBytes).FitWidth();
                        letterhead.Item().PaddingTop(logoBytes != null ? 4 : 0).AlignCenter().Text("Far Western University")
                            .FontSize(17).Bold().FontColor("#1a5276");
                        letterhead.Item().PaddingTop(2).AlignCenter().Text(officeName)
                            .FontSize(10).SemiBold().FontColor(Colors.Grey.Darken3);
                        letterhead.Item().PaddingTop(1).AlignCenter().Text(officeAddress)
                            .FontSize(9).FontColor(Colors.Grey.Darken2);
                        letterhead.Item().PaddingTop(8).LineHorizontal(1).LineColor("#1a5276");
                    });
                });

                pageCfg.Content().PaddingTop(14).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(24);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2.4f);
                        columns.RelativeColumn(1.8f);
                        columns.RelativeColumn(1.4f);
                        columns.RelativeColumn(1.4f);
                        columns.RelativeColumn(0.9f);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("#").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Full Name").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Email").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Roles").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Faculty").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("College").Bold().FontSize(9);
                        header.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text("Status").Bold().FontSize(9);
                    });

                    var serial = (page - 1) * pageSize;
                    foreach (var item in items)
                    {
                        serial++;
                        table.Cell().Padding(6).Text(serial.ToString()).FontSize(9);
                        table.Cell().Padding(6).Text(item.FullName ?? "-").FontSize(9);
                        table.Cell().Padding(6).Text(item.Email).FontSize(9);
                        table.Cell().Padding(6).Text(item.Roles.Count > 0 ? string.Join(", ", item.Roles) : "-").FontSize(9);
                        table.Cell().Padding(6).Text(item.FacultyName ?? "-").FontSize(9);
                        table.Cell().Padding(6).Text(item.CollegeName ?? "-").FontSize(9);
                        table.Cell().Padding(6).Text(item.IsActive ? "Active" : "Inactive").FontSize(9);
                    }
                });

                pageCfg.Footer().Column(footer =>
                {
                    footer.Item().AlignCenter().Text("Far Western University - System Generated Report")
                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                    footer.Item().PaddingTop(3).AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });
        });

        var pdfBytes = document.GeneratePdf();
        var fileName = $"Users_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    private async Task<(string OfficeName, string OfficeAddress, byte[]? LogoBytes)> ResolveLetterheadAsync()
    {
        const string defaultOfficeName = "Office of Controller of Examinations";
        const string defaultOfficeAddress = "Mahendranagar, Kanchanpur, Nepal";

        var tenant = await context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.OfficeCode == tenantContext.TenantCode);

        var officeName = tenant != null && IsAscii(tenant.Name) && !string.IsNullOrWhiteSpace(tenant.Name)
            ? tenant.Name
            : defaultOfficeName;
        var officeAddress = tenant != null && IsAscii(tenant.Address) && !string.IsNullOrWhiteSpace(tenant.Address)
            ? tenant.Address
            : defaultOfficeAddress;

        byte[]? logoBytes = null;
        var configuredLogo = tenant?.BannerImagePath ?? tenant?.LogoPath;
        if (!string.IsNullOrWhiteSpace(configuredLogo) && env.WebRootPath != null)
        {
            var fullPath = Path.Combine(env.WebRootPath, configuredLogo.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath))
                logoBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        }
        if (logoBytes == null && env.WebRootPath != null)
        {
            var fallbackPath = Path.Combine(env.WebRootPath, "images", "fwu-logo-nobg.png");
            if (System.IO.File.Exists(fallbackPath))
                logoBytes = await System.IO.File.ReadAllBytesAsync(fallbackPath);
        }

        return (officeName, officeAddress, logoBytes);
    }

    private static bool IsAscii(string value) => value.All(c => c < 128);

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string? search = null, string? role = null, bool? isActive = null, string sort = "email", string sortDir = "asc")
    {
        var (items, _) = await GetFilteredPageAsync(page, pageSize, search, role, isActive, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Users");

        var headers = new[] { "Full Name", "Email", "Roles", "Faculty", "College", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
        }

        var row = 2;
        foreach (var u in items)
        {
            worksheet.Cell(row, 1).Value = u.FullName ?? "";
            worksheet.Cell(row, 2).Value = u.Email;
            worksheet.Cell(row, 3).Value = string.Join(", ", u.Roles);
            worksheet.Cell(row, 4).Value = u.FacultyName ?? "";
            worksheet.Cell(row, 5).Value = u.CollegeName ?? "";
            worksheet.Cell(row, 6).Value = u.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var fileName = $"Users_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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
            .Where(r => r != null && assignableRoles.Contains(r) && r != Role.Student);
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
        var callerRole = RoleRules.FromRoles(userContext.Roles);

        if (model.SelectedRole == Role.SuperAdmin)
            ModelState.AddModelError(nameof(model.SelectedRole), "Cannot create a Super Admin user.");

        if (model.SelectedRole == Role.Student)
            ModelState.AddModelError(nameof(model.SelectedRole), "Students are created via Student Registration or the batch create tool.");

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
                InvalidateRolesInScopeCache();
                await auditLogWriter.LogAsync(ActivityTypes.UserCreated,
                    $"Created user {user.Email} with role {model.SelectedRole}",
                    new { userId = user.Id, email = user.Email, role = model.SelectedRole },
                    entityName: "AppUser", entityId: user.Id);
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

            var targetRoles = await userManager.GetRolesAsync(user);
            user.Email = model.Email;
            // Student username is the registration number; keep it separate from the email.
            if (!targetRoles.Contains(Role.Student))
                user.UserName = model.Email;
            user.FullName = model.FullName;
            user.CollegeId = model.CollegeId;
            // CollegeAdmin cannot manage faculty assignments; preserve the existing value.
            if (!User.IsInRole(Role.CollegeAdmin))
                user.FacultyId = model.FacultyId;

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await auditLogWriter.LogAsync(ActivityTypes.UserUpdated,
                    $"Updated user {(user.FullName ?? user.Email)}",
                    new { userId = user.Id, email = user.Email },
                    entityName: "AppUser", entityId: user.Id);
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
        await auditLogWriter.LogAsync(ActivityTypes.UserStatusChanged,
            $"{(user.IsActive ? "Enabled" : "Disabled")} user {(user.FullName ?? user.Email)}",
            new { userId = user.Id, email = user.Email, isActive = user.IsActive },
            entityName: "AppUser", entityId: user.Id);

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
            {
                await userManager.DeleteAsync(user);
                await auditLogWriter.LogAsync(ActivityTypes.UserDeleted,
                    $"Deleted user {(user.FullName ?? user.Email)}",
                    new { userId = user.Id, email = user.Email },
                    entityName: "AppUser", entityId: user.Id);
            }

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

        if (toAdd.Count > 0 || toRemove.Count > 0)
        {
            InvalidateRolesInScopeCache();
            await auditLogWriter.LogAsync(ActivityTypes.UserRolesChanged,
                $"Updated roles for {(user.FullName ?? user.Email)}: added [{string.Join(", ", toAdd)}], removed [{string.Join(", ", toRemove)}]",
                new { userId = user.Id, added = toAdd, removed = toRemove },
                entityName: "AppUser", entityId: user.Id);
        }

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
            await auditLogWriter.LogAsync(ActivityTypes.UserPasswordResetByAdmin,
                $"Admin reset password for {(user.FullName ?? user.Email)}",
                new { userId = user.Id, email = user.Email, byAdmin = true },
                entityName: "AppUser", entityId: user.Id);
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
        var callerRole = RoleRules.FromRoles(userContext.Roles);
        if (callerRole != null)
            return RoleRules.CanManageTarget(callerRole, await userManager.GetRolesAsync(user));

        // Caller holds only dynamic/custom roles: permission-subset rule —
        // they may manage a user only if the target's permission set is a
        // subset of the caller's own, preventing privilege escalation.
        var callerId = userContext.UserId;
        if (callerId == null) return false;
        var callerPermissions = new HashSet<string>(await permissionService.GetUserPermissionsAsync(callerId));
        var targetPermissions = await permissionService.GetUserPermissionsAsync(user.Id);
        return targetPermissions.All(callerPermissions.Contains);
    }

    private async Task<IReadOnlySet<string>> GetAssignableRolesAsync()
    {
        var callerRole = RoleRules.FromRoles(userContext.Roles);
        if (callerRole != null)
            return RoleRules.AssignableRoles(callerRole);

        // Caller holds only dynamic/custom roles: permission-subset rule —
        // they may only assign roles whose permission set is a subset of
        // their own, matching the guard used by ManagePermissionsController.
        var callerId = userContext.UserId;
        if (callerId == null) return new HashSet<string>();
        var callerPermissions = new HashSet<string>(await permissionService.GetUserPermissionsAsync(callerId));

        // Load every role's effective permission names in a single query instead of one query per role.
        var rolePermissions = await context.Set<Domain.Entities.Permissions.RolePermission>()
            .AsNoTracking()
            .Include(rp => rp.Permission)
            .Where(rp => rp.Permission.IsActive)
            .Select(rp => new { rp.RoleId, rp.Permission.Name })
            .Distinct()
            .ToListAsync();

        var rolePermissionLookup = rolePermissions
            .GroupBy(x => x.RoleId)
            .ToDictionary(g => g.Key, g => (IReadOnlySet<string>)g.Select(x => x.Name).ToHashSet());

        var assignable = new HashSet<string>();
        foreach (var role in await roleManager.Roles.AsNoTracking().ToListAsync())
        {
            if (role.Name == null) continue;
            var rolePermissionSet = rolePermissionLookup.GetValueOrDefault(role.Id)
                ?? new HashSet<string>();
            if (rolePermissionSet.All(callerPermissions.Contains))
                assignable.Add(role.Name);
        }
        return assignable;
    }

    private async Task<IReadOnlyList<string>> GetRolesPresentInScopeAsync(IQueryable<AppUser> scopedUsers)
    {
        var cacheKey = RolesInScopeCacheKey;
        if (cache.TryGetValue(cacheKey, out IReadOnlyList<string>? cached) && cached != null)
            return cached;

        var scopedUserIds = scopedUsers.Select(u => u.Id);
        var roles = await context.UserRoles
            .AsNoTracking()
            .Where(ur => scopedUserIds.Contains(ur.UserId))
            .Join(context.Roles.AsNoTracking(), ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .Where(n => n != null)
            .Select(n => n!)
            .Distinct()
            .ToListAsync();

        cache.Set(cacheKey, roles, TimeSpan.FromMinutes(2));
        return roles;
    }

    private void InvalidateRolesInScopeCache() => cache.Remove(RolesInScopeCacheKey);

    private string RolesInScopeCacheKey => $"users_roles_in_scope_{tenantContext.TenantId}";

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
            .AsNoTracking()
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
            .AsNoTracking()
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(context.Roles.AsNoTracking(), ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
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
