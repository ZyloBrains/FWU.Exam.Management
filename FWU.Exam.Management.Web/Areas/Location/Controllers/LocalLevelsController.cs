using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Text;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Location.Controllers;

[Area("Location")]
[RequirePermission("locallevels.view")]
public class LocalLevelsController(ILocalLevelService localLevelService) : Controller
{

    // GET: LocalLevels with pagination, search, and sorting
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "LocalLevelName", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await localLevelService.GetLocalLevelsAsync(page, pageSize, search, sort, sortDir);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }

    // Helper method to escape CSV fields

    // Export to CSV (Current Page with pagination)
    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string? search = null, string sort = "LocalLevelName", string sortDir = "asc")
    {
        var items = await localLevelService.GetFilteredLocalLevelsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();

        // CSV header
        sb.AppendLine("Local Level Name,Local Level Type,District,Status");

        foreach (var ll in items)
        {
            sb.AppendLine($"{ll.LocalLevelName.EscapeCsv()}," +
                           $"{ll.LocalLevelType.ToString().EscapeCsv()}," +
                           $"{(ll.District?.DistrictName ?? "").EscapeCsv()}," +
                           $"{(ll.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"LocalLevels_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    // Export to PDF (Current Page with pagination)
    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string? search = null, string sort = "LocalLevelName", string sortDir = "asc")
    {
        var (items, totalCount) = await localLevelService.GetLocalLevelsAsync(page, pageSize, search, sort, sortDir);

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View("PrintPdf", items);
    }

    // Export to Excel (Current Page with pagination)
    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string? search = null, string sort = "LocalLevelName", string sortDir = "asc")
    {
        var items = await localLevelService.GetFilteredLocalLevelsAsync(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("LocalLevels");

        var headers = new[] { "Local Level Name", "Local Level Type", "District", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var ll in items)
        {
            worksheet.Cell(row, 1).Value = ll.LocalLevelName ?? string.Empty;
            worksheet.Cell(row, 2).Value = ll.LocalLevelType.ToString();
            worksheet.Cell(row, 3).Value = ll.District?.DistrictName ?? string.Empty;
            worksheet.Cell(row, 4).Value = ll.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"LocalLevels_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // GET: LocalLevels/Create
    [RequirePermission("locallevels.create")]
    public async Task<IActionResult> Create()
    {
        var districts = await localLevelService.GetActiveDistrictsAsync();
        ViewData["DistrictId"] = new SelectList(districts, "Id", "DistrictName");
        return View();
    }

    // POST: LocalLevels/Create
    [RequirePermission("locallevels.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,DistrictId,LocalLevelName,LocalLevelType,IsActive")] LocalLevel localLevel)
    {
        if (ModelState.IsValid)
        {
            await localLevelService.CreateLocalLevelAsync(localLevel);
            TempData["SuccessMessage"] = "Local level created successfully!";
            return RedirectToAction(nameof(Index));
        }

        var districts = await localLevelService.GetActiveDistrictsAsync();
        ViewData["DistrictId"] = new SelectList(districts, "Id", "DistrictName", localLevel.DistrictId);
        return View(localLevel);
    }

    // GET: LocalLevels/Edit/5
    [RequirePermission("locallevels.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var localLevel = await localLevelService.GetLocalLevelByIdAsync(id.Value);
        if (localLevel == null)
        {
            return NotFound();
        }

        var districts = await localLevelService.GetActiveDistrictsAsync();
        ViewData["DistrictId"] = new SelectList(districts, "Id", "DistrictName", localLevel.DistrictId);
        return View(localLevel);
    }

    // POST: LocalLevels/Edit/5
    [RequirePermission("locallevels.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,DistrictId,LocalLevelName,LocalLevelType,IsActive")] LocalLevel localLevel)
    {
        if (id != localLevel.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await localLevelService.UpdateLocalLevelAsync(localLevel);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await localLevelService.LocalLevelExistsAsync(localLevel.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            TempData["SuccessMessage"] = "Local level updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        var districts = await localLevelService.GetActiveDistrictsAsync();
        ViewData["DistrictId"] = new SelectList(districts, "Id", "DistrictName", localLevel.DistrictId);
        return View(localLevel);
    }

    // GET: LocalLevels/Delete/5
    [RequirePermission("locallevels.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var localLevel = await localLevelService.GetLocalLevelByIdAsync(id.Value);
        if (localLevel == null)
        {
            return NotFound();
        }

        return View(localLevel);
    }

    // POST: LocalLevels/Delete/5
    [RequirePermission("locallevels.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await localLevelService.DeleteLocalLevelAsync(id);
            TempData["SuccessMessage"] = "Local level deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
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
        [RequirePermission("locallevels.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await localLevelService.DeleteLocalLevelAsync(id); return Json(new { success = true, message = "Local level deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
