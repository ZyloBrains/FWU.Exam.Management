using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("departments.view")]
public class DepartmentsController(IDepartmentService departmentService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "DepartmentName", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await departmentService.GetDepartmentsAsync(page, pageSize, search, sort, sortDir);

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

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "DepartmentName", string sortDir = "asc")
    {
        var items = await departmentService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Department Code,Department Name,Short Name,Remarks,Status");

        foreach (var d in items)
        {
            sb.AppendLine($"{EscapeCsv(d.DepartmentCode)}," +
                           $"{EscapeCsv(d.DepartmentName)}," +
                           $"{EscapeCsv(d.ShortName)}," +
                           $"{EscapeCsv(d.Remarks)}," +
                           $"{(d.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"Departments_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "DepartmentName", string sortDir = "asc")
    {
        var (items, totalCount) = await departmentService.GetDepartmentsAsync(page, pageSize, search, sort, sortDir);

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string search = null, string sort = "DepartmentName", string sortDir = "asc")
    {
        var items = await departmentService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Departments");

        var headers = new[] { "Department Code", "Department Name", "Short Name", "Remarks", "Status" };
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
            worksheet.Cell(row, 1).Value = d.DepartmentCode;
            worksheet.Cell(row, 2).Value = d.DepartmentName;
            worksheet.Cell(row, 3).Value = d.ShortName;
            worksheet.Cell(row, 4).Value = d.Remarks;
            worksheet.Cell(row, 5).Value = d.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"Departments_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var department = await departmentService.GetDepartmentByIdAsync(id.Value);
        if (department == null) return NotFound();

        return View(department);
    }

    [RequirePermission("departments.create")]
    public IActionResult Create()
    {
        return View();
    }

    [RequirePermission("departments.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,DepartmentCode,DepartmentName,ShortName,Remarks,IsActive")] Department department)
    {
        if (ModelState.IsValid)
        {
            await departmentService.CreateDepartmentAsync(department);
            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }

    [RequirePermission("departments.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var department = await departmentService.GetDepartmentByIdAsync(id.Value);
        if (department == null) return NotFound();

        return View(department);
    }

    [RequirePermission("departments.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,DepartmentCode,DepartmentName,ShortName,Remarks,IsActive")] Department department)
    {
        if (id != department.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await departmentService.UpdateDepartmentAsync(department);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await departmentService.DepartmentExistsAsync(department.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(department);
    }

    [RequirePermission("departments.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var department = await departmentService.GetDepartmentByIdAsync(id.Value);
        if (department == null) return NotFound();

        return View(department);
    }

    [RequirePermission("departments.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await departmentService.DeleteDepartmentAsync(id);
        return RedirectToAction(nameof(Index));
    }
        [RequirePermission("PLACEHOLDER_PERMISSION")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await departmentService.DeleteDepartmentAsync(id); return Json(new { success = true, message = "Department deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
