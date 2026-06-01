using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Controllers;

[Authorize(Roles = "SuperAdmin")]
public class TenantsController : Controller
{
    private readonly AppDbContext _context;

    public TenantsController(AppDbContext context)
    {
        _context = context;
    }

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
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,OfficeCode,ContactNumber,Address,Email,LogoPath,TenantType,IsActive")] Tenant tenant)
    {
        if (ModelState.IsValid)
        {
            _context.Add(tenant);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tenant created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(tenant);
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
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,OfficeCode,ContactNumber,Address,Email,LogoPath,TenantType,IsActive")] Tenant tenant)
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
