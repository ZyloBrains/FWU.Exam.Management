using System.Security.Claims;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[Authorize(Roles = Role.SystemAdmin)]
public class TenantController : Controller
{
    private readonly AppDbContext _context;
    private readonly IFileUploadHelper _fileUploadHelper;
    private readonly UserManager<AppUser> _userManager;

    public TenantController(AppDbContext context, IFileUploadHelper fileUploadHelper, UserManager<AppUser> userManager)
    {
        _context = context;
        _fileUploadHelper = fileUploadHelper;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var tenants = await _context.Tenants.AsNoTracking().ToListAsync();
        return View(tenants);
    }

    public IActionResult Create()
    {
        return View(new Tenant());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Tenant model, IFormFile? LogoFile)
    {
        if (ModelState.IsValid)
        {
            if (LogoFile != null)
            {
                model.LogoPath = await _fileUploadHelper.UploadAsync(LogoFile, "organization");
            }

            _context.Tenants.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null) return NotFound();
        return View(tenant);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Tenant model, IFormFile? LogoFile)
    {
        if (id != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var tenant = await _context.Tenants.FindAsync(id);
            if (tenant == null) return NotFound();

            tenant.Name = model.Name;
            tenant.OfficeCode = model.OfficeCode;
            tenant.ContactNumber = model.ContactNumber;
            tenant.Address = model.Address;
            tenant.Email = model.Email;
            tenant.TenantType = model.TenantType;
            tenant.IsActive = model.IsActive;

            if (LogoFile != null)
            {
                tenant.LogoPath = await _fileUploadHelper.UploadAsync(LogoFile, "organization");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var tenant = await _context.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        if (tenant == null) return NotFound();
        return View(tenant);
    }

    public async Task<IActionResult> Delete(int id)
    {
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
        return RedirectToAction(nameof(Index));
    }
}
