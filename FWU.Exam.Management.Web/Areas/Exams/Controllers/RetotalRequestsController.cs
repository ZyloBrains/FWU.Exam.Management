using System.Text;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("retotaling.view")]
public class RetotalRequestsController(
    IRetotalRequestService retotalRequestService,
    UserManager<AppUser> userManager,
    AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await retotalRequestService.GetRetotalRequestsAsync(page, pageSize, search, sort, sortDir);

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

        var retotalRequest = await retotalRequestService.GetRetotalRequestByIdAsync(id.Value);
        if (retotalRequest == null) return NotFound();

        return View(retotalRequest);
    }

    [RequirePermission("retotaling.review")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkUnderReview(int id)
    {
        var user = await userManager.GetUserAsync(User);
        await retotalRequestService.MarkUnderReviewAsync(id, user?.UserName ?? "system");
        return RedirectToAction(nameof(Details), new { id });
    }

    [RequirePermission("retotaling.approve")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, string? retotalledGradeLetter, float? retotalledMarks, string? adminRemarks)
    {
        var user = await userManager.GetUserAsync(User);
        await retotalRequestService.ApproveRetotalRequestAsync(id, retotalledGradeLetter, retotalledMarks, adminRemarks, user?.UserName ?? "system");
        return RedirectToAction(nameof(Details), new { id });
    }

    [RequirePermission("retotaling.reject")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? adminRemarks)
    {
        var user = await userManager.GetUserAsync(User);
        await retotalRequestService.RejectRetotalRequestAsync(id, adminRemarks, user?.UserName ?? "system");
        return RedirectToAction(nameof(Details), new { id });
    }

    [RequirePermission("retotaling.view")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var retotalRequest = await retotalRequestService.GetRetotalRequestByIdAsync(id.Value);
        if (retotalRequest == null) return NotFound();

        return View(retotalRequest);
    }

    [HttpPost, ActionName("Delete")]
    [RequirePermission("retotaling.view")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await retotalRequestService.DeleteRetotalRequestAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [RequirePermission("retotaling.view")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try
        {
            await retotalRequestService.DeleteRetotalRequestAsync(id);
            return Json(new { success = true, message = "Retotal request deleted successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> ExportToCsv(string? search = null)
    {
        var items = await retotalRequestService.GetFilteredItemsAsync(search);

        var sb = new StringBuilder();
        sb.AppendLine("ID,Student,Subject,Status,Requested Date,Fee Paid,Reviewed By,Reviewed Date");

        foreach (var item in items)
        {
            var studentName = item.StudentRegistration != null ? $"{item.StudentRegistration.FirstName} {item.StudentRegistration.LastName}" : "";
            var subjectName = item.ExamSubjectResult?.SubjectOffering?.SubjectCatalog?.SubjectName ?? "";
            var reviewedDateStr = item.ReviewedDate?.ToString("yyyy-MM-dd") ?? "";
            sb.AppendLine($"{item.Id},{EscapeCsv(studentName)},{EscapeCsv(subjectName)},{item.Status},{item.RequestedDate:yyyy-MM-dd},{item.FeePaid},{EscapeCsv(item.ReviewedByUsername ?? "")},{reviewedDateStr}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", "RetotalRequests.csv");
    }

    private static string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}
