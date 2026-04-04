using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using fwu_examination_management_system.Data;
using fwu_examination_management_system.Data.Models;

namespace fwu_examination_management_system.Controllers;

public class ProvincesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProvincesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Provinces.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var province = await _context.Provinces
            .FirstOrDefaultAsync(m => m.Id == id);
        if (province == null)
        {
            return NotFound();
        }

        return View(province);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ProvinceName,IsActive")] Province province)
    {
        if (ModelState.IsValid)
        {
            _context.Add(province);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(province);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var province = await _context.Provinces.FindAsync(id);
        if (province == null)
        {
            return NotFound();
        }
        return View(province);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ProvinceName,IsActive")] Province province)
    {
        if (id != province.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(province);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProvinceExists(province.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(province);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var province = await _context.Provinces
            .FirstOrDefaultAsync(m => m.Id == id);
        if (province == null)
        {
            return NotFound();
        }

        return View(province);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var province = await _context.Provinces.FindAsync(id);
        if (province != null)
        {
            _context.Provinces.Remove(province);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ProvinceExists(int id)
    {
        return _context.Provinces.Any(e => e.Id == id);
    }
}
