using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Controllers;

[RequirePermission("tenants.view")]
public class TenantsController(AppDbContext context, UserManager<AppUser> userManager, IEmailService emailService) : Controller
{
    private readonly AppDbContext _context = context;
    private readonly UserManager<AppUser> _userManager = userManager;

    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Name", string sortDir = "asc", int pageSize = 10)
    {
        var query = _context.Tenants.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(t => t.Name.Contains(search) || t.OfficeCode.Contains(search) || t.Email.Contains(search));
        }

        query = sortDir == "asc"
            ? query.OrderBy(t => EF.Property<object>(t, sort))
            : query.OrderByDescending(t => EF.Property<object>(t, sort));

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking().ToListAsync();

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }

    [RequirePermission("tenants.create")]
    public async Task<IActionResult> Create()
    {
        var model = new TenantCreateViewModel
        {
            FacultyList = await GetAvailableFacultiesAsync()
        };
        return View(model);
    }

    [RequirePermission("tenants.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TenantCreateViewModel viewModel)
    {
        viewModel.FacultyList = await GetAvailableFacultiesAsync();

        if (viewModel.Tenant.TenantType == TenantType.Standard && viewModel.SelectedFacultyId == null)
        {
            ModelState.AddModelError(nameof(viewModel.SelectedFacultyId), "Please select a faculty for a Standard tenant.");
        }

        if (ModelState.IsValid)
        {
            var tenant = viewModel.Tenant;

            _context.Add(tenant);
            await _context.SaveChangesAsync();

            var adminUser = new AppUser
            {
                UserName = viewModel.AdminEmail,
                Email = viewModel.AdminEmail,
                EmailConfirmed = true,
                FullName = viewModel.AdminFullName,
                IsActive = true,
                FacultyId = viewModel.SelectedFacultyId
            };

            var result = await _userManager.CreateAsync(adminUser, viewModel.AdminPassword);
            if (result.Succeeded)
            {
                if (viewModel.SelectedFacultyId.HasValue)
                {
                    var faculty = await _context.Faculties.FindAsync(viewModel.SelectedFacultyId.Value);
                    if (faculty != null)
                    {
                        faculty.TenantId = tenant.Id;
                    }
                    await _context.SaveChangesAsync();
                }

                await _userManager.AddToRoleAsync(adminUser, Role.FacultyAdmin);

                try
                {
                    if (!string.IsNullOrWhiteSpace(viewModel.AdminEmail))
                    {
                        var emailBody = $@"
                            <h3>Dear {viewModel.AdminFullName},</h3>
                            <p>Your tenant account has been created successfully.</p>
                            <p><strong>Tenant Details:</strong></p>
                            <ul>
                                <li><strong>Tenant:</strong> {tenant.Name}</li>
                                <li><strong>Office Code:</strong> {tenant.OfficeCode}</li>
                            </ul>
                            <p><strong>Login:</strong> Please use your email address {viewModel.AdminEmail} to log in. If you have not set your password yet, use the 'Forgot Password' option on the login page to set your password.</p>
                            <br/>
                            <p>Regards,<br/>Far-Western University</p>";
                        await emailService.SendEmailAsync(viewModel.AdminEmail, "Tenant Account Created - Login Instructions", emailBody);
                    }
                }
                catch
                {
                    // Email failure does not block tenant creation
                }

                TempData["SuccessMessage"] = $"Tenant '{tenant.Name}' created successfully with admin user '{viewModel.AdminEmail}'.";
            }
            else
            {
                _context.Tenants.Remove(tenant);
                await _context.SaveChangesAsync();
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(viewModel);
            }

            return RedirectToAction(nameof(Index));
        }
        return View(viewModel);
    }

    private async Task<IEnumerable<SelectListItem>> GetAvailableFacultiesAsync()
    {
        return await _context.Faculties
            .Where(f => f.TenantId == null)
            .OrderBy(f => f.Name)
            .Select(f => new SelectListItem
            {
                Value = f.Id.ToString(),
                Text = f.Name + " (" + f.OfficeCode + ")"
            })
            .ToListAsync();
    }

    [RequirePermission("tenants.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var tenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (tenant == null) return NotFound();

        return View(tenant);
    }

    [RequirePermission("tenants.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Tenant tenant)
    {
        if (id != tenant.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(tenant);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tenant updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Tenants.AnyAsync(t => t.Id == id))
                    return NotFound();
                throw;
            }
        }
        return View(tenant);
    }

    [RequirePermission("tenants.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var tenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (tenant == null) return NotFound();

        var dependencyCounts = await GetDependencyCountsAsync(id.Value);
        ViewBag.DependencyCounts = dependencyCounts;
        ViewBag.HasDependencies = dependencyCounts.Values.Sum() > 0;

        return View(tenant);
    }

    [RequirePermission("tenants.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null) return NotFound();

        try
        {
            var faculties = await _context.Faculties.Where(f => f.TenantId == id).ToListAsync();
            _context.Faculties.RemoveRange(faculties);

            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tenant deleted successfully!";
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "Cannot delete this tenant because it has associated data. Remove or reassign the dependent records first.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<Dictionary<string, int>> GetDependencyCountsAsync(int tenantId)
    {
        return new Dictionary<string, int>
        {
            ["Faculties"] = await _context.Faculties.CountAsync(f => f.TenantId == tenantId),
            ["Colleges"] = await _context.Set<College>().CountAsync(c => c.TenantId == tenantId),
            ["College Programs"] = await _context.Set<CollegeProgram>().CountAsync(cp => cp.TenantId == tenantId),
            ["Academic Years"] = await _context.AcademicYears.CountAsync(),
            ["Students"] = await _context.StudentRegistrations.CountAsync(s => s.TenantId == tenantId),
            ["Exam Schedules"] = await _context.ExamSchedules.CountAsync(e => e.TenantId == tenantId),
        };
    }
        [RequirePermission("tenants.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null)
                return Json(new { success = false, message = "Tenant not found." });

            var faculties = await _context.Faculties.Where(f => f.TenantId == id).ToListAsync();
            _context.Faculties.RemoveRange(faculties);
            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Tenant deleted successfully!" });
        }
        catch (DbUpdateException)
        {
            return Json(new { success = false, message = "Cannot delete this tenant because it has associated data. Remove or reassign the dependent records first." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

}
