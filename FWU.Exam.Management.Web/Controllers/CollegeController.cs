using System.Security.Claims;
using fwu_examination_management_system.Data;
using fwu_examination_management_system.Data.Models;
using fwu_examination_management_system.Data.Models.Colleges;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace fwu_examination_management_system.Controllers;

[Authorize(Roles = Role.SystemAdmin + "," + Role.Admin)]
public class CollegeController : Controller
{
    private const string MustChangePasswordClaimType = "must_change_password";

    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public CollegeController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var colleges = await _context.Colleges
            .Include(c => c.Organization)
            .Include(c => c.District)
            .Include(c => c.Area)
            .Include(c => c.CollegeType)
            .Include(c => c.CollegeProfile)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(colleges);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var college = await _context.Colleges
            .Include(c => c.Organization)
            .Include(c => c.District)
            .Include(c => c.Area)
            .Include(c => c.CollegeType)
            .Include(c => c.CollegeProfile)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (college == null)
            return NotFound();

        return View(college);
    }

    public async Task<IActionResult> Create()
    {
        return View(new College
        {
            IsActive = true,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Code,Name,CollegeNameNepali,ShortName,Email,Phone1,IsExamCenterOnly,IsActive,Remarks")] College college)
    {
        if (string.IsNullOrWhiteSpace(college.Email))
        {
            ModelState.AddModelError(nameof(college.Email), "College email is required.");
        }

        if (!ModelState.IsValid)
        {
            return View(college);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Colleges.Add(college);
            await _context.SaveChangesAsync();

            var existingUser = await _userManager.FindByEmailAsync(college.Email!);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(college.Email), "This email is already used by another login account.");
                await transaction.RollbackAsync();
                return View(college);
            }

            var initialPassword = BuildInitialPassword(college.Code);

            var collegeUser = new AppUser
            {
                UserName = college.Email,
                Email = college.Email,
                CollegeId = college.Id,
                EmailConfirmed = true,
                IsActive = true
            };

            var createUserResult = await _userManager.CreateAsync(collegeUser, initialPassword);
            if (!createUserResult.Succeeded)
            {
                foreach (var error in createUserResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                await transaction.RollbackAsync();
                return View(college);
            }

            if (!await _userManager.IsInRoleAsync(collegeUser, Role.Student))
                await _userManager.AddToRoleAsync(collegeUser, Role.Student);

            if (!await _userManager.IsInRoleAsync(collegeUser, Role.CollegeAdmin))
                await _userManager.AddToRoleAsync(collegeUser, Role.CollegeAdmin);

            await _userManager.AddClaimAsync(collegeUser, new Claim(MustChangePasswordClaimType, "true"));

            TempData["CollegeLoginEmail"] = college.Email;
            TempData["CollegeLoginPassword"] = initialPassword;
            TempData["CollegeName"] = college.Name;

            await transaction.CommitAsync();
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var college = await _context.Colleges.FindAsync(id);
        if (college == null)
            return NotFound();

        await PopulateLookupsAsync(college.DistrictId, college.AreaId, college.CollegeTypeId, college.CollegeProfileId, college.OrganizationId);
        return View(college);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Name,CollegeNameNepali,ShortName,Email,Phone1,CollegeTypeId,OrganizationId,IsExamCenterOnly,IsActive,Remarks")] College college)
    {
        if (id != college.Id)
            return NotFound();

        var existingCollege = await _context.Colleges.FindAsync(id);
        if (existingCollege == null)
            return NotFound();

        existingCollege.Code = college.Code;
        existingCollege.Name = college.Name;
        existingCollege.CollegeNameNepali = college.CollegeNameNepali;
        existingCollege.ShortName = college.ShortName;
        existingCollege.Email = college.Email;
        existingCollege.Phone1 = college.Phone1;
        existingCollege.CollegeTypeId = college.CollegeTypeId;
        existingCollege.OrganizationId = college.OrganizationId;
        existingCollege.IsExamCenterOnly = college.IsExamCenterOnly;
        existingCollege.IsActive = college.IsActive;
        existingCollege.Remarks = college.Remarks;

        await ValidateCollegeReferencesAsync(existingCollege, isEdit: true);

        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(existingCollege.DistrictId, existingCollege.AreaId, existingCollege.CollegeTypeId, existingCollege.CollegeProfileId, existingCollege.OrganizationId);
            return View(existingCollege);
        }

        try
        {
            _context.Update(existingCollege);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CollegeExists(existingCollege.Id))
                return NotFound();

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var college = await _context.Colleges
            .Include(c => c.District)
            .Include(c => c.Area)
            .Include(c => c.CollegeType)
            .Include(c => c.CollegeProfile)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (college == null)
            return NotFound();

        return View(college);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var college = await _context.Colleges.FindAsync(id);
        if (college == null)
            return RedirectToAction(nameof(Index));

        _context.Colleges.Remove(college);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private bool CollegeExists(int id)
    {
        return _context.Colleges.Any(c => c.Id == id);
    }

    private async Task ValidateCollegeReferencesAsync(College college, bool isEdit = false)
    {
        if (string.IsNullOrWhiteSpace(college.Email))
        {
            ModelState.AddModelError(nameof(college.Email), "College email is required.");
            return;
        }

        if (college.OrganizationId <= 0)
        {
            ModelState.AddModelError(nameof(college.OrganizationId), "No organization is available for college creation.");
        }
        else if (!await _context.Organizations.AnyAsync(o => o.Id == college.OrganizationId))
        {
            ModelState.AddModelError(nameof(college.OrganizationId), "Selected organization is invalid.");
        }

        if (college.DistrictId <= 0)
        {
            ModelState.AddModelError(nameof(college.DistrictId), "No district is available for college creation.");
        }
        else if (!await _context.Districts.AnyAsync(d => d.Id == college.DistrictId))
        {
            ModelState.AddModelError(nameof(college.DistrictId), "Selected district is invalid.");
        }

        if (college.AreaId <= 0)
        {
            ModelState.AddModelError(nameof(college.AreaId), "No area is available for college creation.");
        }
        else if (!await _context.Areas.AnyAsync(a => a.Id == college.AreaId))
        {
            ModelState.AddModelError(nameof(college.AreaId), "Selected area is invalid.");
        }

        if (college.CollegeProfileId <= 0)
        {
            ModelState.AddModelError(nameof(college.CollegeProfileId), "No college profile is available for college creation.");
        }
        else if (!await _context.CollegeProfiles.AnyAsync(cp => cp.Id == college.CollegeProfileId))
        {
            ModelState.AddModelError(nameof(college.CollegeProfileId), "Selected college profile is invalid.");
        }

        var existingEmailOwner = await _userManager.FindByEmailAsync(college.Email);
        if (existingEmailOwner != null)
        {
            var isSameCollegeUser = existingEmailOwner.CollegeId == college.Id;
            if (!isSameCollegeUser || !isEdit)
                ModelState.AddModelError(nameof(college.Email), "This email is already used by another login account.");
        }
    }

    private async Task NormalizeCollegeOrganizationAsync(College college)
    {
        if (college.OrganizationId <= 0)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.OrganizationId != null)
            {
                college.OrganizationId = currentUser.OrganizationId.Value;
            }
            else
            {
                college.OrganizationId = await _context.Organizations.OrderBy(o => o.Id).Select(o => o.Id).FirstOrDefaultAsync();
            }
        }

        if (college.DistrictId <= 0)
            college.DistrictId = await _context.Districts.OrderBy(d => d.Id).Select(d => d.Id).FirstOrDefaultAsync();

        if (college.AreaId <= 0)
            college.AreaId = await _context.Areas.OrderBy(a => a.Id).Select(a => a.Id).FirstOrDefaultAsync();

        if (college.CollegeProfileId <= 0)
            college.CollegeProfileId = await _context.CollegeProfiles.OrderBy(cp => cp.Id).Select(cp => cp.Id).FirstOrDefaultAsync();
    }

    private async Task<int> ResolveDefaultOrganizationIdAsync(AppUser? currentUser)
    {
        if (currentUser?.OrganizationId != null)
            return currentUser.OrganizationId.Value;

        return await _context.Organizations.OrderBy(o => o.Id).Select(o => o.Id).FirstOrDefaultAsync();
    }

    private async Task PopulateLookupsAsync(int? districtId = null, int? areaId = null, int? collegeTypeId = null, int? collegeProfileId = null, int? organizationId = null)
    {
        ViewData["DistrictId"] = new SelectList(
            await _context.Districts.OrderBy(d => d.DistrictName).ToListAsync(),
            "Id",
            "DistrictName",
            districtId);

        ViewData["AreaId"] = new SelectList(
            await _context.Areas.OrderBy(a => a.AreaName).ToListAsync(),
            "Id",
            "AreaName",
            areaId);

        ViewData["CollegeTypeId"] = new SelectList(
            await _context.CollegeTypes.OrderBy(ct => ct.Name).ToListAsync(),
            "Id",
            "Name",
            collegeTypeId);

        ViewData["OrganizationId"] = new SelectList(
            await _context.Organizations.OrderBy(o => o.Name).ToListAsync(),
            "Id",
            "Name",
            organizationId);

        var profiles = await _context.CollegeProfiles
            .OrderBy(cp => cp.Id)
            .Select(cp => new
            {
                cp.Id,
                DisplayName = string.IsNullOrWhiteSpace(cp.ContactPersonName)
                    ? $"Profile #{cp.Id}"
                    : $"Profile #{cp.Id} - {cp.ContactPersonName}"
            })
            .ToListAsync();

        ViewData["CollegeProfileId"] = new SelectList(profiles, "Id", "DisplayName", collegeProfileId);
    }

    private static string BuildInitialPassword(string code)
    {
        var seed = string.IsNullOrWhiteSpace(code) ? "College" : code.Trim();
        var lettersOnly = new string(seed.Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrWhiteSpace(lettersOnly))
            lettersOnly = "College";

        var prefix = char.ToUpperInvariant(lettersOnly[0]) + lettersOnly.Substring(1).ToLowerInvariant();
        return $"{prefix}@123aA";
    }
}
