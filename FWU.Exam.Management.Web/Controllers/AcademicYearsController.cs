using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Web.Controllers;

public class AcademicYearsController : Controller
{
    private readonly AppDbContext _context;

    public AcademicYearsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.AcademicYears.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var academicYear = await _context.AcademicYears
            .FirstOrDefaultAsync(m => m.Id == id);
        if (academicYear == null)
        {
            return NotFound();
        }

        return View(academicYear);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,AcademicYearCode,AcademicYearCodeNepali,AcademicYearName,AcademicYearNameNepali,Remark,IsRunning,IsActive")] AcademicYear academicYear)
    {
        if (ModelState.IsValid)
        {
            _context.Add(academicYear);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(academicYear);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var academicYear = await _context.AcademicYears.FindAsync(id);
        if (academicYear == null)
        {
            return NotFound();
        }
        return View(academicYear);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,AcademicYearCode,AcademicYearCodeNepali,AcademicYearName,AcademicYearNameNepali,Remark,IsRunning,IsActive")] AcademicYear academicYear)
    {
        if (id != academicYear.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(academicYear);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AcademicYearExists(academicYear.Id))
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
        return View(academicYear);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var academicYear = await _context.AcademicYears
            .FirstOrDefaultAsync(m => m.Id == id);
        if (academicYear == null)
        {
            return NotFound();
        }

        return View(academicYear);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var academicYear = await _context.AcademicYears.FindAsync(id);
        if (academicYear != null)
        {
            _context.AcademicYears.Remove(academicYear);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool AcademicYearExists(int id)
    {
        return _context.AcademicYears.Any(e => e.Id == id);
    }
}
