using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
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
    UserManager<AppUser> userManager,
    AppDbContext context) : Controller
{
    private async Task<(int? collegeId, int? facultyId)> ResolveScopeAsync(int? collegeId, int? facultyId)
    {
        if (collegeId.HasValue || facultyId.HasValue) return (collegeId, facultyId);
        var user = await userManager.GetUserAsync(User);
        if (user == null) return (null, null);
        if (User.IsInRole(Role.CollegeAdmin)) return (user.CollegeId, null);
        if (User.IsInRole(Role.FacultyAdmin)) return (null, user.FacultyId);
        return (null, null);
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10, int? collegeId = null, int? facultyId = null)
    {
        (collegeId, facultyId) = await ResolveScopeAsync(collegeId, facultyId);
        var (items, totalCount) = await resultRecordService.GetResultRecordsAsync(page, pageSize, search, sort, sortDir, collegeId, facultyId);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;
        ViewBag.CollegeId = collegeId;
        ViewBag.FacultyId = facultyId;

        ViewData["CollegeId"] = new SelectList(context.Colleges.AsNoTracking().Select(c => new { c.Id, c.Name }), "Id", "Name", collegeId);
        ViewData["FacultyId"] = new SelectList(context.Faculties.AsNoTracking().Select(f => new { f.Id, f.Name }), "Id", "Name", facultyId);

        return View(items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var resultRecord = await resultRecordService.GetResultRecordByIdAsync(id.Value);
        if (resultRecord == null) return NotFound();

        return View(resultRecord);
    }

    public async Task<IActionResult> ExportToCsv(string? search = null, int? collegeId = null, int? facultyId = null)
    {
        (collegeId, facultyId) = await ResolveScopeAsync(collegeId, facultyId);
        var items = await resultRecordService.GetFilteredItemsAsync(search, collegeId, facultyId);

        var sb = new StringBuilder();
        sb.AppendLine("ID,StudentName,SymbolNumber,RegistrationNumber,Year,Part,GPA,Result");

        foreach (var item in items)
        {
            sb.AppendLine($"{item.Id},{EscapeCsv(item.StudentName ?? "")},{EscapeCsv(item.SymbolNumber ?? "")},{EscapeCsv(item.RegistrationNumber ?? "")},{EscapeCsv(item.Year ?? "")},{EscapeCsv(item.Part ?? "")},{EscapeCsv(item.Gpa ?? "")},{EscapeCsv(item.Result ?? "")}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", "ResultRecords.csv");
    }

    public async Task<IActionResult> ExportToPdf(string? search = null, int? collegeId = null, int? facultyId = null)
    {
        (collegeId, facultyId) = await ResolveScopeAsync(collegeId, facultyId);
        var items = await resultRecordService.GetFilteredItemsAsync(search, collegeId, facultyId);
        return View("PrintPdf", items);
    }

    private static string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}
