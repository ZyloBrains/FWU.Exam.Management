using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Domain.Extensions;
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
    IAuditLogWriter auditLogWriter) : Controller
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
        await auditLogWriter.LogAsync(ActivityTypes.RetotalUnderReview, $"Retotal request {id} marked as under review", new { requestId = id }, entityName: "RetotalRequest", entityId: id.ToString());
        return RedirectToAction(nameof(Details), new { id });
    }

    [RequirePermission("retotaling.approve")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, string? retotalledGradeLetter, float? retotalledMarks, string? adminRemarks)
    {
        var user = await userManager.GetUserAsync(User);
        await retotalRequestService.ApproveRetotalRequestAsync(id, retotalledGradeLetter, retotalledMarks, adminRemarks, user?.UserName ?? "system");
        await auditLogWriter.LogAsync(ActivityTypes.RetotalApproved, $"Retotal request {id} approved", new { requestId = id, retotalledGradeLetter, retotalledMarks, adminRemarks }, entityName: "RetotalRequest", entityId: id.ToString());
        return RedirectToAction(nameof(Details), new { id });
    }

    [RequirePermission("retotaling.reject")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? adminRemarks)
    {
        var user = await userManager.GetUserAsync(User);
        await retotalRequestService.RejectRetotalRequestAsync(id, adminRemarks, user?.UserName ?? "system");
        await auditLogWriter.LogAsync(ActivityTypes.RetotalRejected, $"Retotal request {id} rejected", new { requestId = id, adminRemarks }, entityName: "RetotalRequest", entityId: id.ToString());
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
        try
        {
            await retotalRequestService.DeleteRetotalRequestAsync(id);
            await auditLogWriter.LogAsync(ActivityTypes.RetotalRequested, $"Retotal request {id} deleted", new { requestId = id }, entityName: "RetotalRequest", entityId: id.ToString());
            TempData["SuccessMessage"] = "Retotal request deleted successfully!";
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

    [RequirePermission("retotaling.view")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try
        {
            await retotalRequestService.DeleteRetotalRequestAsync(id);
            await auditLogWriter.LogAsync(ActivityTypes.RetotalRequested, $"Retotal request {id} deleted", new { requestId = id }, entityName: "RetotalRequest", entityId: id.ToString());
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
            var studentName = item.StudentRegistration != null ? item.StudentRegistration.FirstName.GetFullName(item.StudentRegistration.LastName) : "";
            var subjectName = item.ExamSubjectResult?.SubjectOffering?.SubjectCatalog?.SubjectName ?? "";
            var reviewedDateStr = item.ReviewedDate?.ToString("yyyy-MM-dd") ?? "";
            sb.AppendLine($"{item.Id},{studentName.EscapeCsv()},{subjectName.EscapeCsv()},{item.Status},{item.RequestedDate:yyyy-MM-dd},{item.FeePaid},{(item.ReviewedByUsername ?? "").EscapeCsv()},{reviewedDateStr}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        await auditLogWriter.LogAsync(ActivityTypes.ResultExported, "Retotal requests exported to CSV", new { format = "csv", count = items.Count }, entityName: "RetotalRequest");
        return File(csvBytes, "text/csv", "RetotalRequests.csv");
    }

    public async Task<IActionResult> ExportToPdf(string? search = null)
    {
        var items = await retotalRequestService.GetFilteredItemsAsync(search);
        await auditLogWriter.LogAsync(ActivityTypes.ResultExported, "Retotal requests exported to PDF", new { format = "pdf", count = items.Count }, entityName: "RetotalRequest");
        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(string? search = null)
    {
        var items = await retotalRequestService.GetFilteredItemsAsync(search);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("RetotalRequests");

        var headers = new[] { "ID", "Student", "Subject", "Status", "Requested Date", "Fee Paid", "Reviewed By", "Reviewed Date" };
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
            var studentName = item.StudentRegistration != null ? item.StudentRegistration.FirstName.GetFullName(item.StudentRegistration.LastName) : "";
            var subjectName = item.ExamSubjectResult?.SubjectOffering?.SubjectCatalog?.SubjectName ?? "";
            worksheet.Cell(row, 1).Value = item.Id;
            worksheet.Cell(row, 2).Value = studentName;
            worksheet.Cell(row, 3).Value = subjectName;
            worksheet.Cell(row, 4).Value = item.Status.ToString();
            worksheet.Cell(row, 5).Value = item.RequestedDate.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 6).Value = item.FeePaid;
            worksheet.Cell(row, 7).Value = item.ReviewedByUsername ?? "";
            worksheet.Cell(row, 8).Value = item.ReviewedDate?.ToString("yyyy-MM-dd") ?? "";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        await auditLogWriter.LogAsync(ActivityTypes.ResultExported, "Retotal requests exported to Excel", new { format = "excel", count = items.Count }, entityName: "RetotalRequest");
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RetotalRequests.xlsx");
    }

}
