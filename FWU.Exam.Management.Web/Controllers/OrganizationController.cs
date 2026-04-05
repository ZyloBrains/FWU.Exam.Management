using System.Security.Claims;
using fwu_examination_management_system.Data;
using fwu_examination_management_system.Data.Models;
using fwu_examination_management_system.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fwu_examination_management_system.Controllers;

[Authorize(Roles = Role.SystemAdmin)]
public class OrganizationController : Controller
{
    private const string MustChangePasswordClaimType = "must_change_password";

    private readonly ApplicationDbContext _context;
    private readonly IFileUploadHelper _fileUploadHelper;
    private readonly UserManager<AppUser> _userManager;

    public OrganizationController(ApplicationDbContext context, IFileUploadHelper fileUploadHelper, UserManager<AppUser> userManager)
    {
        _context = context;
        _fileUploadHelper = fileUploadHelper;
        _userManager = userManager;
    }

    // GET: Organization
    public async Task<IActionResult> Index()
    {
        return View(await _context.Organizations.ToListAsync());
    }

    // GET: Organization/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == id);
        if (organization == null)
            return NotFound();

        return View(organization);
    }

    // GET: Organization/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Organization/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,OfficeCode,ContactNumber,Address,Email")] Organization organization, IFormFile? logoFile)
    {
        if (ModelState.IsValid)
        {
            if (string.IsNullOrWhiteSpace(organization.Email))
            {
                ModelState.AddModelError(nameof(organization.Email), "Organization email is required to create login.");
                return View(organization);
            }

            var existingUser = await _userManager.FindByEmailAsync(organization.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(organization.Email), "This email is already used by another login account.");
                return View(organization);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                organization.LogoPath = await _fileUploadHelper.UploadAsync(logoFile) ?? string.Empty;
                _context.Add(organization);
                await _context.SaveChangesAsync();

                var initialPassword = BuildInitialPassword(organization.OfficeCode);
                var orgUser = new AppUser
                {
                    UserName = organization.Email,
                    Email = organization.Email,
                    OrganizationId = organization.Id,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var createUserResult = await _userManager.CreateAsync(orgUser, initialPassword);
                if (!createUserResult.Succeeded)
                {
                    foreach (var error in createUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    await transaction.RollbackAsync();
                    return View(organization);
                }

                if (!await _userManager.IsInRoleAsync(orgUser, Role.Admin))
                    await _userManager.AddToRoleAsync(orgUser, Role.Admin);

                if (!await _userManager.IsInRoleAsync(orgUser, Role.Student))
                    await _userManager.AddToRoleAsync(orgUser, Role.Student);

                await _userManager.AddClaimAsync(orgUser, new Claim(MustChangePasswordClaimType, "true"));

                await transaction.CommitAsync();

                TempData["OrgLoginEmail"] = organization.Email;
                TempData["OrgLoginPassword"] = initialPassword;
                TempData["OrgOfficeCode"] = organization.OfficeCode;

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        return View(organization);
    }

    // GET: Organization/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var organization = await _context.Organizations.FindAsync(id);
        if (organization == null)
            return NotFound();

        return View(organization);
    }

    // POST: Organization/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,OfficeCode,ContactNumber,Address,Email,LogoPath")] Organization organization, IFormFile? logoFile)
    {
        if (id != organization.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var uploadedPath = await _fileUploadHelper.UploadAsync(logoFile);
                if (uploadedPath != null)
                    organization.LogoPath = uploadedPath;

                _context.Update(organization);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrganizationExists(organization.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(organization);
    }

    // GET: Organization/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Id == id);
        if (organization == null)
            return NotFound();

        return View(organization);
    }

    // POST: Organization/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var organization = await _context.Organizations.FindAsync(id);
        if (organization != null)
            _context.Organizations.Remove(organization);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool OrganizationExists(int id)
    {
        return _context.Organizations.Any(o => o.Id == id);
    }

    private static string BuildInitialPassword(string officeCode)
    {
        var seed = string.IsNullOrWhiteSpace(officeCode) ? "Org" : officeCode.Trim();

        var lettersOnly = new string(seed.Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrWhiteSpace(lettersOnly))
            lettersOnly = "Org";

        var prefix = char.ToUpperInvariant(lettersOnly[0]) + lettersOnly.Substring(1).ToLowerInvariant();

        // Always includes: uppercase + lowercase + number + special char
        return $"{prefix}@123aA";
    }
}
