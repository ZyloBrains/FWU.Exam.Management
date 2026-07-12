using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using FWU.Exam.Management.Web.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("examschedules.view")]
public class ExamSchedulesController(
    IExamScheduleService examScheduleService,
    UserManager<AppUser> userManager,
    AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10)
    {
        await examScheduleService.DeactivateExpiredSchedulesAsync();

        var (items, totalCount) = await examScheduleService.GetExamSchedulesAsync(page, pageSize, search, sort, sortDir);

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

    public async Task<IActionResult> ExportToCsv(string search = null)
    {
        var items = await examScheduleService.GetFilteredItemsAsync(search);

        var sb = new StringBuilder();
        sb.AppendLine("ID,Exam Schedule Name,Code,Academic Year,Level,Exam Type,Start Date (BS),End Date (BS),Start Date (AD),End Date (AD),Published Date,Start Time,End Time,Is Active,Extended Date,Extended Date Charge,College Approval Date,Admission Card Release Date,Remarks");

        foreach (var item in items)
        {
            sb.AppendLine($"{EscapeCsv(item.Id.ToString())}," +
                          $"{EscapeCsv(item.ExamScheduleName ?? string.Empty)}," +
                          $"{EscapeCsv(item.ExamScheduleCode ?? string.Empty)}," +
                          $"{EscapeCsv(item.AcademicYear?.AcademicYearName ?? string.Empty)}," +
                          $"{EscapeCsv(item.ExamType?.Name ?? string.Empty)}," +
                          $"{EscapeCsv(item.StartDateBs ?? string.Empty)}," +
                          $"{EscapeCsv(item.EndDateBs ?? string.Empty)}," +
                          $"{EscapeCsv(item.StartDate?.ToString("yyyy-MM-dd") ?? string.Empty)}," +
                          $"{EscapeCsv(item.EndDate?.ToString("yyyy-MM-dd") ?? string.Empty)}," +
                          $"{EscapeCsv(item.PublishedDate?.ToString("yyyy-MM-dd") ?? string.Empty)}," +
                          $"{EscapeCsv(item.StartTime.ToString())}," +
                          $"{EscapeCsv(item.EndTime.ToString())}," +
                          $"{(item.IsActive ? "Yes" : "No")}," +
                          $"{EscapeCsv(item.ExtendedDate?.ToString("yyyy-MM-dd") ?? string.Empty)}," +
                          $"{item.ExtendedDateCharge}," +
                          $"{EscapeCsv(item.CollegeApprovalDate?.ToString("yyyy-MM-dd") ?? string.Empty)}," +
                          $"{EscapeCsv(item.AdmissionCardReleaseDate?.ToString("yyyy-MM-dd") ?? string.Empty)}," +
                          $"{EscapeCsv(item.Remarks ?? string.Empty)}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", "ExamSchedules.csv");
    }

    public async Task<IActionResult> ExportToPdf(string search = null)
    {
        var items = await examScheduleService.GetFilteredItemsAsync(search);
        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(string search = null)
    {
        var items = await examScheduleService.GetFilteredItemsAsync(search);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("ExamSchedules");

        var headers = new[] { "ID", "Exam Schedule Name", "Code", "Academic Year", "Exam Type", "Start Date (BS)", "End Date (BS)", "Start Date (AD)", "End Date (AD)", "Published Date", "Start Time", "End Time", "Is Active", "Extended Date", "Extended Date Charge", "College Approval Date", "Admission Card Release Date", "Remarks" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var item in items)
        {
            worksheet.Cell(row, 1).Value = item.Id;
            worksheet.Cell(row, 2).Value = item.ExamScheduleName ?? string.Empty;
            worksheet.Cell(row, 3).Value = item.ExamScheduleCode ?? string.Empty;
            worksheet.Cell(row, 4).Value = item.AcademicYear?.AcademicYearName ?? string.Empty;
            worksheet.Cell(row, 5).Value = item.ExamType?.Name ?? string.Empty;
            worksheet.Cell(row, 6).Value = item.StartDateBs ?? string.Empty;
            worksheet.Cell(row, 7).Value = item.EndDateBs ?? string.Empty;
            worksheet.Cell(row, 8).Value = item.StartDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            worksheet.Cell(row, 9).Value = item.EndDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            worksheet.Cell(row, 10).Value = item.PublishedDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            worksheet.Cell(row, 11).Value = item.StartTime.ToString();
            worksheet.Cell(row, 12).Value = item.EndTime.ToString();
            worksheet.Cell(row, 13).Value = item.IsActive ? "Yes" : "No";
            worksheet.Cell(row, 14).Value = item.ExtendedDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            worksheet.Cell(row, 15).Value = item.ExtendedDateCharge?.ToString() ?? string.Empty;
            worksheet.Cell(row, 16).Value = item.CollegeApprovalDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            worksheet.Cell(row, 17).Value = item.AdmissionCardReleaseDate?.ToString("yyyy-MM-dd") ?? string.Empty;
            worksheet.Cell(row, 18).Value = item.Remarks ?? string.Empty;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExamSchedules.xlsx");
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var examSchedule = await examScheduleService.GetExamScheduleByIdAsync(id.Value);
        if (examSchedule == null) return NotFound();

        return View(examSchedule);
    }

    [RequirePermission("examschedules.create")]
    public async Task<IActionResult> Create()
    {
        var selectLists = examScheduleService.GetSelectListData();
        PopulateDropdowns(selectLists);
        return View();
    }

    [HttpPost]
    [RequirePermission("examschedules.create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,AcademicYearId,ProgramId,SemesterId,ExamTypeId,ExamScheduleName,StartDateBs,EndDateBs,StartDate,EndDate,PublishedDate,StartTime,EndTime,Remarks,IsActive,ExtendedDate,ExtendedDateCharge,ExamFee,PracticalSubjectFee,CollegeApprovalDate,AdmissionCardReleaseDate,ExamScheduleCode")] ExamSchedule examSchedule)
    {
        if (ModelState.IsValid)
        {
            await examScheduleService.CreateExamScheduleAsync(examSchedule);
            return RedirectToAction(nameof(Index));
        }
        var selectLists = examScheduleService.GetSelectListData();
        PopulateDropdowns(selectLists, examSchedule);
        return View(examSchedule);
    }

    [RequirePermission("examschedules.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var examSchedule = await examScheduleService.GetExamScheduleByIdAsync(id.Value);
        if (examSchedule == null) return NotFound();

        var selectLists = examScheduleService.GetSelectListData();
        PopulateDropdowns(selectLists, examSchedule);
        return View(examSchedule);
    }

    [HttpPost]
    [RequirePermission("examschedules.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,AcademicYearId,ProgramId,SemesterId,ExamTypeId,ExamScheduleName,StartDateBs,EndDateBs,StartDate,EndDate,PublishedDate,StartTime,EndTime,Remarks,IsActive,ExtendedDate,ExtendedDateCharge,ExamFee,PracticalSubjectFee,CollegeApprovalDate,AdmissionCardReleaseDate,ExamScheduleCode")] ExamSchedule examSchedule)
    {
        if (id != examSchedule.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var existing = await examScheduleService.GetExamScheduleByIdAsync(id);
                if (existing is null) return NotFound();
                examSchedule.TenantId = existing.TenantId;
                await examScheduleService.UpdateExamScheduleAsync(examSchedule);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await examScheduleService.ExamScheduleExistsAsync(examSchedule.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        var selectLists = examScheduleService.GetSelectListData();
        PopulateDropdowns(selectLists, examSchedule);
        return View(examSchedule);
    }

    [RequirePermission("examschedules.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var examSchedule = await examScheduleService.GetExamScheduleByIdAsync(id.Value);
        if (examSchedule == null) return NotFound();

        return View(examSchedule);
    }

    [HttpPost, ActionName("Delete")]
    [RequirePermission("examschedules.delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await examScheduleService.DeleteExamScheduleAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private void PopulateDropdowns(ExamScheduleSelectListsDto selectLists, ExamSchedule? examSchedule = null)
    {
        ViewData["AcademicYearId"] = new SelectList(selectLists.AcademicYears, "Id", "Name", examSchedule?.AcademicYearId);
        ViewData["ExamTypeId"] = new SelectList(selectLists.ExamTypes, "Id", "Name", examSchedule?.ExamTypeId);
        ViewData["ProgramId"] = new SelectList(selectLists.Programs, "Id", "Name", examSchedule?.ProgramId);
        ViewData["SemesterId"] = new SelectList(selectLists.Semesters, "Id", "Name", examSchedule?.SemesterId);
    }

    [HttpGet]
    public JsonResult ConvertBsToAd(string bsDate)
    {
        var parts = bsDate.Split('-');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var y) || !int.TryParse(parts[1], out var m) || !int.TryParse(parts[2], out var d))
            return Json(new { adDate = (string?)null });
        var ad = NepaliCalendarHelper.BsToAd(y, m, d);
        return Json(new { adDate = ad?.ToString("yyyy-MM-dd") });
    }

    [HttpGet]
    public JsonResult ConvertAdToBs(string adDate)
    {
        if (!DateTime.TryParse(adDate, out var dt))
            return Json(new { bsDate = (string?)null });
        var bs = NepaliCalendarHelper.AdToBs(dt);
        return Json(new { bsDate = $"{bs.Year:D4}-{bs.Month:D2}-{bs.Day:D2}" });
    }

    [RequirePermission("examschedules.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await examScheduleService.DeleteExamScheduleAsync(id); return Json(new { success = true, message = "Exam schedule deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
