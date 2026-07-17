using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FWU.Exam.Management.Infrastructure;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("examcenters.view")]
public class ExamCenterDistributionController(
    IExamCenterDistributionService distributionService,
    IExamRollNumberService rollNumberService,
    AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(int? examScheduleId)
    {
        ViewData["ExamScheduleId"] = new SelectList(
            await context.ExamSchedules.AsNoTracking().OrderByDescending(es => es.Id).ToListAsync(),
            "Id", "ExamScheduleName", examScheduleId);

        if (examScheduleId.HasValue)
        {
            var dto = new ExamCenterDistributionDto
            {
                ExamScheduleId = examScheduleId.Value,
                ExamScheduleName = await context.ExamSchedules
                    .Where(es => es.Id == examScheduleId.Value)
                    .Select(es => es.ExamScheduleName)
                    .FirstOrDefaultAsync(),
                TotalRegistrations = await distributionService.GetRegisteredCountAsync(examScheduleId.Value),
                AssignedCount = await distributionService.GetAssignedCountAsync(examScheduleId.Value),
                UnassignedCount = await distributionService.GetUnassignedCountAsync(examScheduleId.Value),
                SymbolNumbersAssigned = await context.ExamRegistrations
                    .AnyAsync(er => er.ExamScheduleId == examScheduleId.Value && er.SymbolNumber != null),
                RollNumbersAssigned = await rollNumberService.HasRollNumbersAsync(examScheduleId.Value),
                RollNumberCount = await context.ExamRegistrations
                    .CountAsync(er => er.ExamScheduleId == examScheduleId.Value && er.ExamRollNumber != null)
            };

            var examCenters = await context.ExamCenters
                .AsNoTracking()
                .Where(ec => ec.ExamScheduleId == examScheduleId.Value && ec.IsActive)
                .Include(ec => ec.College)
                .Include(ec => ec.ExamCenterVenues)
                    .ThenInclude(ecv => ecv.College)
                .Include(ec => ec.ExamCenterColleges)
                    .ThenInclude(ecc => ecc.College)
                .ToListAsync();

            var ranges = await distributionService.GetRangesAsync(examScheduleId.Value);
            var distributionCounts = await distributionService.GetCenterDistributionCountsAsync(examScheduleId.Value);

            foreach (var center in examCenters)
            {
                var range = ranges.FirstOrDefault(r => r.ExamCenterId == center.Id);
                dto.Centers.Add(new CenterDistributionInfo
                {
                    ExamCenterId = center.Id,
                    CenterCode = center.Code,
                    VenueColleges = center.ExamCenterVenues?
                        .Select(ecv => ecv.College?.Name ?? "")
                        .Where(n => !string.IsNullOrEmpty(n))
                        .ToList() ?? [],
                    FromSymbolNumber = range?.FromSymbolNumber,
                    ToSymbolNumber = range?.ToSymbolNumber,
                    StudentCount = distributionCounts.GetValueOrDefault(center.Id, 0),
                    SourceColleges = center.ExamCenterColleges?
                        .Select(ecc => ecc.College?.Name ?? "")
                        .Where(n => !string.IsNullOrEmpty(n))
                        .ToList() ?? []
                });
            }

            return View(dto);
        }

        return View(null);
    }

    [HttpPost]
    [RequirePermission("examcenters.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignSymbolNumbers(int examScheduleId)
    {
        await distributionService.AssignSymbolNumbersAsync(examScheduleId);
        TempData["SuccessMessage"] = "Symbol numbers assigned successfully!";
        return RedirectToAction(nameof(Index), new { examScheduleId });
    }

    [HttpPost]
    [RequirePermission("examcenters.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveRanges(int examScheduleId, int[] examCenterIds, long[] fromSymbols, long[] toSymbols)
    {
        await distributionService.ClearRangesAsync(examScheduleId);

        for (int i = 0; i < examCenterIds.Length; i++)
        {
            if (fromSymbols[i] > 0 && toSymbols[i] >= fromSymbols[i])
            {
                await distributionService.SetSymbolRangeAsync(examCenterIds[i], examScheduleId, fromSymbols[i], toSymbols[i]);
            }
        }

        TempData["SuccessMessage"] = "Symbol number ranges saved!";
        return RedirectToAction(nameof(Index), new { examScheduleId });
    }

    [HttpPost]
    [RequirePermission("examcenters.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DistributeStudents(int examScheduleId)
    {
        var assigned = await distributionService.DistributeStudentsAsync(examScheduleId);
        TempData["SuccessMessage"] = $"{assigned} students assigned to exam centers!";
        return RedirectToAction(nameof(Index), new { examScheduleId });
    }

    [HttpPost]
    [RequirePermission("examcenters.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetDistribution(int examScheduleId)
    {
        await distributionService.ResetDistributionAsync(examScheduleId);
        TempData["SuccessMessage"] = "Distribution has been reset.";
        return RedirectToAction(nameof(Index), new { examScheduleId });
    }

    [HttpPost]
    [RequirePermission("examcenters.generaterollnumbers")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateRollNumbers(int examScheduleId)
    {
        var count = await rollNumberService.GenerateRollNumbersAsync(examScheduleId);
        TempData["SuccessMessage"] = $"Roll numbers generated for {count} students!";
        return RedirectToAction(nameof(Index), new { examScheduleId });
    }

    [HttpPost]
    [RequirePermission("examcenters.generaterollnumbers")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearRollNumbers(int examScheduleId)
    {
        var count = await rollNumberService.ClearRollNumbersAsync(examScheduleId);
        TempData["SuccessMessage"] = $"Roll numbers cleared for {count} students.";
        return RedirectToAction(nameof(Index), new { examScheduleId });
    }
}
