using fwu_examination_management_system.Data;
using fwu_examination_management_system.Data.Models;
using fwu_examination_management_system.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fwu_examination_management_system.Controllers;

public class OrganizationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileUploadHelper _fileUploadHelper;

    public OrganizationController(ApplicationDbContext context, IFileUploadHelper fileUploadHelper)
    {
        _context = context;
        _fileUploadHelper = fileUploadHelper;
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
            organization.LogoPath = await _fileUploadHelper.UploadAsync(logoFile) ?? string.Empty;
            _context.Add(organization);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
}
