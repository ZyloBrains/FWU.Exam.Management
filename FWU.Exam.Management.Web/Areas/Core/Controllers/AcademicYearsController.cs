using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
public class AcademicYearsController : Controller
{
    private readonly IAcademicYearService _academicYearService;

    public AcademicYearsController(IAcademicYearService academicYearService)
    {
        _academicYearService = academicYearService;
    }

    public async Task<IActionResult> Index()
    {
        var academicYears = await _academicYearService.GetAllAcademicYearsAsync();
        return View(academicYears);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var academicYear = await _academicYearService.GetAcademicYearByIdAsync(id.Value);
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
            await _academicYearService.CreateAcademicYearAsync(academicYear);
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

        var academicYear = await _academicYearService.GetAcademicYearByIdAsync(id.Value);
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
                await _academicYearService.UpdateAcademicYearAsync(academicYear);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _academicYearService.AcademicYearExistsAsync(academicYear.Id))
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

        var academicYear = await _academicYearService.GetAcademicYearByIdAsync(id.Value);
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
        await _academicYearService.DeleteAcademicYearAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
