using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("examtypes.view")]
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

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string search = null, string sort = "Name", string sortDir = "asc")
    {
        var items = await examTypeService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("ExamTypes");

        var headers = new[] { "Code", "Name", "Remarks", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var e in items)
        {
            worksheet.Cell(row, 1).Value = e.Code.ToString();
            worksheet.Cell(row, 2).Value = e.Name ?? string.Empty;
            worksheet.Cell(row, 3).Value = e.Remarks ?? string.Empty;
            worksheet.Cell(row, 4).Value = e.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"ExamTypes_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var examType = await examTypeService.GetExamTypeByIdAsync(id.Value);
        if (examType == null) return NotFound();

        return View(examType);
    }

    [RequirePermission("examtypes.create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [RequirePermission("examtypes.create")]
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

    [RequirePermission("examtypes.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var examType = await examTypeService.GetExamTypeByIdAsync(id.Value);
        if (examType == null) return NotFound();

        return View(examType);
    }

    [HttpPost]
    [RequirePermission("examtypes.edit")]
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

    [RequirePermission("examtypes.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var examType = await examTypeService.GetExamTypeByIdAsync(id.Value);
        if (examType == null) return NotFound();

        return View(examType);
    }

    [HttpPost, ActionName("Delete")]
    [RequirePermission("examtypes.delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await examTypeService.DeleteExamTypeAsync(id);
        return RedirectToAction(nameof(Index));
    }
        [RequirePermission("PLACEHOLDER_PERMISSION")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await examTypeService.DeleteExamTypeAsync(id); return Json(new { success = true, message = "Exam type deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
