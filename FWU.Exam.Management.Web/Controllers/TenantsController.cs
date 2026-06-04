using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Helpers;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Controllers;

[Authorize(Roles = "SuperAdmin")]
public class TenantsController(AppDbContext context, IFileUploadHelper fileUploadHelper, UserManager<AppUser> userManager) : Controller
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

    public IActionResult Create()
    {
        return View(new TenantCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TenantCreateViewModel viewModel, IFormFile? logoFile)
    {
        if (ModelState.IsValid)
        {
            var tenant = viewModel.Tenant;

            if (logoFile != null)
            {
                tenant.LogoPath = await fileUploadHelper.UploadAsync(logoFile);
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

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var tenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (tenant == null) return NotFound();

        return View(tenant);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Tenant tenant, IFormFile? logoFile)
    {
        if (id != tenant.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                if (logoFile != null)
                {
                    tenant.LogoPath = await fileUploadHelper.UploadAsync(logoFile);
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
        }
        return View(tenant);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var tenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (tenant == null) return NotFound();

        return View(tenant);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant != null)
        {
            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();
        }
        TempData["SuccessMessage"] = "Tenant deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
