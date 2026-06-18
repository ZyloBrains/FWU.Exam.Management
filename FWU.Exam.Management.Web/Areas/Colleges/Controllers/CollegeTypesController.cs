using ClosedXML.Excel;
using System.Text;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Colleges;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Colleges.Controllers;

[Area("Colleges")]
[RequirePermission("collegetypes.view")]
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

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string search = null, string sort = "Name", string sortDir = "asc")
    {
        var items = await collegeTypeService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("College Types");

        var headers = new[] { "Code", "Name", "Remarks", "Is Default", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var c in items)
        {
            worksheet.Cell(row, 1).Value = c.Code ?? "";
            worksheet.Cell(row, 2).Value = c.Name ?? "";
            worksheet.Cell(row, 3).Value = c.Remarks ?? "N/A";
            worksheet.Cell(row, 4).Value = c.IsDefault == true ? "Yes" : "No";
            worksheet.Cell(row, 5).Value = c.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CollegeTypes_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var collegeType = await collegeTypeService.GetCollegeTypeByIdAsync(id.Value);
        if (collegeType == null) return NotFound();

        return View(collegeType);
    }

    [RequirePermission("collegetypes.create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("collegetypes.create")]
    public async Task<IActionResult> Create([Bind("Id,Code,Name,Remarks,IsDefault,IsActive")] CollegeType collegeType)
    {
        if (ModelState.IsValid)
        {
            await collegeTypeService.CreateCollegeTypeAsync(collegeType);
            return RedirectToAction(nameof(Index));
        }
        return View(collegeType);
    }

    [RequirePermission("collegetypes.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var collegeType = await collegeTypeService.GetCollegeTypeByIdAsync(id.Value);
        if (collegeType == null) return NotFound();

        return View(collegeType);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("collegetypes.edit")]
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

    [RequirePermission("collegetypes.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var collegeType = await collegeTypeService.GetCollegeTypeByIdAsync(id.Value);
        if (collegeType == null) return NotFound();

        return View(collegeType);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [RequirePermission("collegetypes.delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await collegeTypeService.DeleteCollegeTypeAsync(id);
        return RedirectToAction(nameof(Index));
    }
        [RequirePermission("collegetypes.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await collegeTypeService.DeleteCollegeTypeAsync(id); return Json(new { success = true, message = "College type deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
