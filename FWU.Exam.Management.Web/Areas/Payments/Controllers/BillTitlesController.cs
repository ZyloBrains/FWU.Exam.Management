using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Domain.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Payments.Controllers;

[Area("Payments")]
[RequirePermission("billtitles.view")]
public class BillTitlesController(
    IBillTitleService billTitleService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "BillTitleName", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await billTitleService.GetBillTitlesAsync(page, pageSize, search, sort, sortDir);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }


    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string? search = null, string sort = "BillTitleName", string sortDir = "asc")
    {
        var items = await billTitleService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Bill Title Name,Category,Amount,Exam Schedule,Applicable Date,Through Date,Status");

        foreach (var bt in items)
        {
            sb.AppendLine($"{bt.BillTitleName.EscapeCsv()}," +
                           $"{(bt.Category ?? "-").EscapeCsv()}," +
                           $"{bt.Amount?.ToString("F2") ?? "-"}," +
                           $"{(bt.ExamSchedule?.ExamScheduleName ?? "-").EscapeCsv()}," +
                           $"{bt.ApplicableDate?.ToString("yyyy-MM-dd") ?? "-"}," +
                           $"{bt.ThroughDate?.ToString("yyyy-MM-dd") ?? "-"}," +
                           $"{(bt.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"BillTitles_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string? search = null, string sort = "BillTitleName", string sortDir = "asc")
    {
        var (items, totalCount) = await billTitleService.GetBillTitlesAsync(page, pageSize, search, sort, sortDir);

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string? search = null, string sort = "BillTitleName", string sortDir = "asc")
    {
        var items = await billTitleService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("BillTitles");

        var headers = new[] { "Bill Title Name", "Category", "Amount", "Exam Schedule", "Applicable Date", "Through Date", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.Gray;
        }

        int row = 2;
        foreach (var bt in items)
        {
            worksheet.Cell(row, 1).Value = bt.BillTitleName ?? "";
            worksheet.Cell(row, 2).Value = bt.Category ?? "-";
            worksheet.Cell(row, 3).Value = bt.Amount?.ToString("F2") ?? "-";
            worksheet.Cell(row, 4).Value = bt.ExamSchedule?.ExamScheduleName ?? "-";
            worksheet.Cell(row, 5).Value = bt.ApplicableDate?.ToString("yyyy-MM-dd") ?? "-";
            worksheet.Cell(row, 6).Value = bt.ThroughDate?.ToString("yyyy-MM-dd") ?? "-";
            worksheet.Cell(row, 7).Value = bt.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        var fileName = $"BillTitles_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [RequirePermission("billtitles.create")]
    public async Task<IActionResult> Create()
    {
        var examSchedules = await billTitleService.GetExamSchedulesAsync();
        ViewData["ExamScheduleId"] = new SelectList(examSchedules, "Id", "ExamScheduleName");
        var programs = await billTitleService.GetProgramsAsync();
        ViewData["ProgramsId"] = new SelectList(programs, "Id", "ProgramName");
        return View();
    }

    [RequirePermission("billtitles.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,BillTitleName,Category,IsActive,Amount,PracticalFee,ThroughDate,ApplicableDate,ExamScheduleId,ProgramsId")] BillTitle billTitle)
    {
        if (ModelState.IsValid)
        {
            await billTitleService.CreateBillTitleAsync(billTitle);
            TempData["SuccessMessage"] = "Bill title created successfully!";
            return RedirectToAction(nameof(Index));
        }
        var examSchedules = await billTitleService.GetExamSchedulesAsync();
        ViewData["ExamScheduleId"] = new SelectList(examSchedules, "Id", "ExamScheduleName", billTitle.ExamScheduleId);
        var programs = await billTitleService.GetProgramsAsync();
        ViewData["ProgramsId"] = new SelectList(programs, "Id", "ProgramName", billTitle.ProgramsId);
        return View(billTitle);
    }

    [RequirePermission("billtitles.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var billTitle = await billTitleService.GetBillTitleByIdAsync(id.Value);
        if (billTitle == null) return NotFound();

        var examSchedules = await billTitleService.GetExamSchedulesAsync();
        ViewData["ExamScheduleId"] = new SelectList(examSchedules, "Id", "ExamScheduleName", billTitle.ExamScheduleId);
        var programList = await billTitleService.GetProgramsAsync();
        ViewData["ProgramsId"] = new SelectList(programList, "Id", "ProgramName", billTitle.ProgramsId);
        return View(billTitle);
    }

    [RequirePermission("billtitles.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,BillTitleName,Category,IsActive,Amount,PracticalFee,ThroughDate,ApplicableDate,ExamScheduleId,ProgramsId")] BillTitle billTitle)
    {
        if (id != billTitle.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await billTitleService.UpdateBillTitleAsync(billTitle);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await billTitleService.BillTitleExistsAsync(billTitle.Id))
                    return NotFound();
                throw;
            }
            TempData["SuccessMessage"] = "Bill title updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        var examSchedules = await billTitleService.GetExamSchedulesAsync();
        ViewData["ExamScheduleId"] = new SelectList(examSchedules, "Id", "ExamScheduleName", billTitle.ExamScheduleId);
        var programs = await billTitleService.GetProgramsAsync();
        ViewData["ProgramsId"] = new SelectList(programs, "Id", "ProgramName", billTitle.ProgramsId);
        return View(billTitle);
    }

    [RequirePermission("billtitles.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var billTitle = await billTitleService.GetBillTitleByIdAsync(id.Value);
        if (billTitle == null) return NotFound();

        return View(billTitle);
    }

    [RequirePermission("billtitles.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await billTitleService.DeleteBillTitleAsync(id);
            TempData["SuccessMessage"] = "Bill title deleted successfully!";
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
        [RequirePermission("billtitles.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await billTitleService.DeleteBillTitleAsync(id); return Json(new { success = true, message = "Bill title deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
