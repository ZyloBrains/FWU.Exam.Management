using System.Text;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[Authorize(Roles = "SuperAdmin,FacultyAdmin")]
public class ExamTypesController(IExamTypeService examTypeService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Name", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await examTypeService.GetExamTypesAsync(page, pageSize, search, sort, sortDir);

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
        var items = await examTypeService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Code,Name,Remarks,Status");

        foreach (var e in items)
        {
            sb.AppendLine($"{EscapeCsv(e.Code.ToString())}," +
                           $"{EscapeCsv(e.Name)}," +
                           $"{EscapeCsv(e.Remarks)}," +
                           $"{(e.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"ExamTypes_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "Name", string sortDir = "asc")
    {
        var (items, totalCount) = await examTypeService.GetExamTypesAsync(page, pageSize, search, sort, sortDir);

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

        var examType = await examTypeService.GetExamTypeByIdAsync(id.Value);
        if (examType == null) return NotFound();

        return View(examType);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Remarks,IsActive,Code")] ExamType examType)
    {
        if (ModelState.IsValid)
        {
            await examTypeService.CreateExamTypeAsync(examType);
            return RedirectToAction(nameof(Index));
        }
        return View(examType);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var examType = await examTypeService.GetExamTypeByIdAsync(id.Value);
        if (examType == null) return NotFound();

        return View(examType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Remarks,IsActive,Code")] ExamType examType)
    {
        if (id != examType.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await examTypeService.UpdateExamTypeAsync(examType);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await examTypeService.ExamTypeExistsAsync(examType.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(examType);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var examType = await examTypeService.GetExamTypeByIdAsync(id.Value);
        if (examType == null) return NotFound();

        return View(examType);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await examTypeService.DeleteExamTypeAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
