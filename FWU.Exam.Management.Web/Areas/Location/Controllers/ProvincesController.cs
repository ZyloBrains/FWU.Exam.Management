using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Location;
using Microsoft.EntityFrameworkCore;
using System.Text;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Location.Controllers;

[Area("Location")]
[RequirePermission("provinces.view")]
public class ProvincesController(IProvinceService provinceService) : Controller
{

    // GET: Provinces with pagination, search, and sorting
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "ProvinceName", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await provinceService.GetProvincesAsync(page, pageSize, search, sort, sortDir);

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
    private string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    // Export to CSV (Current Page with pagination)
    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "ProvinceName", string sortDir = "asc")
    {
        var items = await provinceService.GetFilteredProvincesAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();

        // CSV header
        sb.AppendLine("Province Name,Status");

        foreach (var p in items)
        {
            sb.AppendLine($"{EscapeCsv(p.ProvinceName)}," +
                           $"{(p.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"Provinces_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    // Export to PDF (Current Page with pagination)
    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "ProvinceName", string sortDir = "asc")
    {
        var (items, totalCount) = await provinceService.GetProvincesAsync(page, pageSize, search, sort, sortDir);

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
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string search = null, string sort = "ProvinceName", string sortDir = "asc")
    {
        var items = await provinceService.GetFilteredProvincesAsync(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Provinces");

        var headers = new[] { "Province Name", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var p in items)
        {
            worksheet.Cell(row, 1).Value = p.ProvinceName ?? string.Empty;
            worksheet.Cell(row, 2).Value = p.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"Provinces_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // GET: Provinces/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var province = await provinceService.GetProvinceByIdAsync(id.Value);
        if (province == null)
        {
            return NotFound();
        }

        return View(province);
    }

    // GET: Provinces/Create
    [RequirePermission("provinces.create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Provinces/Create
    [RequirePermission("provinces.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ProvinceName,Remarks,IsActive")] Province province)
    {
        if (ModelState.IsValid)
        {
            await provinceService.CreateProvinceAsync(province);
            TempData["SuccessMessage"] = "Province created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(province);
    }

    // GET: Provinces/Edit/5
    [RequirePermission("provinces.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var province = await provinceService.GetProvinceByIdAsync(id.Value);
        if (province == null)
        {
            return NotFound();
        }
        return View(province);
    }

    // POST: Provinces/Edit/5
    [RequirePermission("provinces.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ProvinceName,Remarks,IsActive")] Province province)
    {
        if (id != province.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await provinceService.UpdateProvinceAsync(province);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await provinceService.ProvinceExistsAsync(province.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            TempData["SuccessMessage"] = "Province updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(province);
    }

    // GET: Provinces/Delete/5
    [RequirePermission("provinces.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var province = await provinceService.GetProvinceByIdAsync(id.Value);
        if (province == null)
        {
            return NotFound();
        }

        return View(province);
    }

    // POST: Provinces/Delete/5
    [RequirePermission("provinces.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await provinceService.DeleteProvinceAsync(id);
            TempData["SuccessMessage"] = "Province deleted successfully!";
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
        [RequirePermission("provinces.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await provinceService.DeleteProvinceAsync(id); return Json(new { success = true, message = "Province deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
