using System.Text;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Controllers;

public class FacultiesController : Controller
{
    private readonly IFacultyService _facultyService;

    public FacultiesController(IFacultyService facultyService)
    {
        _facultyService = facultyService;
    }

    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "FacultyName", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await _facultyService.GetFacultiesAsync(page, pageSize, search, sort, sortDir);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }

    private string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "FacultyName", string sortDir = "asc")
    {
        var items = await _facultyService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Faculty Code,Faculty Name,Short Name,Remarks,Status");

        foreach (var f in items)
        {
            sb.AppendLine($"{EscapeCsv(f.FacultyCode)}," +
                           $"{EscapeCsv(f.FacultyName)}," +
                           $"{EscapeCsv(f.ShortName)}," +
                           $"{EscapeCsv(f.Remarks)}," +
                           $"{(f.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"Faculties_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "FacultyName", string sortDir = "asc")
    {
        var (items, totalCount) = await _facultyService.GetFacultiesAsync(page, pageSize, search, sort, sortDir);

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View("PrintPdf", items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var faculty = await _facultyService.GetFacultyByIdAsync(id.Value);
        if (faculty == null) return NotFound();

        return View(faculty);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,FacultyCode,FacultyName,ShortName,Remarks,IsActive")] Faculty faculty)
    {
        if (ModelState.IsValid)
        {
            await _facultyService.CreateFacultyAsync(faculty);
            return RedirectToAction(nameof(Index));
        }
        return View(faculty);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var faculty = await _facultyService.GetFacultyByIdAsync(id.Value);
        if (faculty == null) return NotFound();

        return View(faculty);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,FacultyCode,FacultyName,ShortName,Remarks,IsActive")] Faculty faculty)
    {
        if (id != faculty.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await _facultyService.UpdateFacultyAsync(faculty);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _facultyService.FacultyExistsAsync(faculty.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(faculty);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var faculty = await _facultyService.GetFacultyByIdAsync(id.Value);
        if (faculty == null) return NotFound();

        return View(faculty);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _facultyService.DeleteFacultyAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
