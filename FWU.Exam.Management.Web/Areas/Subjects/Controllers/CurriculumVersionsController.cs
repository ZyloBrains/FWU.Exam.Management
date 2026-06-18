using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Subjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Subjects.Controllers;

[Area("Subjects")]
[RequirePermission("curriculumversions.view")]
public class CurriculumVersionsController : Controller
{
    private readonly ICurriculumVersionService _curriculumVersionService;

    public CurriculumVersionsController(ICurriculumVersionService curriculumVersionService)
    {
        _curriculumVersionService = curriculumVersionService;
    }

    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Name", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await _curriculumVersionService.GetCurriculumVersionsAsync(page, pageSize, search, sort, sortDir);

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
        var items = await _curriculumVersionService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Name,Program,Effective Year,Status");

        foreach (var c in items)
        {
            sb.AppendLine($"{EscapeCsv(c.Name)}," +
                           $"{EscapeCsv(c.Program?.ProgramName ?? "-")}," +
                           $"{EscapeCsv(c.EffectiveAcademicYear?.AcademicYearName ?? "-")}," +
                           $"{(c.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"CurriculumVersions_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string search = null, string sort = "Name", string sortDir = "asc")
    {
        var items = await _curriculumVersionService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Curriculum Versions");

        var headers = new[] { "Name", "Program", "Effective Year", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var c in items)
        {
            worksheet.Cell(row, 1).Value = c.Name ?? "-";
            worksheet.Cell(row, 2).Value = c.Program?.ProgramName ?? "-";
            worksheet.Cell(row, 3).Value = c.EffectiveAcademicYear?.AcademicYearName ?? "-";
            worksheet.Cell(row, 4).Value = c.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        var fileName = $"CurriculumVersions_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "Name", string sortDir = "asc")
    {
        var (items, totalCount) = await _curriculumVersionService.GetCurriculumVersionsAsync(page, pageSize, search, sort, sortDir);

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

        var curriculumVersion = await _curriculumVersionService.GetCurriculumVersionByIdAsync(id.Value);
        if (curriculumVersion == null) return NotFound();

        return View(curriculumVersion);
    }

    [RequirePermission("curriculumversions.create")]
    public async Task<IActionResult> Create()
    {
        var (programs, academicYears) = await _curriculumVersionService.GetSelectListsAsync();
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName");
        ViewData["EffectiveAcademicYearId"] = new SelectList(academicYears, "Id", "AcademicYearName");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("curriculumversions.create")]
    public async Task<IActionResult> Create([Bind("Id,Name,ProgramId,EffectiveAcademicYearId,Description,IsActive")] CurriculumVersion curriculumVersion)
    {
        if (ModelState.IsValid)
        {
            await _curriculumVersionService.CreateCurriculumVersionAsync(curriculumVersion);
            return RedirectToAction(nameof(Index));
        }
        var (programs, academicYears) = await _curriculumVersionService.GetSelectListsAsync(curriculumVersion.ProgramId, curriculumVersion.EffectiveAcademicYearId);
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", curriculumVersion.ProgramId);
        ViewData["EffectiveAcademicYearId"] = new SelectList(academicYears, "Id", "AcademicYearName", curriculumVersion.EffectiveAcademicYearId);
        return View(curriculumVersion);
    }

    [RequirePermission("curriculumversions.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var curriculumVersion = await _curriculumVersionService.GetCurriculumVersionByIdAsync(id.Value);
        if (curriculumVersion == null) return NotFound();

        var (programs, academicYears) = await _curriculumVersionService.GetSelectListsAsync(curriculumVersion.ProgramId, curriculumVersion.EffectiveAcademicYearId);
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", curriculumVersion.ProgramId);
        ViewData["EffectiveAcademicYearId"] = new SelectList(academicYears, "Id", "AcademicYearName", curriculumVersion.EffectiveAcademicYearId);
        return View(curriculumVersion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("curriculumversions.edit")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ProgramId,EffectiveAcademicYearId,Description,IsActive")] CurriculumVersion curriculumVersion)
    {
        if (id != curriculumVersion.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await _curriculumVersionService.UpdateCurriculumVersionAsync(curriculumVersion);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _curriculumVersionService.CurriculumVersionExistsAsync(curriculumVersion.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        var (programs, academicYears) = await _curriculumVersionService.GetSelectListsAsync(curriculumVersion.ProgramId, curriculumVersion.EffectiveAcademicYearId);
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", curriculumVersion.ProgramId);
        ViewData["EffectiveAcademicYearId"] = new SelectList(academicYears, "Id", "AcademicYearName", curriculumVersion.EffectiveAcademicYearId);
        return View(curriculumVersion);
    }

    [RequirePermission("curriculumversions.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var curriculumVersion = await _curriculumVersionService.GetCurriculumVersionByIdAsync(id.Value);
        if (curriculumVersion == null) return NotFound();

        return View(curriculumVersion);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [RequirePermission("curriculumversions.delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _curriculumVersionService.DeleteCurriculumVersionAsync(id);
        return RedirectToAction(nameof(Index));
    }
        [RequirePermission("PLACEHOLDER_PERMISSION")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await _curriculumVersionService.DeleteCurriculumVersionAsync(id); return Json(new { success = true, message = "Curriculum version deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
