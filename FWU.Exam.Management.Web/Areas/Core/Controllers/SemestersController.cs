using System.Text;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Semesters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("semesters.view")]
public class SemestersController(ISemesterService semesterService, IAcademicYearService academicYearService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Name", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await semesterService.GetSemestersAsync(page, pageSize, search, sort, sortDir);

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

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "Name", string sortDir = "asc")
    {
        var items = await semesterService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Code,Name,Number,Year,Start Date,End Date,Remark");

        foreach (var s in items)
        {
            sb.AppendLine($"{EscapeCsv(s.Code)}," +
                           $"{EscapeCsv(s.Name)}," +
                           $"{s.Number}," +
                           $"{s.Year}," +
                           $"{s.StartDate:yyyy-MM-dd}," +
                           $"{s.EndDate:yyyy-MM-dd}," +
                           $"{EscapeCsv(s.Remark)}");
        }

        var fileName = $"Semesters_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "Name", string sortDir = "asc")
    {
        var (items, totalCount) = await semesterService.GetSemestersAsync(page, pageSize, search, sort, sortDir);

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

        var semester = await semesterService.GetSemesterByIdAsync(id.Value);
        if (semester == null) return NotFound();

        return View(semester);
    }

    [RequirePermission("semesters.create")]
    public async Task<IActionResult> Create()
    {
        ViewData["AcademicYearId"] = new SelectList(await GetAcademicYearsAsync(), "Id", "AcademicYearName");
        return View();
    }

    [RequirePermission("semesters.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Code,Name,Number,Year,StartDate,EndDate,Remark,AcademicYearId")] Semester semester)
    {
        if (ModelState.IsValid)
        {
            await semesterService.CreateSemesterAsync(semester);
            return RedirectToAction(nameof(Index));
        }
        ViewData["AcademicYearId"] = new SelectList(await GetAcademicYearsAsync(), "Id", "AcademicYearName", semester.AcademicYearId);
        return View(semester);
    }

    [RequirePermission("semesters.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var semester = await semesterService.GetSemesterByIdAsync(id.Value);
        if (semester == null) return NotFound();

        ViewData["AcademicYearId"] = new SelectList(await GetAcademicYearsAsync(), "Id", "AcademicYearName", semester.AcademicYearId);
        return View(semester);
    }

    [RequirePermission("semesters.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Name,Number,Year,StartDate,EndDate,Remark,AcademicYearId")] Semester semester)
    {
        if (id != semester.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await semesterService.UpdateSemesterAsync(semester);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await semesterService.SemesterExistsAsync(semester.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["AcademicYearId"] = new SelectList(await GetAcademicYearsAsync(), "Id", "AcademicYearName", semester.AcademicYearId);
        return View(semester);
    }

    [RequirePermission("semesters.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var semester = await semesterService.GetSemesterByIdAsync(id.Value);
        if (semester == null) return NotFound();

        return View(semester);
    }

    [RequirePermission("semesters.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await semesterService.DeleteSemesterAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<Domain.Entities.AcademicYear>> GetAcademicYearsAsync()
    {
        var (items, _) = await academicYearService.GetAllAcademicYearsAsync(1, int.MaxValue, null);
        return items;
    }
}
