using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("examsubjectresults.view")]
public class ExamSubjectResultsController(
    IExamSubjectResultService examSubjectResultService,
    IUserContext userContext,
    AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10, int? examScheduleId = null, int? examRegistrationId = null)
    {
        var (items, totalCount) = await examSubjectResultService.GetExamSubjectResultsAsync(page, pageSize, search, sort, sortDir, examScheduleId, examRegistrationId);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;
        ViewBag.ExamScheduleId = examScheduleId;
        ViewBag.ExamRegistrationId = examRegistrationId;

        ViewData["ExamScheduleId"] = new SelectList(context.ExamSchedules.AsNoTracking().Select(es => new { es.Id, es.ExamScheduleName }), "Id", "ExamScheduleName", examScheduleId);
        ViewData["ExamRegistrationId"] = new SelectList(context.ExamRegistrations.AsNoTracking().Select(er => new { er.Id, Name = "Reg #" + er.Id }), "Id", "Name", examRegistrationId);

        return View(items);
    }

    [RequirePermission("examsubjectresults.create")]
    public async Task<IActionResult> Create()
    {
        var selectLists = await examSubjectResultService.GetSelectListDataAsync();
        PopulateDropdowns(selectLists);
        return View();
    }

    [HttpPost]
    [RequirePermission("examsubjectresults.create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ExamRegistrationId,ExamTypeId,SubjectOfferingId,ExamScheduleId,ObtainedMarksTheory,ObtainedMarksTheoryConfirm,ObtainedMarksPractical,ObtainedMarksPracticalConfirm,ObtainedMarksTheoryInternal,ObtainedMarksPracticalInternal,GradeLetter,Remarks,IsActive,IsSubmitted,ObtainedMarks")] ExamSubjectResult examSubjectResult)
    {
        if (ModelState.IsValid)
        {
            await examSubjectResultService.CreateExamSubjectResultAsync(examSubjectResult);
            TempData["SuccessMessage"] = "Exam subject result created successfully!";
            return RedirectToAction(nameof(Index));
        }
        var selectLists = await examSubjectResultService.GetSelectListDataAsync();
        PopulateDropdowns(selectLists, examSubjectResult);
        return View(examSubjectResult);
    }

    [RequirePermission("examsubjectresults.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var examSubjectResult = await examSubjectResultService.GetExamSubjectResultByIdAsync(id.Value);
        if (examSubjectResult == null) return NotFound();

        var selectLists = await examSubjectResultService.GetSelectListDataAsync();
        PopulateDropdowns(selectLists, examSubjectResult);
        return View(examSubjectResult);
    }

    [HttpPost]
    [RequirePermission("examsubjectresults.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ExamRegistrationId,ExamTypeId,SubjectOfferingId,ExamScheduleId,ObtainedMarksTheory,ObtainedMarksTheoryConfirm,ObtainedMarksPractical,ObtainedMarksPracticalConfirm,ObtainedMarksTheoryInternal,ObtainedMarksPracticalInternal,GradeLetter,Remarks,IsActive,IsSubmitted,ObtainedMarks")] ExamSubjectResult examSubjectResult)
    {
        if (id != examSubjectResult.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await examSubjectResultService.UpdateExamSubjectResultAsync(examSubjectResult);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await examSubjectResultService.ExamSubjectResultExistsAsync(examSubjectResult.Id))
                    return NotFound();
                throw;
            }
            TempData["SuccessMessage"] = "Exam subject result updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        var selectLists = await examSubjectResultService.GetSelectListDataAsync();
        PopulateDropdowns(selectLists, examSubjectResult);
        return View(examSubjectResult);
    }

    [RequirePermission("examsubjectresults.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var examSubjectResult = await examSubjectResultService.GetExamSubjectResultByIdAsync(id.Value);
        if (examSubjectResult == null) return NotFound();

        return View(examSubjectResult);
    }

    [HttpPost, ActionName("Delete")]
    [RequirePermission("examsubjectresults.delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await examSubjectResultService.DeleteExamSubjectResultAsync(id);
            TempData["SuccessMessage"] = "Exam subject result deleted successfully!";
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

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var examSubjectResult = await examSubjectResultService.GetExamSubjectResultByIdAsync(id.Value);
        if (examSubjectResult == null) return NotFound();

        return View(examSubjectResult);
    }

    [RequirePermission("examsubjectresults.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try
        {
            await examSubjectResultService.DeleteExamSubjectResultAsync(id);
            return Json(new { success = true, message = "Subject result deleted successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    private void PopulateDropdowns(ExamSubjectResultSelectListsDto selectLists, ExamSubjectResult? examSubjectResult = null)
    {
        ViewData["ExamRegistrationId"] = new SelectList(selectLists.ExamRegistrations, "Id", "Name", examSubjectResult?.ExamRegistrationId);
        ViewData["SubjectOfferingId"] = new SelectList(selectLists.SubjectOfferings, "Id", "Name", examSubjectResult?.SubjectOfferingId);
        ViewData["ExamTypeId"] = new SelectList(selectLists.ExamTypes, "Id", "Name", examSubjectResult?.ExamTypeId);
        ViewData["ExamScheduleId"] = new SelectList(selectLists.ExamSchedules, "Id", "Name", examSubjectResult?.ExamScheduleId);
    }
}
