using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("academicyears.view")]
public class AcademicYearsController(IAcademicYearService academicYearService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, int pageSize = 10)
    {
        // The service currently returns a List<AcademicYear>. Do not attempt to deconstruct it.
        var (Items, TotalCount) = await academicYearService.GetAllAcademicYearsAsync(page, pageSize,search);

        // If you need the total count across all pages, update the service to return it.
        //var totalCount = items?.Count ?? 0;

        ViewBag.TotalCount = TotalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)TotalCount / pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        //ViewBag.Sort = sort;
        //ViewBag.SortDir = sortDir;

        return View(Items);
    }


    // Helper to escape CSV fields
    private string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    // Export to PDF (browser print)
    // Export to CSV - only the current page
    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null)
    {
        var (Items, TotalCount) = await academicYearService.GetAllAcademicYearsAsync(page, pageSize,search);

    
        var sb = new StringBuilder();
        sb.AppendLine("Code,Code (Nepali),Name,Name (Nepali),Remark,Running,Active");
        foreach (var item in Items)
        {
            sb.AppendLine($"{item.AcademicYearCode}," +
                          $"{EscapeCsv(item.AcademicYearCodeNepali)}," +
                          $"{EscapeCsv(item.AcademicYearName)}," +
                          $"{EscapeCsv(item.AcademicYearNameNepali)}," +
                          $"{EscapeCsv(item.Remark)}," +
                          $"{(item.IsRunning ? "Yes" : "No")}," +
                          $"{(item.IsActive ? "Active" : "Inactive")}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", "AcademicYears.csv");
    }

    // Export to PDF - only the current page (using browser print)
    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null)
    {
        var (Items, TotalCount) = await academicYearService.GetAllAcademicYearsAsync(page,pageSize,search);


        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = TotalCount;
        ViewBag.Search = search;
        //ViewBag.Sort = sort;
        //ViewBag.SortDir = sortDir;
        return View("PrintPdf", Items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string search = null)
    {
        var (Items, TotalCount) = await academicYearService.GetAllAcademicYearsAsync(page, pageSize, search);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Academic Years");

        var headers = new[] { "Code", "Code (Nepali)", "Name", "Name (Nepali)", "Remark", "Running", "Active" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var item in Items)
        {
            worksheet.Cell(row, 1).Value = item.AcademicYearCode;
            worksheet.Cell(row, 2).Value = item.AcademicYearCodeNepali;
            worksheet.Cell(row, 3).Value = item.AcademicYearName;
            worksheet.Cell(row, 4).Value = item.AcademicYearNameNepali;
            worksheet.Cell(row, 5).Value = item.Remark;
            worksheet.Cell(row, 6).Value = item.IsRunning ? "Yes" : "No";
            worksheet.Cell(row, 7).Value = item.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AcademicYears.xlsx");
    }

    // GET: AcademicYears/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var academicYear = await academicYearService.GetAcademicYearByIdAsync(id.Value);
        if (academicYear == null)
        {
            return NotFound();
        }

        return View(academicYear);
    }

    [RequirePermission("academicyears.create")]
    public IActionResult Create()
    {
        return View();
    }

    [RequirePermission("academicyears.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,AcademicYearCode,AcademicYearCodeNepali,AcademicYearName,AcademicYearNameNepali,Remark,IsRunning,IsActive")] AcademicYear academicYear)
    {
        if (ModelState.IsValid)
        {
            await academicYearService.CreateAcademicYearAsync(academicYear);
            return RedirectToAction(nameof(Index));
        }
        return View(academicYear);
    }

    [RequirePermission("academicyears.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var academicYear = await academicYearService.GetAcademicYearByIdAsync(id.Value);
        if (academicYear == null)
        {
            return NotFound();
        }
        return View(academicYear);
    }

    [RequirePermission("academicyears.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,AcademicYearCode,AcademicYearCodeNepali,AcademicYearName,AcademicYearNameNepali,Remark,IsRunning,IsActive")] AcademicYear academicYear)
    {
        if (id != academicYear.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await academicYearService.UpdateAcademicYearAsync(academicYear);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await academicYearService.AcademicYearExistsAsync(academicYear.Id))
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
        return View(academicYear);
    }

    [RequirePermission("academicyears.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var academicYear = await academicYearService.GetAcademicYearByIdAsync(id.Value);
        if (academicYear == null)
        {
            return NotFound();
        }

        return View(academicYear);
    }

    [RequirePermission("academicyears.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await academicYearService.DeleteAcademicYearAsync(id);
        return RedirectToAction(nameof(Index));
    }
        [RequirePermission("academicyears.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await academicYearService.DeleteAcademicYearAsync(id); return Json(new { success = true, message = "Academic year deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
