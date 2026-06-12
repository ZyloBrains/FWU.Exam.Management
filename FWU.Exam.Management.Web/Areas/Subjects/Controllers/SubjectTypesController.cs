using System.Text;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Subjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Subjects.Controllers;

[Area("Subjects")]
[RequirePermission("subjecttypes.view")]
public class SubjectTypesController : Controller
{
    private readonly ISubjectTypeService _subjectTypeService;

    public SubjectTypesController(ISubjectTypeService subjectTypeService)
    {
        _subjectTypeService = subjectTypeService;
    }

    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Name", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await _subjectTypeService.GetSubjectTypesAsync(page, pageSize, search, sort, sortDir);

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
        var items = await _subjectTypeService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Code,Name,Max Allowed Subjects,Is Default,Status");

        foreach (var s in items)
        {
            sb.AppendLine($"{EscapeCsv(s.Code)}," +
                           $"{EscapeCsv(s.Name)}," +
                           $"{s.MaxAllowedSubjects}," +
                           $"{(s.IsDefault ? "Yes" : "No")}," +
                           $"{(s.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"SubjectTypes_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "Name", string sortDir = "asc")
    {
        var (items, totalCount) = await _subjectTypeService.GetSubjectTypesAsync(page, pageSize, search, sort, sortDir);

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

        var subjectType = await _subjectTypeService.GetSubjectTypeByIdAsync(id.Value);
        if (subjectType == null) return NotFound();

        return View(subjectType);
    }

    [RequirePermission("subjecttypes.create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("subjecttypes.create")]
    public async Task<IActionResult> Create([Bind("Id,Code,Name,IsActive,IsDefault,MaxAllowedSubjects")] SubjectType subjectType)
    {
        if (ModelState.IsValid)
        {
            await _subjectTypeService.CreateSubjectTypeAsync(subjectType);
            return RedirectToAction(nameof(Index));
        }
        return View(subjectType);
    }

    [RequirePermission("subjecttypes.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var subjectType = await _subjectTypeService.GetSubjectTypeByIdAsync(id.Value);
        if (subjectType == null) return NotFound();

        return View(subjectType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("subjecttypes.edit")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Name,IsActive,IsDefault,MaxAllowedSubjects")] SubjectType subjectType)
    {
        if (id != subjectType.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await _subjectTypeService.UpdateSubjectTypeAsync(subjectType);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _subjectTypeService.SubjectTypeExistsAsync(subjectType.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(subjectType);
    }

    [RequirePermission("subjecttypes.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var subjectType = await _subjectTypeService.GetSubjectTypeByIdAsync(id.Value);
        if (subjectType == null) return NotFound();

        return View(subjectType);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [RequirePermission("subjecttypes.delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _subjectTypeService.DeleteSubjectTypeAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
