using System.Text;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("admitcards.view")]
public class AdmitCardsController(
    IAdmitCardService admitCardService,
    AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10, int? examScheduleId = null)
    {
        var (items, totalCount) = await admitCardService.GetAdmitCardsAsync(page, pageSize, search, sort, sortDir, examScheduleId);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;
        ViewBag.ExamScheduleId = examScheduleId;

        ViewData["ExamScheduleId"] = new SelectList(context.ExamSchedules.AsNoTracking().Select(es => new { es.Id, es.ExamScheduleName }), "Id", "ExamScheduleName", examScheduleId);

        return View(items);
    }

    [RequirePermission("admitcards.generate")]
    public async Task<IActionResult> Generate(int? examScheduleId)
    {
        ViewData["ExamScheduleId"] = new SelectList(context.ExamSchedules.AsNoTracking().Select(es => new { es.Id, es.ExamScheduleName }), "Id", "ExamScheduleName", examScheduleId);

        if (examScheduleId.HasValue)
        {
            var registrations = await context.ExamRegistrations
                .Where(er => er.ExamScheduleId == examScheduleId.Value && er.IsActive && er.Status == Domain.Enums.RegistrationStatus.Registered)
                .Include(er => er.College)
                .ToListAsync();

            var existingTicketIds = await context.AdmitCards
                .Where(ht => ht.ExamScheduleId == examScheduleId.Value && ht.IsActive)
                .Select(ht => ht.ExamRegistrationId)
                .ToListAsync();

            var pendingRegistrations = registrations.Where(r => !existingTicketIds.Contains(r.Id)).ToList();
            var missingSymbolCount = pendingRegistrations.Count(r => string.IsNullOrEmpty(r.SymbolNumber));
            ViewBag.PendingCount = pendingRegistrations.Count;
            ViewBag.TotalCount = registrations.Count;
            ViewBag.ExistingCount = existingTicketIds.Count;
            ViewBag.MissingSymbolCount = missingSymbolCount;
            ViewBag.SelectedScheduleId = examScheduleId.Value;
        }

        return View();
    }

    [HttpPost]
    [RequirePermission("admitcards.generate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateConfirmed(int examScheduleId)
    {
        try
        {
            var admitCards = await admitCardService.GenerateBulkAdmitCardsAsync(examScheduleId);
            TempData["SuccessMessage"] = $"{admitCards.Count} admit card(s) generated successfully!";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Index), new { examScheduleId });
    }

    [RequirePermission("admitcards.download")]
    public async Task<IActionResult> Download(int? id)
    {
        if (id == null) return NotFound();

        var admitCard = await admitCardService.GetAdmitCardByIdAsync(id.Value);
        if (admitCard == null) return NotFound();

        admitCard.IsDownloaded = true;
        admitCard.DownloadedDate = DateTime.UtcNow;
        await admitCardService.UpdateAdmitCardAsync(admitCard);

        return View("PrintAdmitCard", admitCard);
    }

    [RequirePermission("admitcards.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var admitCard = await admitCardService.GetAdmitCardByIdAsync(id.Value);
        if (admitCard == null) return NotFound();

        return View(admitCard);
    }

    [HttpPost, ActionName("Delete")]
    [RequirePermission("admitcards.delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await admitCardService.DeleteAdmitCardAsync(id);
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

    [RequirePermission("admitcards.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try
        {
            await admitCardService.DeleteAdmitCardAsync(id);
            return Json(new { success = true, message = "Admit card deleted successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var admitCard = await admitCardService.GetAdmitCardByIdAsync(id.Value);
        if (admitCard == null) return NotFound();

        return View(admitCard);
    }

    public async Task<IActionResult> ExportToCsv(string? search = null)
    {
        var items = await admitCardService.GetFilteredItemsAsync(search);

        var sb = new StringBuilder();
        sb.AppendLine("ID,Admit Card Number,Exam Schedule,Generated Date,Downloaded,Is Active");

        foreach (var item in items)
        {
            sb.AppendLine($"{item.Id},{EscapeCsv(item.AdmitCardNumber ?? "")},{EscapeCsv(item.ExamSchedule?.ExamScheduleName ?? "")},{item.GeneratedDate:yyyy-MM-dd},{(item.IsDownloaded ? "Yes" : "No")},{(item.IsActive ? "Yes" : "No")}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", "AdmitCards.csv");
    }

    private static string EscapeCsv(string? field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}
