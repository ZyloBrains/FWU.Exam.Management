using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Location;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FWU.Exam.Management.Web.Areas.Location.Controllers;

[Area("Location")]
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

    // GET: Districts/Create
    public async Task<IActionResult> Create()
    {
        var provinces = await districtService.GetActiveProvincesAsync();
        ViewData["ProvinceId"] = new SelectList(provinces, "Id", "ProvinceName");
        return View();
    }

    // POST: Districts/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ProvinceId,DistrictName,Remarks,IsActive")] District district)
    {
        if (ModelState.IsValid)
        {
            await districtService.CreateDistrictAsync(district);
            return RedirectToAction(nameof(Index));
        }

        var provinces = await districtService.GetActiveProvincesAsync();
        ViewData["ProvinceId"] = new SelectList(provinces, "Id", "ProvinceName", district.ProvinceId);
        return View(district);
    }

    // GET: Districts/Edit/5
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
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ProvinceId,DistrictName,Remarks,IsActive")] District district)
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
            return RedirectToAction(nameof(Index));
        }

        var provinces = await districtService.GetActiveProvincesAsync();
        ViewData["ProvinceId"] = new SelectList(provinces, "Id", "ProvinceName", district.ProvinceId);
        return View(district);
    }

    // GET: Districts/Delete/5
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
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await districtService.DeleteDistrictAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
