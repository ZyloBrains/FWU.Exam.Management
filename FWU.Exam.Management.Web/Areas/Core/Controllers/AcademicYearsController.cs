using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

using Microsoft.AspNetCore.Authorization;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[Authorize(Roles = "SuperAdmin,FacultyAdmin")]
public class AcademicYearsController(IAcademicYearService academicYearService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, int pageSize = 10)
    {
        // The service currently returns a List<AcademicYear>. Do not attempt to deconstruct it.
        var (Items, TotalCount) = await academicYearService.GetAllAcademicYearsAsync(page, pageSize,search);

        // If you need the total count across all pages, update the service to return it.
        //var totalCount = items?.Count ?? 0;

        ViewBag.TotalCount = TotalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)TotalCount / pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        //ViewBag.Sort = sort;
        //ViewBag.SortDir = sortDir;

        return View(Items);
    }


    // Helper to escape CSV fields
    private string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    // Export to PDF (browser print)
    // Export to CSV – only the current page
    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null)
    {
        var (Items, TotalCount) = await academicYearService.GetAllAcademicYearsAsync(page, pageSize,search);

    
        var sb = new StringBuilder();
        sb.AppendLine("Code,Code (Nepali),Name,Name (Nepali),Remark,Running,Active");
        foreach (var item in Items)
        {
            sb.AppendLine($"{item.AcademicYearCode}," +
                          $"{EscapeCsv(item.AcademicYearCodeNepali)}," +
                          $"{EscapeCsv(item.AcademicYearName)}," +
                          $"{EscapeCsv(item.AcademicYearNameNepali)}," +
                          $"{EscapeCsv(item.Remark)}," +
                          $"{(item.IsRunning ? "Yes" : "No")}," +
                          $"{(item.IsActive ? "Active" : "Inactive")}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", "AcademicYears.csv");
    }

    // Export to PDF – only the current page (using browser print)
    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null)
    {
        var (Items, TotalCount) = await academicYearService.GetAllAcademicYearsAsync(page,pageSize,search);


        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = TotalCount;
        ViewBag.Search = search;
        //ViewBag.Sort = sort;
        //ViewBag.SortDir = sortDir;
        return View("PrintPdf", Items);
    }
    // GET: AcademicYears/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var academicYear = await academicYearService.GetAcademicYearByIdAsync(id.Value);
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
            await academicYearService.CreateAcademicYearAsync(academicYear);
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

        var academicYear = await academicYearService.GetAcademicYearByIdAsync(id.Value);
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
                await academicYearService.UpdateAcademicYearAsync(academicYear);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await academicYearService.AcademicYearExistsAsync(academicYear.Id))
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

        var academicYear = await academicYearService.GetAcademicYearByIdAsync(id.Value);
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
        await academicYearService.DeleteAcademicYearAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
