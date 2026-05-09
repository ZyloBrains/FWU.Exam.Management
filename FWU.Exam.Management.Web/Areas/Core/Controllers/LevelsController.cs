using System.Text;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
public class LevelsController : Controller
{
    private readonly ILevelService _levelService;

    public LevelsController(ILevelService levelService)
    {
        _levelService = levelService;
    }

    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "LevelDisplayOrder", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await _levelService.GetLevelsAsync(page, pageSize, search, sort, sortDir);

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

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "LevelDisplayOrder", string sortDir = "asc")
    {
        var items = await _levelService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Level Code,Level Name,Display Order,Remarks,Is Running,Status");

        foreach (var l in items)
        {
            sb.AppendLine($"{EscapeCsv(l.LevelCode)}," +
                           $"{EscapeCsv(l.LevelName)}," +
                           $"{l.LevelDisplayOrder}," +
                           $"{EscapeCsv(l.Remarks)}," +
                           $"{(l.IsRunning == true ? "Yes" : "No")}," +
                           $"{(l.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"Levels_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "LevelDisplayOrder", string sortDir = "asc")
    {
        var (items, totalCount) = await _levelService.GetLevelsAsync(page, pageSize, search, sort, sortDir);

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

        var level = await _levelService.GetLevelByIdAsync(id.Value);
        if (level == null) return NotFound();

        return View(level);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,LevelCode,LevelName,LevelDisplayOrder,Remarks,IsRunning,IsActive")] Level level)
    {
        if (ModelState.IsValid)
        {
            await _levelService.CreateLevelAsync(level);
            return RedirectToAction(nameof(Index));
        }
        return View(level);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var level = await _levelService.GetLevelByIdAsync(id.Value);
        if (level == null) return NotFound();

        return View(level);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,LevelCode,LevelName,LevelDisplayOrder,Remarks,IsRunning,IsActive")] Level level)
    {
        if (id != level.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await _levelService.UpdateLevelAsync(level);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _levelService.LevelExistsAsync(level.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(level);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var level = await _levelService.GetLevelByIdAsync(id.Value);
        if (level == null) return NotFound();

        return View(level);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _levelService.DeleteLevelAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
