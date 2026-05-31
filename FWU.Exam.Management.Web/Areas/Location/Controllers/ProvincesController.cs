using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Location;
using Microsoft.EntityFrameworkCore;
using System.Text;

using Microsoft.AspNetCore.Authorization;

namespace FWU.Exam.Management.Web.Areas.Location.Controllers;

[Area("Location")]
[Authorize(Roles = "SuperAdmin,FacultyAdmin,CollegeAdmin")]
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
    public IActionResult Create()
    {
        return View();
    }

    // POST: Provinces/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ProvinceName,Remarks,IsActive")] Province province)
    {
        if (ModelState.IsValid)
        {
            await provinceService.CreateProvinceAsync(province);
            return RedirectToAction(nameof(Index));
        }
        return View(province);
    }

    // GET: Provinces/Edit/5
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
            return RedirectToAction(nameof(Index));
        }
        return View(province);
    }

    // GET: Provinces/Delete/5
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
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await provinceService.DeleteProvinceAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
