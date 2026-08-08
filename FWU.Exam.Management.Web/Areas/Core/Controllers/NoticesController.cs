using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("notices.view")]
public class NoticesController(INoticeService noticeService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "PublishedDate", string sortDir = "desc", int pageSize = 10)
    {
        var (items, totalCount) = await noticeService.GetNoticesAsync(page, pageSize, search, sort, sortDir);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }


    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string? search = null, string sort = "PublishedDate", string sortDir = "desc")
    {
        var items = await noticeService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Title,Preview,Published Date");

        foreach (var n in items)
        {
            sb.AppendLine($"{n.NoticeTitle.EscapeCsv()}," +
                           $"{n.NoticePreview.EscapeCsv()}," +
                           $"{(n.PublishedDate.HasValue ? n.PublishedDate.Value.ToString("yyyy-MM-dd") : "")}");
        }

        var fileName = $"Notices_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string? search = null, string sort = "PublishedDate", string sortDir = "desc")
    {
        var (items, totalCount) = await noticeService.GetNoticesAsync(page, pageSize, search, sort, sortDir);

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = totalCount;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string? search = null, string sort = "PublishedDate", string sortDir = "desc")
    {
        var items = await noticeService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Notices");

        var headers = new[] { "Title", "Preview", "Published Date" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var n in items)
        {
            worksheet.Cell(row, 1).Value = n.NoticeTitle;
            worksheet.Cell(row, 2).Value = n.NoticePreview;
            worksheet.Cell(row, 3).Value = n.PublishedDate.HasValue ? n.PublishedDate.Value.ToString("yyyy-MM-dd") : "";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"Notices_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var notice = await noticeService.GetNoticeByIdAsync(id.Value);
        if (notice == null) return NotFound();

        return View(notice);
    }

    [RequirePermission("notices.create")]
    public IActionResult Create()
    {
        return View();
    }

    [RequirePermission("notices.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,NoticeTitle,NoticePreview,NoticeContent,PublishedDate")] Notice notice)
    {
        if (ModelState.IsValid)
        {
            await noticeService.CreateNoticeAsync(notice);
            TempData["SuccessMessage"] = "Notice created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(notice);
    }

    [RequirePermission("notices.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var notice = await noticeService.GetNoticeByIdAsync(id.Value);
        if (notice == null) return NotFound();

        return View(notice);
    }

    [RequirePermission("notices.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,NoticeTitle,NoticePreview,NoticeContent,PublishedDate")] Notice notice)
    {
        if (id != notice.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await noticeService.UpdateNoticeAsync(notice);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await noticeService.NoticeExistsAsync(notice.Id))
                    return NotFound();
                throw;
            }
            TempData["SuccessMessage"] = "Notice updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(notice);
    }

    [RequirePermission("notices.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var notice = await noticeService.GetNoticeByIdAsync(id.Value);
        if (notice == null) return NotFound();

        return View(notice);
    }

    [RequirePermission("notices.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await noticeService.DeleteNoticeAsync(id);
            TempData["SuccessMessage"] = "Notice deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
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
        [RequirePermission("notices.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await noticeService.DeleteNoticeAsync(id); return Json(new { success = true, message = "Notice deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
