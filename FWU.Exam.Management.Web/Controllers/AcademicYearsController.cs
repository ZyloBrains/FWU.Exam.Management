using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using fwu_examination_management_system.Data;
using fwu_examination_management_system.Data.Models;

namespace fwu_examination_management_system.Controllers;

public class AcademicYearsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AcademicYearsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: AcademicYears
    public async Task<IActionResult> Index()
    {
        return View(await _context.AcademicYears.ToListAsync());
    }

    // GET: AcademicYears/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var academicYear = await _context.AcademicYears
            .FirstOrDefaultAsync(m => m.AcademicYearId == id);
        if (academicYear == null)
        {
            return NotFound();
        }

        return View(academicYear);
    }

    // GET: AcademicYears/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: AcademicYears/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("AcademicYearId,AcademicYearCode,AcademicYearCodeNepali,AcademicYearName,AcademicYearNameNepali,Remark,IsRunning,IsActive")] AcademicYear academicYear)
    {
        if (ModelState.IsValid)
        {
            _context.Add(academicYear);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(academicYear);
    }

    // GET: AcademicYears/Edit/5
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

    // POST: AcademicYears/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("AcademicYearId,AcademicYearCode,AcademicYearCodeNepali,AcademicYearName,AcademicYearNameNepali,Remark,IsRunning,IsActive")] AcademicYear academicYear)
    {
        if (id != academicYear.AcademicYearId)
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
                if (!AcademicYearExists(academicYear.AcademicYearId))
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

    // GET: AcademicYears/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var academicYear = await _context.AcademicYears
            .FirstOrDefaultAsync(m => m.AcademicYearId == id);
        if (academicYear == null)
        {
            return NotFound();
        }

        return View(academicYear);
    }

    // POST: AcademicYears/Delete/5
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
        return _context.AcademicYears.Any(e => e.AcademicYearId == id);
    }
}
