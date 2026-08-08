using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("levels.view")]
public class LevelsController(ILevelService levelService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "LevelDisplayOrder", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await levelService.GetLevelsAsync(page, pageSize, search, sort, sortDir);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }


    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string? search = null, string sort = "LevelDisplayOrder", string sortDir = "asc")
    {
        var items = await levelService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Level Code,Level Name,Display Order,Remarks,Is Running,Status");

        foreach (var l in items)
        {
            sb.AppendLine($"{l.LevelCode.EscapeCsv()}," +
                           $"{l.LevelName.EscapeCsv()}," +
                           $"{l.LevelDisplayOrder}," +
                           $"{l.Remarks.EscapeCsv()}," +
                           $"{(l.IsRunning == true ? "Yes" : "No")}," +
                           $"{(l.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"Levels_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string? search = null, string sort = "LevelDisplayOrder", string sortDir = "asc")
    {
        var (items, totalCount) = await levelService.GetLevelsAsync(page, pageSize, search, sort, sortDir);

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string? search = null, string sort = "LevelDisplayOrder", string sortDir = "asc")
    {
        var items = await levelService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Levels");

        var headers = new[] { "Level Code", "Level Name", "Display Order", "Remarks", "Is Running", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var l in items)
        {
            worksheet.Cell(row, 1).Value = l.LevelCode;
            worksheet.Cell(row, 2).Value = l.LevelName;
            worksheet.Cell(row, 3).Value = l.LevelDisplayOrder;
            worksheet.Cell(row, 4).Value = l.Remarks;
            worksheet.Cell(row, 5).Value = l.IsRunning == true ? "Yes" : "No";
            worksheet.Cell(row, 6).Value = l.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"Levels_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var level = await levelService.GetLevelByIdAsync(id.Value);
        if (level == null) return NotFound();

        return View(level);
    }

    [RequirePermission("levels.create")]
    public IActionResult Create()
    {
        return View();
    }

    [RequirePermission("levels.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,LevelCode,LevelName,LevelDisplayOrder,Remarks,IsRunning,IsActive")] Level level)
    {
        if (ModelState.IsValid)
        {
            await levelService.CreateLevelAsync(level);
            TempData["SuccessMessage"] = "Level created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(level);
    }

    [RequirePermission("levels.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var level = await levelService.GetLevelByIdAsync(id.Value);
        if (level == null) return NotFound();

        return View(level);
    }

    [RequirePermission("levels.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,LevelCode,LevelName,LevelDisplayOrder,Remarks,IsRunning,IsActive")] Level level)
    {
        if (id != level.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await levelService.UpdateLevelAsync(level);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await levelService.LevelExistsAsync(level.Id))
                    return NotFound();
                throw;
            }
            TempData["SuccessMessage"] = "Level updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(level);
    }

    [RequirePermission("levels.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var level = await levelService.GetLevelByIdAsync(id.Value);
        if (level == null) return NotFound();

        return View(level);
    }

    [RequirePermission("levels.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await levelService.DeleteLevelAsync(id);
            TempData["SuccessMessage"] = "Level deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            TempData["ErrorMessage"] = "Cannot delete this record because it is referenced by other records. Please remove or reassign dependent records first.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while deleting: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
        [RequirePermission("levels.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await levelService.DeleteLevelAsync(id); return Json(new { success = true, message = "Level deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
