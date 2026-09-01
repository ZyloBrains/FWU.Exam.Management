using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Domain.Extensions;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("resultrecords.view")]
public class ResultRecordsController(
    IResultRecordService resultRecordService,
    IUserContext userContext,
    AppDbContext context,
    IAuditLogWriter auditLogWriter) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10, int? collegeId = null, int? facultyId = null)
    {
        var (items, totalCount) = await resultRecordService.GetResultRecordsAsync(page, pageSize, search, sort, sortDir);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;
        ViewBag.CollegeId = collegeId;
        ViewBag.FacultyId = facultyId;

        ViewData["CollegeId"] = new SelectList(context.Colleges.AsNoTracking().ApplyScope(userContext).OrderBy(c => c.Name).Select(c => new { c.Id, c.Name }), "Id", "Name", collegeId);
        ViewData["FacultyId"] = new SelectList(await context.GetScopedFacultiesAsync(userContext), "Id", "Name", facultyId);
        ViewData["ShowCollegeFilter"] = userContext.IsSuperAdmin || userContext.IsFacultyAdmin;

        return View(items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var resultRecord = await resultRecordService.GetResultRecordByIdAsync(id.Value);
        if (resultRecord == null) return NotFound();

        return View(resultRecord);
    }

    public async Task<IActionResult> ExportToCsv(string? search = null)
    {
        var items = await resultRecordService.GetFilteredItemsAsync(search);

        var sb = new StringBuilder();
        sb.AppendLine("ID,StudentName,SymbolNumber,RegistrationNumber,Year,Part,GPA,Result");

        foreach (var item in items)
        {
            sb.AppendLine($"{item.Id},{(item.StudentName ?? "").EscapeCsv()},{(item.SymbolNumber ?? "").EscapeCsv()},{(item.RegistrationNumber ?? "").EscapeCsv()},{(item.Year ?? "").EscapeCsv()},{(item.Part ?? "").EscapeCsv()},{(item.Gpa ?? "").EscapeCsv()},{(item.Result ?? "").EscapeCsv()}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        await auditLogWriter.LogAsync(ActivityTypes.ResultExported, "Result records exported to CSV", new { format = "csv", count = items.Count }, entityName: "ResultRecord");
        return File(csvBytes, "text/csv", "ResultRecords.csv");
    }

    public async Task<IActionResult> ExportToPdf(string? search = null)
    {
        var items = await resultRecordService.GetFilteredItemsAsync(search);
        await auditLogWriter.LogAsync(ActivityTypes.ResultExported, "Result records exported to PDF", new { format = "pdf", count = items.Count }, entityName: "ResultRecord");
        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(string? search = null)
    {
        var items = await resultRecordService.GetFilteredItemsAsync(search);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("ResultRecords");

        var headers = new[] { "ID", "StudentName", "SymbolNumber", "RegistrationNumber", "Year", "Part", "GPA", "Result" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.Gray;
        }

        int row = 2;
        foreach (var item in items)
        {
            worksheet.Cell(row, 1).Value = item.Id;
            worksheet.Cell(row, 2).Value = item.StudentName ?? "";
            worksheet.Cell(row, 3).Value = item.SymbolNumber ?? "";
            worksheet.Cell(row, 4).Value = item.RegistrationNumber ?? "";
            worksheet.Cell(row, 5).Value = item.Year ?? "";
            worksheet.Cell(row, 6).Value = item.Part ?? "";
            worksheet.Cell(row, 7).Value = item.Gpa ?? "";
            worksheet.Cell(row, 8).Value = item.Result ?? "";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        await auditLogWriter.LogAsync(ActivityTypes.ResultExported, "Result records exported to Excel", new { format = "excel", count = items.Count }, entityName: "ResultRecord");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ResultRecords.xlsx");
    }

}
