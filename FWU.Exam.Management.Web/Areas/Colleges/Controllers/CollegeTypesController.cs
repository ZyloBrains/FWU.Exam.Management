using System.Text;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Colleges;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Colleges.Controllers;

[Area("Colleges")]
public class CollegeTypesController(ICollegeTypeService collegeTypeService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Name", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await collegeTypeService.GetCollegeTypesAsync(page, pageSize, search, sort, sortDir);

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
        var items = await collegeTypeService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Code,Name,Remarks,Is Default,Status");

        foreach (var c in items)
        {
            sb.AppendLine($"{EscapeCsv(c.Code)}," +
                           $"{EscapeCsv(c.Name)}," +
                           $"{EscapeCsv(c.Remarks ?? "N/A")}," +
                           $"{(c.IsDefault == true ? "Yes" : "No")}," +
                           $"{(c.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"CollegeTypes_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "Name", string sortDir = "asc")
    {
        var (items, totalCount) = await collegeTypeService.GetCollegeTypesAsync(page, pageSize, search, sort, sortDir);

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

        var collegeType = await collegeTypeService.GetCollegeTypeByIdAsync(id.Value);
        if (collegeType == null) return NotFound();

        return View(collegeType);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Code,Name,Remarks,IsDefault,IsActive")] CollegeType collegeType)
    {
        if (ModelState.IsValid)
        {
            await collegeTypeService.CreateCollegeTypeAsync(collegeType);
            return RedirectToAction(nameof(Index));
        }
        return View(collegeType);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var collegeType = await collegeTypeService.GetCollegeTypeByIdAsync(id.Value);
        if (collegeType == null) return NotFound();

        return View(collegeType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Code,Name,Remarks,IsDefault,IsActive")] CollegeType collegeType)
    {
        if (id != collegeType.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await collegeTypeService.UpdateCollegeTypeAsync(collegeType);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await collegeTypeService.CollegeTypeExistsAsync(collegeType.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(collegeType);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var collegeType = await collegeTypeService.GetCollegeTypeByIdAsync(id.Value);
        if (collegeType == null) return NotFound();

        return View(collegeType);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await collegeTypeService.DeleteCollegeTypeAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
