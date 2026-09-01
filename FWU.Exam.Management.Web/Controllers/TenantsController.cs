using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Infrastructure.Services;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Web.ViewModels;
using FWU.Exam.Management.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Controllers;

[RequirePermission("tenants.view")]
public class TenantsController(AppDbContext context, UserManager<AppUser> userManager, IEmailService emailService, IFileUploadHelper fileUploadHelper) : Controller
{
    private readonly AppDbContext _context = context;
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly IFileUploadHelper _fileUploadHelper = fileUploadHelper;

    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "Name", string sortDir = "asc", int pageSize = 10)
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
    public IActionResult Create()
    {
        return View(new TenantCreateViewModel());
    }

    [RequirePermission("tenants.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TenantCreateViewModel viewModel, IFormFile? bannerImage, IFormFile? logoImage)
    {
        if (ModelState.IsValid)
        {
            var tenant = viewModel.Tenant;

            if (bannerImage != null && bannerImage.Length > 0)
            {
                try
                {
                    var bannerPath = await _fileUploadHelper.UploadAsync(bannerImage, "uploads/banners", Helpers.FileUploadHelper.MaxDocumentSizeBytes);
                    if (bannerPath != null)
                        tenant.BannerImagePath = bannerPath;
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(viewModel);
                }
            }

            if (logoImage != null && logoImage.Length > 0)
            {
                try
                {
                    var logoPath = await _fileUploadHelper.UploadAsync(logoImage, "uploads/logos", Helpers.FileUploadHelper.MaxDocumentSizeBytes);
                    if (logoPath != null)
                        tenant.LogoPath = logoPath;
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(viewModel);
                }
            }

            _context.Add(tenant);
            await _context.SaveChangesAsync();

            var adminUser = new AppUser
            {
                UserName = viewModel.AdminEmail,
                Email = viewModel.AdminEmail,
                EmailConfirmed = true,
                FullName = viewModel.AdminFullName,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(adminUser, viewModel.AdminPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, Role.FacultyAdmin);

                try
                {
                    if (!string.IsNullOrWhiteSpace(viewModel.AdminEmail))
                    {
                        var emailBody = EmailTemplateHelper.TenantAccountCreated(viewModel.AdminFullName, tenant.Name, tenant.OfficeCode, viewModel.AdminEmail);
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
    public async Task<IActionResult> Edit(int id, Tenant tenant, IFormFile? bannerImage, IFormFile? logoImage)
    {
        if (id != tenant.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                if (bannerImage != null && bannerImage.Length > 0)
                {
                    var bannerPath = await _fileUploadHelper.UploadAsync(bannerImage, "uploads/banners", Helpers.FileUploadHelper.MaxDocumentSizeBytes);
                    if (bannerPath != null)
                        tenant.BannerImagePath = bannerPath;
                }

                if (logoImage != null && logoImage.Length > 0)
                {
                    var logoPath = await _fileUploadHelper.UploadAsync(logoImage, "uploads/logos", Helpers.FileUploadHelper.MaxDocumentSizeBytes);
                    if (logoPath != null)
                        tenant.LogoPath = logoPath;
                }

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
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(tenant);
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
            var collegeFaculties = await _context.CollegeFaculties.Where(cf => cf.TenantId == id).ToListAsync();
            _context.CollegeFaculties.RemoveRange(collegeFaculties);

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
            ["Colleges"] = await _context.CollegeFaculties
                .Where(cf => cf.TenantId == tenantId)
                .Select(cf => cf.CollegeId)
                .Distinct()
                .CountAsync(),
            ["College Programs"] = await _context.Set<CollegeProgram>().CountAsync(cp => cp.TenantId == tenantId),
            ["Academic Years"] = await _context.AcademicYears.CountAsync(),
            ["Students"] = await _context.StudentRegistrations.CountAsync(s => s.TenantId == tenantId),
            ["Exam Schedules"] = await _context.ExamSchedules.CountAsync(e => e.TenantId == tenantId),
        };
    }
    [RequirePermission("tenants.edit")]
    public async Task<IActionResult> ControllerSignature(int? id)
    {
        if (id == null) return NotFound();

        var tenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (tenant == null) return NotFound();

        return View(tenant);
    }

    [RequirePermission("tenants.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ControllerSignature(int id, IFormFile? signature)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null) return NotFound();

        if (signature != null && signature.Length > 0)
        {
            try
            {
                var signaturePath = await _fileUploadHelper.UploadAsync(signature, "uploads/controller-signatures", Helpers.FileUploadHelper.MaxDocumentSizeBytes, Helpers.FileUploadHelper.ImageOnlyExtensions);
                if (signaturePath != null)
                    tenant.ControllerSignaturePath = signaturePath;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Controller signature updated successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
        }
        else
        {
            TempData["ErrorMessage"] = "Please select a file to upload.";
        }

        return RedirectToAction(nameof(ControllerSignature), new { id });
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

            var collegeFaculties = await _context.CollegeFaculties.Where(cf => cf.TenantId == id).ToListAsync();
            _context.CollegeFaculties.RemoveRange(collegeFaculties);

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

    [RequirePermission("tenants.edit")]
    public async Task<IActionResult> ResetPassword(int? id)
    {
        if (id == null) return NotFound();

        var tenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (tenant == null) return NotFound();

        var adminUser = await _userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == tenant.Email);

        if (adminUser == null)
        {
            TempData["ErrorMessage"] = "No admin user found for this tenant.";
            return RedirectToAction(nameof(Index));
        }

        var model = new TenantResetPasswordViewModel
        {
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            OfficeCode = tenant.OfficeCode,
            AdminEmail = tenant.Email
        };

        return View(model);
    }

    [RequirePermission("tenants.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(TenantResetPasswordViewModel viewModel)
    {
        if (!ModelState.IsValid)
            return View(viewModel);

        var tenant = await _context.Tenants.FindAsync(viewModel.TenantId);
        if (tenant == null) return NotFound();

        var adminUser = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Email == tenant.Email);

        if (adminUser == null)
        {
            TempData["ErrorMessage"] = "No admin user found for this tenant.";
            return RedirectToAction(nameof(Index));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(adminUser);
        var result = await _userManager.ResetPasswordAsync(adminUser, token, viewModel.NewPassword);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = $"Password reset successfully for '{tenant.Name}' admin user.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(viewModel);
    }

}
