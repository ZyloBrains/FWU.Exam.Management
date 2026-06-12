using System.Text;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Payments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Payments.Controllers;

[Area("Payments")]
[RequirePermission("billtitles.view")]
public class BillTitlesController(IBillTitleService billTitleService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "BillTitleName", string sortDir = "asc", int pageSize = 10)
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

    private string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "BillTitleName", string sortDir = "asc")
    {
        var items = await billTitleService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Bill Title Name,Category,Amount,Exam Schedule,Applicable Date,Through Date,Status");

        foreach (var bt in items)
        {
            sb.AppendLine($"{EscapeCsv(bt.BillTitleName)}," +
                           $"{EscapeCsv(bt.Category ?? "-")}," +
                           $"{bt.Amount?.ToString("F2") ?? "-"}," +
                           $"{EscapeCsv(bt.ExamSchedule?.ExamScheduleName ?? "-")}," +
                           $"{bt.ApplicableDate?.ToString("yyyy-MM-dd") ?? "-"}," +
                           $"{bt.ThroughDate?.ToString("yyyy-MM-dd") ?? "-"}," +
                           $"{(bt.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"BillTitles_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "BillTitleName", string sortDir = "asc")
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

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var billTitle = await billTitleService.GetBillTitleByIdAsync(id.Value);
        if (billTitle == null) return NotFound();

        return View(billTitle);
    }

    [RequirePermission("billtitles.create")]
    public async Task<IActionResult> Create()
    {
        var examSchedules = await billTitleService.GetExamSchedulesAsync();
        ViewData["ExamScheduleId"] = new SelectList(examSchedules, "Id", "ExamScheduleName");
        return View();
    }

    [RequirePermission("billtitles.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,BillTitleName,Category,IsActive,Amount,ThroughDate,ApplicableDate,ExamScheduleId")] BillTitle billTitle)
    {
        if (ModelState.IsValid)
        {
            await billTitleService.CreateBillTitleAsync(billTitle);
            return RedirectToAction(nameof(Index));
        }
        var examSchedules = await billTitleService.GetExamSchedulesAsync();
        ViewData["ExamScheduleId"] = new SelectList(examSchedules, "Id", "ExamScheduleName", billTitle.ExamScheduleId);
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
        return View(billTitle);
    }

    [RequirePermission("billtitles.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,BillTitleName,Category,IsActive,Amount,ThroughDate,ApplicableDate,ExamScheduleId")] BillTitle billTitle)
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
            return RedirectToAction(nameof(Index));
        }
        var examSchedules = await billTitleService.GetExamSchedulesAsync();
        ViewData["ExamScheduleId"] = new SelectList(examSchedules, "Id", "ExamScheduleName", billTitle.ExamScheduleId);
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
        await billTitleService.DeleteBillTitleAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
