using System.Text;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("halltickets.view")]
public class HallTicketsController(
    IHallTicketService hallTicketService,
    UserManager<AppUser> userManager,
    AppDbContext context) : Controller
{
    private async Task<(int? collegeId, int? facultyId)> GetScopeAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return (null, null);

        if (User.IsInRole(Role.CollegeAdmin))
            return (user.CollegeId, null);

        if (User.IsInRole(Role.FacultyAdmin))
            return (null, user.FacultyId);

        return (null, null);
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10, int? examScheduleId = null)
    {
        var (collegeId, facultyId) = await GetScopeAsync();
        var (items, totalCount) = await hallTicketService.GetHallTicketsAsync(page, pageSize, search, sort, sortDir, collegeId, facultyId, examScheduleId);

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

    [RequirePermission("halltickets.generate")]
    public async Task<IActionResult> Generate(int? examScheduleId)
    {
        ViewData["ExamScheduleId"] = new SelectList(context.ExamSchedules.AsNoTracking().Select(es => new { es.Id, es.ExamScheduleName }), "Id", "ExamScheduleName", examScheduleId);

        if (examScheduleId.HasValue)
        {
            var registrations = await context.ExamRegistrations
                .Where(er => er.ExamScheduleId == examScheduleId.Value && er.IsActive && er.Status == Domain.Enums.RegistrationStatus.Registered)
                .Include(er => er.College)
                .ToListAsync();

            var existingTicketIds = await context.HallTickets
                .Where(ht => ht.ExamScheduleId == examScheduleId.Value && ht.IsActive)
                .Select(ht => ht.ExamRegistrationId)
                .ToListAsync();

            var pendingRegistrations = registrations.Where(r => !existingTicketIds.Contains(r.Id)).ToList();
            ViewBag.PendingCount = pendingRegistrations.Count;
            ViewBag.TotalCount = registrations.Count;
            ViewBag.ExistingCount = existingTicketIds.Count;
            ViewBag.SelectedScheduleId = examScheduleId.Value;
        }

        return View();
    }

    [HttpPost]
    [RequirePermission("halltickets.generate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateConfirmed(int examScheduleId)
    {
        var hallTickets = await hallTicketService.GenerateBulkHallTicketsAsync(examScheduleId);
        TempData["SuccessMessage"] = $"{hallTickets.Count} hall ticket(s) generated successfully!";
        return RedirectToAction(nameof(Index), new { examScheduleId });
    }

    [RequirePermission("halltickets.download")]
    public async Task<IActionResult> Download(int? id)
    {
        if (id == null) return NotFound();

        var hallTicket = await hallTicketService.GetHallTicketByIdAsync(id.Value);
        if (hallTicket == null) return NotFound();

        hallTicket.IsDownloaded = true;
        hallTicket.DownloadedDate = DateTime.UtcNow;
        await hallTicketService.UpdateHallTicketAsync(hallTicket);

        return View("PrintHallTicket", hallTicket);
    }

    [RequirePermission("halltickets.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var hallTicket = await hallTicketService.GetHallTicketByIdAsync(id.Value);
        if (hallTicket == null) return NotFound();

        return View(hallTicket);
    }

    [HttpPost, ActionName("Delete")]
    [RequirePermission("halltickets.delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await hallTicketService.DeleteHallTicketAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [RequirePermission("halltickets.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try
        {
            await hallTicketService.DeleteHallTicketAsync(id);
            return Json(new { success = true, message = "Hall ticket deleted successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var hallTicket = await hallTicketService.GetHallTicketByIdAsync(id.Value);
        if (hallTicket == null) return NotFound();

        return View(hallTicket);
    }

    public async Task<IActionResult> ExportToCsv(string? search = null)
    {
        var (collegeId, facultyId) = await GetScopeAsync();
        var items = await hallTicketService.GetFilteredItemsAsync(search, collegeId, facultyId);

        var sb = new StringBuilder();
        sb.AppendLine("ID,Hall Ticket Number,Exam Schedule,Generated Date,Downloaded,Is Active");

        foreach (var item in items)
        {
            sb.AppendLine($"{item.Id},{EscapeCsv(item.HallTicketNumber ?? "")},{EscapeCsv(item.ExamSchedule?.ExamScheduleName ?? "")},{item.GeneratedDate:yyyy-MM-dd},{(item.IsDownloaded ? "Yes" : "No")},{(item.IsActive ? "Yes" : "No")}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", "HallTickets.csv");
    }

    private static string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}
