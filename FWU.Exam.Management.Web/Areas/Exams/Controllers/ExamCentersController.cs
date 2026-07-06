using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("examcenters.view")]
public class ExamCentersController(
    IExamCenterService examCenterService,
    AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await examCenterService.GetExamCentersAsync(page, pageSize, search, sort, sortDir);

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
        var examCenter = await examCenterService.GetExamCenterByIdAsync(id.Value);
        if (examCenter == null) return NotFound();
        return View(examCenter);
    }

    [RequirePermission("examcenters.create")]
    public async Task<IActionResult> Create()
    {
        ViewData["ExamScheduleId"] = new SelectList(await context.ExamSchedules.AsNoTracking().ToListAsync(), "Id", "ExamScheduleName");
        ViewData["VenueCollegeList"] = await context.Colleges
            .AsNoTracking()
            .Where(c => c.IsActive && c.IsExamCenterOnly)
            .OrderBy(c => c.Name)
            .ToListAsync();
        ViewData["SourceCollegeList"] = await context.Colleges
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("examcenters.create")]
    public async Task<IActionResult> Create([Bind("Code,ExamScheduleId,Remark,IsActive")] ExamCenter examCenter,
        int[] venueColleges, int[] sourceColleges)
    {
        if (ModelState.IsValid)
        {
            await examCenterService.CreateExamCenterWithCollegesAsync(
                examCenter,
                [.. venueColleges],
                [.. sourceColleges]);
            return RedirectToAction(nameof(Index));
        }

        ViewData["ExamScheduleId"] = new SelectList(await context.ExamSchedules.AsNoTracking().ToListAsync(), "Id", "ExamScheduleName", examCenter.ExamScheduleId);
        ViewData["VenueCollegeList"] = await context.Colleges
            .AsNoTracking()
            .Where(c => c.IsActive && c.IsExamCenterOnly)
            .OrderBy(c => c.Name)
            .ToListAsync();
        ViewData["SourceCollegeList"] = await context.Colleges
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View(examCenter);
    }

    [RequirePermission("examcenters.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var examCenter = await examCenterService.GetExamCenterByIdAsync(id.Value);
        if (examCenter == null) return NotFound();

        ViewData["ExamScheduleId"] = new SelectList(await context.ExamSchedules.AsNoTracking().ToListAsync(), "Id", "ExamScheduleName", examCenter.ExamScheduleId);
        ViewData["VenueCollegeList"] = await context.Colleges
            .AsNoTracking()
            .Where(c => c.IsActive && c.IsExamCenterOnly)
            .OrderBy(c => c.Name)
            .ToListAsync();
        ViewData["SourceCollegeList"] = await context.Colleges
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        ViewData["SelectedVenueIds"] = examCenter.ExamCenterVenues?.Select(ecv => ecv.CollegeId).ToArray() ?? [];
        ViewData["SelectedSourceIds"] = examCenter.ExamCenterColleges?.Select(ecc => ecc.CollegeId).ToArray() ?? [];
        return View(examCenter);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("examcenters.edit")]
    public async Task<IActionResult> Edit(int id,
        [Bind("Id,Code,ExamScheduleId,Remark,IsActive")] ExamCenter examCenter,
        int[] venueColleges, int[] sourceColleges)
    {
        if (id != examCenter.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await examCenterService.UpdateExamCenterWithCollegesAsync(
                    examCenter,
                    [.. venueColleges],
                    [.. sourceColleges]);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await examCenterService.ExamCenterExistsAsync(examCenter.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["ExamScheduleId"] = new SelectList(await context.ExamSchedules.AsNoTracking().ToListAsync(), "Id", "ExamScheduleName", examCenter.ExamScheduleId);
        ViewData["VenueCollegeList"] = await context.Colleges
            .AsNoTracking()
            .Where(c => c.IsActive && c.IsExamCenterOnly)
            .OrderBy(c => c.Name)
            .ToListAsync();
        ViewData["SourceCollegeList"] = await context.Colleges
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        ViewData["SelectedVenueIds"] = venueColleges;
        ViewData["SelectedSourceIds"] = sourceColleges;
        return View(examCenter);
    }

    [RequirePermission("examcenters.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var examCenter = await examCenterService.GetExamCenterByIdAsync(id.Value);
        if (examCenter == null) return NotFound();
        return View(examCenter);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [RequirePermission("examcenters.delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await examCenterService.DeleteExamCenterAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [RequirePermission("examcenters.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try
        {
            await examCenterService.DeleteExamCenterAsync(id);
            return Json(new { success = true, message = "Exam center deleted successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
