using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Extensions;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("examcenters.view")]
public class ExamCentersController(
    IExamCenterService examCenterService,
    IUserContext userContext,
    AppDbContext context) : Controller
{
    private async Task<List<ExamSchedule>> GetFilteredExamSchedulesAsync()
    {
        return await context.ExamSchedules.AsNoTracking().ApplyScope(userContext).ToListAsync();
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await examCenterService.GetExamCentersAsync(page, pageSize, search, sort, sortDir);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var examCenter = await examCenterService.GetExamCenterByIdAsync(id.Value);
        if (examCenter == null) return NotFound();
        return View(examCenter);
    }

    [RequirePermission("examcenters.create")]
    public async Task<IActionResult> Create()
    {
        var examSchedules = await GetFilteredExamSchedulesAsync();
        ViewData["ExamScheduleId"] = new SelectList(examSchedules, "Id", "ExamScheduleName");
        ViewData["VenueCollegeList"] = await context.Colleges
            .AsNoTracking()
            .ApplyScope(userContext)
            .Where(c => c.IsActive && c.IsExamCenterOnly)
            .OrderBy(c => c.Name)
            .ToListAsync();
        ViewData["SourceCollegeList"] = await context.Colleges
            .AsNoTracking()
            .ApplyScope(userContext)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("examcenters.create")]
    public async Task<IActionResult> Create([Bind("Code,ExamScheduleId,Remark,IsActive")] ExamCenter examCenter,
        int[] venueColleges, int[] sourceColleges)
    {
        if (ModelState.IsValid)
        {
            await examCenterService.CreateExamCenterWithCollegesAsync(
                examCenter,
                [.. venueColleges],
                [.. sourceColleges]);
            TempData["SuccessMessage"] = "Exam center created successfully!";
            return RedirectToAction(nameof(Index));
        }

        var examSchedules = await GetFilteredExamSchedulesAsync();
        ViewData["ExamScheduleId"] = new SelectList(examSchedules, "Id", "ExamScheduleName", examCenter.ExamScheduleId);
        ViewData["VenueCollegeList"] = await context.Colleges
            .AsNoTracking()
            .ApplyScope(userContext)
            .Where(c => c.IsActive && c.IsExamCenterOnly)
            .OrderBy(c => c.Name)
            .ToListAsync();
        ViewData["SourceCollegeList"] = await context.Colleges
            .AsNoTracking()
            .ApplyScope(userContext)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(examCenter);
    }

    [RequirePermission("examcenters.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var examCenter = await examCenterService.GetExamCenterByIdAsync(id.Value);
        if (examCenter == null) return NotFound();

        var examSchedules = await GetFilteredExamSchedulesAsync();
        ViewData["ExamScheduleId"] = new SelectList(examSchedules, "Id", "ExamScheduleName", examCenter.ExamScheduleId);
        ViewData["VenueCollegeList"] = await context.Colleges
            .AsNoTracking()
            .ApplyScope(userContext)
            .Where(c => c.IsActive && c.IsExamCenterOnly)
            .OrderBy(c => c.Name)
            .ToListAsync();
        ViewData["SourceCollegeList"] = await context.Colleges
            .AsNoTracking()
            .ApplyScope(userContext)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        ViewData["SelectedVenueIds"] = examCenter.ExamCenterVenues?.Select(ecv => ecv.CollegeId).ToArray() ?? [];
        ViewData["SelectedSourceIds"] = examCenter.ExamCenterColleges?.Select(ecc => ecc.CollegeId).ToArray() ?? [];
        return View(examCenter);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("examcenters.edit")]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,Code,ExamScheduleId,Remark,IsActive")] ExamCenter examCenter,
        int[] venueColleges, int[] sourceColleges)
    {
        if (id != examCenter.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await examCenterService.UpdateExamCenterWithCollegesAsync(
                    examCenter,
                    [.. venueColleges],
                    [.. sourceColleges]);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await examCenterService.ExamCenterExistsAsync(examCenter.Id))
                    return NotFound();
                throw;
            }
            TempData["SuccessMessage"] = "Exam center updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        var examSchedules = await GetFilteredExamSchedulesAsync();
        ViewData["ExamScheduleId"] = new SelectList(examSchedules, "Id", "ExamScheduleName", examCenter.ExamScheduleId);
        ViewData["VenueCollegeList"] = await context.Colleges
            .AsNoTracking()
            .ApplyScope(userContext)
            .Where(c => c.IsActive && c.IsExamCenterOnly)
            .OrderBy(c => c.Name)
            .ToListAsync();
        ViewData["SourceCollegeList"] = await context.Colleges
            .AsNoTracking()
            .ApplyScope(userContext)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        ViewData["SelectedVenueIds"] = venueColleges;
        ViewData["SelectedSourceIds"] = sourceColleges;
        return View(examCenter);
    }

    [RequirePermission("examcenters.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var examCenter = await examCenterService.GetExamCenterByIdAsync(id.Value);
        if (examCenter == null) return NotFound();
        return View(examCenter);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [RequirePermission("examcenters.delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await examCenterService.DeleteExamCenterAsync(id);
            TempData["SuccessMessage"] = "Exam center deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "Cannot delete this record because it is referenced by other records. Please remove or reassign dependent records first.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while deleting: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [RequirePermission("examcenters.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try
        {
            await examCenterService.DeleteExamCenterAsync(id);
            return Json(new { success = true, message = "Exam center deleted successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> ExportToCsv(string? search = null)
    {
        var items = await examCenterService.GetFilteredItemsAsync(search, "Id", "asc");

        var sb = new StringBuilder();
        sb.AppendLine("ID,Code,Exam Schedule,College,Remark,Is Active");

        foreach (var item in items)
        {
            sb.AppendLine($"{item.Id},{(item.Code ?? "").EscapeCsv()},{(item.ExamSchedule?.ExamScheduleName ?? "").EscapeCsv()},{(item.College?.Name ?? "").EscapeCsv()},{(item.Remark ?? "").EscapeCsv()},{(item.IsActive ? "Yes" : "No")}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", "ExamCenters.csv");
    }

    public async Task<IActionResult> ExportToPdf(string? search = null)
    {
        var items = await examCenterService.GetFilteredItemsAsync(search, "Id", "asc");
        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(string? search = null)
    {
        var items = await examCenterService.GetFilteredItemsAsync(search, "Id", "asc");

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("ExamCenters");

        var headers = new[] { "ID", "Code", "Exam Schedule", "College", "Remark", "Is Active" };
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
            worksheet.Cell(row, 2).Value = item.Code ?? "";
            worksheet.Cell(row, 3).Value = item.ExamSchedule?.ExamScheduleName ?? "";
            worksheet.Cell(row, 4).Value = item.College?.Name ?? "";
            worksheet.Cell(row, 5).Value = item.Remark ?? "";
            worksheet.Cell(row, 6).Value = item.IsActive ? "Yes" : "No";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExamCenters.xlsx");
    }
}
