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
                RollNumbersAssigned = false,
                RollNumberCount = 0
            };

            var examCenters = await context.ExamCenters
                .AsNoTracking()
                .Where(ec => ec.ExamScheduleId == examScheduleId.Value && ec.IsActive)
                .Include(ec => ec.College)
                .Include(ec => ec.ExamCenterVenues)
                    .ThenInclude(ecv => ecv.College)
                .ToListAsync();

            var distributionCounts = await distributionService.GetCenterDistributionCountsAsync(examScheduleId.Value);

            foreach (var center in examCenters)
            {
                dto.Centers.Add(new CenterDistributionInfo
                {
                    ExamCenterId = center.Id,
                    CenterCode = center.Code,
                    VenueColleges = center.ExamCenterVenues?
                        .Select(ecv => ecv.College?.Name ?? "")
                        .Where(n => !string.IsNullOrEmpty(n))
                        .ToList() ?? [],
                    StudentCount = distributionCounts.GetValueOrDefault(center.Id, 0),
                });
            }

            return View(dto);
        }

        return View(null);
    }

    [HttpPost]
    [RequirePermission("examcenters.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignAndDistribute(int examScheduleId)
    {
        await distributionService.AssignSymbolNumbersAsync(examScheduleId);
        var count = await distributionService.DistributeStudentsAsync(examScheduleId);
        TempData["SuccessMessage"] = $"Symbol numbers assigned and {count} students distributed to exam centers!";
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
}
