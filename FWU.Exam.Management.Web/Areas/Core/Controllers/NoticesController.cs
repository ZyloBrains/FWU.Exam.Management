using System.Text;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[Authorize(Roles = "SuperAdmin,FacultyAdmin")]
public class NoticesController(INoticeService noticeService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "PublishedDate", string sortDir = "desc", int pageSize = 10)
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

    private string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "PublishedDate", string sortDir = "desc")
    {
        var items = await noticeService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Title,Preview,Published Date");

        foreach (var n in items)
        {
            sb.AppendLine($"{EscapeCsv(n.NoticeTitle)}," +
                           $"{EscapeCsv(n.NoticePreview)}," +
                           $"{(n.PublishedDate.HasValue ? n.PublishedDate.Value.ToString("yyyy-MM-dd") : "")}");
        }

        var fileName = $"Notices_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "PublishedDate", string sortDir = "desc")
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

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var notice = await noticeService.GetNoticeByIdAsync(id.Value);
        if (notice == null) return NotFound();

        return View(notice);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,NoticeTitle,NoticePreview,NoticeContent,PublishedDate")] Notice notice)
    {
        if (ModelState.IsValid)
        {
            await noticeService.CreateNoticeAsync(notice);
            return RedirectToAction(nameof(Index));
        }
        return View(notice);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var notice = await noticeService.GetNoticeByIdAsync(id.Value);
        if (notice == null) return NotFound();

        return View(notice);
    }

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
            return RedirectToAction(nameof(Index));
        }
        return View(notice);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var notice = await noticeService.GetNoticeByIdAsync(id.Value);
        if (notice == null) return NotFound();

        return View(notice);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await noticeService.DeleteNoticeAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
