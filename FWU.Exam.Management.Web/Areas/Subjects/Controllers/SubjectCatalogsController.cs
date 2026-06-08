using System.Text;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Subjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;

namespace FWU.Exam.Management.Web.Areas.Subjects.Controllers;

[Area("Subjects")]
[Authorize(Roles = "SuperAdmin")]
public class SubjectCatalogsController : Controller
{
    private readonly ISubjectCatalogService _subjectCatalogService;

    public SubjectCatalogsController(ISubjectCatalogService subjectCatalogService)
    {
        _subjectCatalogService = subjectCatalogService;
    }

    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "SubjectName", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await _subjectCatalogService.GetSubjectCatalogsAsync(page, pageSize, search, sort, sortDir);

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

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "SubjectName", string sortDir = "asc")
    {
        var items = await _subjectCatalogService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Code,Subject Name,Short Name,Credit Hours,Type,Status");

        foreach (var s in items)
        {
            sb.AppendLine($"{EscapeCsv(s.SubjectCode)}," +
                           $"{EscapeCsv(s.SubjectName)}," +
                           $"{EscapeCsv(s.ShortName ?? "-")}," +
                           $"{s.CreditHours}," +
                           $"{EscapeCsv(s.SubjectType?.Name ?? "-")}," +
                           $"{(s.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"SubjectCatalogs_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "SubjectName", string sortDir = "asc")
    {
        var (items, totalCount) = await _subjectCatalogService.GetSubjectCatalogsAsync(page, pageSize, search, sort, sortDir);

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

        var subjectCatalog = await _subjectCatalogService.GetSubjectCatalogByIdAsync(id.Value);
        if (subjectCatalog == null) return NotFound();

        return View(subjectCatalog);
    }

    public async Task<IActionResult> Create()
    {
        var subjectTypes = await _subjectCatalogService.GetSelectListsAsync();
        ViewData["SubjectTypeId"] = new SelectList(subjectTypes, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,SubjectCode,SubjectName,ShortName,Description,CreditHours,SubjectTypeId,IsActive")] SubjectCatalog subjectCatalog)
    {
        if (ModelState.IsValid)
        {
            await _subjectCatalogService.CreateSubjectCatalogAsync(subjectCatalog);
            return RedirectToAction(nameof(Index));
        }
        var subjectTypes = await _subjectCatalogService.GetSelectListsAsync(subjectCatalog.SubjectTypeId);
        ViewData["SubjectTypeId"] = new SelectList(subjectTypes, "Id", "Name", subjectCatalog.SubjectTypeId);
        return View(subjectCatalog);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var subjectCatalog = await _subjectCatalogService.GetSubjectCatalogByIdAsync(id.Value);
        if (subjectCatalog == null) return NotFound();

        var subjectTypes = await _subjectCatalogService.GetSelectListsAsync(subjectCatalog.SubjectTypeId);
        ViewData["SubjectTypeId"] = new SelectList(subjectTypes, "Id", "Name", subjectCatalog.SubjectTypeId);
        return View(subjectCatalog);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,SubjectCode,SubjectName,ShortName,Description,CreditHours,SubjectTypeId,IsActive")] SubjectCatalog subjectCatalog)
    {
        if (id != subjectCatalog.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await _subjectCatalogService.UpdateSubjectCatalogAsync(subjectCatalog);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _subjectCatalogService.SubjectCatalogExistsAsync(subjectCatalog.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        var subjectTypes = await _subjectCatalogService.GetSelectListsAsync(subjectCatalog.SubjectTypeId);
        ViewData["SubjectTypeId"] = new SelectList(subjectTypes, "Id", "Name", subjectCatalog.SubjectTypeId);
        return View(subjectCatalog);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var subjectCatalog = await _subjectCatalogService.GetSubjectCatalogByIdAsync(id.Value);
        if (subjectCatalog == null) return NotFound();

        return View(subjectCatalog);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _subjectCatalogService.DeleteSubjectCatalogAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
