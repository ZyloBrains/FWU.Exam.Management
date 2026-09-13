using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("examcenters.view")]
public class SymbolNumberGenerationController(
    ISymbolNumberService symbolNumberService,
    AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(int? examScheduleId, int? startSequence, int? sequenceWidth, string? prefix)
    {
        ViewData["ExamScheduleId"] = new SelectList(
            await context.ExamSchedules.AsNoTracking().OrderByDescending(es => es.Id).ToListAsync(),
            "Id", "ExamScheduleName", examScheduleId);

        if (!examScheduleId.HasValue) return View(null);

        var dto = await symbolNumberService.GetOverviewAsync(examScheduleId.Value, startSequence, sequenceWidth, prefix);
        return View(dto);
    }

    [HttpPost]
    [RequirePermission("examcenters.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(int examScheduleId, int? startSequence, int? sequenceWidth, string? prefix)
    {
        try
        {
            var result = await symbolNumberService.GenerateAsync(examScheduleId, startSequence, sequenceWidth, prefix);
            TempData["SuccessMessage"] = result.Message;
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { examScheduleId, startSequence, sequenceWidth, prefix });
    }

    [HttpPost]
    [RequirePermission("examcenters.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSymbolNumber(int registrationId, string symbolNumber, int examScheduleId)
    {
        try
        {
            await symbolNumberService.UpdateSymbolNumberAsync(registrationId, symbolNumber);
            TempData["SuccessMessage"] = $"Symbol number updated to '{symbolNumber}'.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { examScheduleId });
    }
}
