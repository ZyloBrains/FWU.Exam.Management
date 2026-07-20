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
[RequirePermission("districts.view")]
public class DistrictsController(IDistrictService districtService) : Controller
{

    // GET: Districts with pagination, search, and sorting
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "DistrictName", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await districtService.GetDistrictsAsync(page, pageSize, search, sort, sortDir);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var district = await districtService.GetDistrictByIdAsync(id.Value);
        if (district == null) return NotFound();

        return View(district);
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
    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "DistrictName", string sortDir = "asc")
    {
        var items = await districtService.GetFilteredDistrictsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();

        // CSV header
        sb.AppendLine("District Name,Province,Status");

        foreach (var d in items)
        {
            sb.AppendLine($"{EscapeCsv(d.DistrictName)}," +
                           $"{EscapeCsv(d.Province?.ProvinceName ?? "")}," +
                           $"{(d.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"Districts_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    // Export to PDF (Current Page with pagination)
    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "DistrictName", string sortDir = "asc")
    {
        var (items, totalCount) = await districtService.GetDistrictsAsync(page, pageSize, search, sort, sortDir);

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
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string search = null, string sort = "DistrictName", string sortDir = "asc")
    {
        var items = await districtService.GetFilteredDistrictsAsync(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Districts");

        var headers = new[] { "District Name", "Province", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var d in items)
        {
            worksheet.Cell(row, 1).Value = d.DistrictName ?? string.Empty;
            worksheet.Cell(row, 2).Value = d.Province?.ProvinceName ?? string.Empty;
            worksheet.Cell(row, 3).Value = d.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"Districts_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    // GET: Districts/Create
    [RequirePermission("districts.create")]
    public async Task<IActionResult> Create()
    {
        var provinces = await districtService.GetActiveProvincesAsync();
        ViewData["ProvinceId"] = new SelectList(provinces, "Id", "ProvinceName");
        return View();
    }

    // POST: Districts/Create
    [RequirePermission("districts.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ProvinceId,DistrictCode,DistrictName,Remarks,IsActive")] District district)
    {
        if (ModelState.IsValid)
        {
            await districtService.CreateDistrictAsync(district);
            TempData["SuccessMessage"] = "District created successfully!";
            return RedirectToAction(nameof(Index));
        }

        var provinces = await districtService.GetActiveProvincesAsync();
        ViewData["ProvinceId"] = new SelectList(provinces, "Id", "ProvinceName", district.ProvinceId);
        return View(district);
    }

    // GET: Districts/Edit/5
    [RequirePermission("districts.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var district = await districtService.GetDistrictByIdAsync(id.Value);
        if (district == null)
        {
            return NotFound();
        }

        var provinces = await districtService.GetActiveProvincesAsync();
        ViewData["ProvinceId"] = new SelectList(provinces, "Id", "ProvinceName", district.ProvinceId);
        return View(district);
    }

    // POST: Districts/Edit/5
    [RequirePermission("districts.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ProvinceId,DistrictCode,DistrictName,Remarks,IsActive")] District district)
    {
        if (id != district.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await districtService.UpdateDistrictAsync(district);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await districtService.DistrictExistsAsync(district.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            TempData["SuccessMessage"] = "District updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        var provinces = await districtService.GetActiveProvincesAsync();
        ViewData["ProvinceId"] = new SelectList(provinces, "Id", "ProvinceName", district.ProvinceId);
        return View(district);
    }

    // GET: Districts/Delete/5
    [RequirePermission("districts.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var district = await districtService.GetDistrictByIdAsync(id.Value);
        if (district == null)
        {
            return NotFound();
        }

        return View(district);
    }

    // POST: Districts/Delete/5
    [RequirePermission("districts.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await districtService.DeleteDistrictAsync(id);
            TempData["SuccessMessage"] = "District deleted successfully!";
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
        [RequirePermission("districts.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await districtService.DeleteDistrictAsync(id); return Json(new { success = true, message = "District deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
