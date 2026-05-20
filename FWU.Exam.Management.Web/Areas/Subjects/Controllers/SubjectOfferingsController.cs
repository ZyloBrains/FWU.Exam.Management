using System.Text;
using System.Text.Json;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Subjects.Controllers;

[Area("Subjects")]
public class SubjectOfferingsController : Controller
{
    private readonly ISubjectOfferingService _subjectOfferingService;

    public SubjectOfferingsController(ISubjectOfferingService subjectOfferingService)
    {
        _subjectOfferingService = subjectOfferingService;
    }

    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Subject", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await _subjectOfferingService.GetSubjectOfferingsAsync(page, pageSize, search, sort, sortDir);

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

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "Subject", string sortDir = "asc")
    {
        var items = await _subjectOfferingService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Subject,Program,Semester,Compulsory,Theory Marks,Practical Marks,Internal Marks");

        foreach (var s in items)
        {
            sb.AppendLine($"{EscapeCsv(s.SubjectCatalog?.SubjectName ?? "-")}," +
                           $"{EscapeCsv(s.Program?.ProgramName ?? "-")}," +
                           $"{EscapeCsv(s.Semester?.Name ?? "-")}," +
                           $"{(s.IsCompulsory ? "Yes" : "No")}," +
                           $"{s.TheoryFullMarks}," +
                           $"{s.PracticalFullMarks ?? 0}," +
                           $"{(s.InternalTheoryFullMarks ?? 0) + (s.InternalPracticalFullMarks ?? 0)}");
        }

        var fileName = $"SubjectOfferings_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "Subject", string sortDir = "asc")
    {
        var (items, totalCount) = await _subjectOfferingService.GetSubjectOfferingsAsync(page, pageSize, search, sort, sortDir);

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

        var subjectOffering = await _subjectOfferingService.GetSubjectOfferingByIdAsync(id.Value);
        if (subjectOffering == null) return NotFound();

        return View(subjectOffering);
    }

    public async Task<IActionResult> Create()
    {
        var (subjectCatalogs, programs, semesters) = await _subjectOfferingService.GetSelectListsAsync();
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName");
        ViewData["SemesterId"] = new SelectList(semesters, "Id", "Name");

        var subjectsData = subjectCatalogs.Select(s => new
        {
            id = s.Id,
            code = s.SubjectCode,
            name = s.SubjectName,
            type = s.SubjectType?.Name ?? "",
            credits = s.CreditHours
        });
        ViewBag.SubjectCatalogsJson = JsonSerializer.Serialize(subjectsData);

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SubjectOfferingBulkCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (model.Subjects == null || model.Subjects.Count == 0)
            {
                ModelState.AddModelError("", "Please add at least one subject.");
            }
            else
            {
                var offerings = model.Subjects.Select(s => new SubjectOffering
                {
                    SubjectCatalogId = s.SubjectCatalogId,
                    ProgramId = model.ProgramId,
                    SemesterId = model.SemesterId,
                    IsCompulsory = s.IsCompulsory,
                    DisplayOrder = s.DisplayOrder,
                    HasTheory = s.HasTheory,
                    HasPractical = s.HasPractical,
                    HasInternal = s.HasInternal,
                    TheoryFullMarks = s.TheoryFullMarks,
                    TheoryPassMarks = s.TheoryPassMarks,
                    PracticalFullMarks = s.PracticalFullMarks,
                    PracticalPassMarks = s.PracticalPassMarks,
                    InternalTheoryFullMarks = s.InternalTheoryFullMarks,
                    InternalTheoryPassMarks = s.InternalTheoryPassMarks,
                    InternalPracticalFullMarks = s.InternalPracticalFullMarks,
                    InternalPracticalPassMarks = s.InternalPracticalPassMarks
                }).ToList();

                await _subjectOfferingService.CreateSubjectOfferingsAsync(offerings);
                TempData["SuccessMessage"] = $"{offerings.Count} subject offering(s) created successfully.";
                return RedirectToAction(nameof(Index));
            }
        }

        var (subjectCatalogs, programs, semesters) = await _subjectOfferingService.GetSelectListsAsync();
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", model.ProgramId);
        ViewData["SemesterId"] = new SelectList(semesters, "Id", "Name", model.SemesterId);

        var subjectsData = subjectCatalogs.Select(s => new
        {
            id = s.Id,
            code = s.SubjectCode,
            name = s.SubjectName,
            type = s.SubjectType?.Name ?? "",
            credits = s.CreditHours
        });
        ViewBag.SubjectCatalogsJson = JsonSerializer.Serialize(subjectsData);

        return View(model);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var subjectOffering = await _subjectOfferingService.GetSubjectOfferingByIdAsync(id.Value);
        if (subjectOffering == null) return NotFound();

        var (subjectCatalogs, programs, semesters) = await _subjectOfferingService.GetSelectListsAsync(subjectOffering.SubjectCatalogId, subjectOffering.ProgramId, subjectOffering.SemesterId);
        ViewData["SubjectCatalogId"] = new SelectList(subjectCatalogs, "Id", "SubjectName", subjectOffering.SubjectCatalogId);
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", subjectOffering.ProgramId);
        ViewData["SemesterId"] = new SelectList(semesters, "Id", "Name", subjectOffering.SemesterId);
        return View(subjectOffering);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,SubjectCatalogId,ProgramId,SemesterId,IsCompulsory,DisplayOrder,HasTheory,HasPractical,HasInternal,TheoryFullMarks,TheoryPassMarks,PracticalFullMarks,PracticalPassMarks,InternalTheoryFullMarks,InternalTheoryPassMarks,InternalPracticalFullMarks,InternalPracticalPassMarks")] SubjectOffering subjectOffering)
    {
        if (id != subjectOffering.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await _subjectOfferingService.UpdateSubjectOfferingAsync(subjectOffering);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _subjectOfferingService.SubjectOfferingExistsAsync(subjectOffering.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        var (subjectCatalogs, programs, semesters) = await _subjectOfferingService.GetSelectListsAsync(subjectOffering.SubjectCatalogId, subjectOffering.ProgramId, subjectOffering.SemesterId);
        ViewData["SubjectCatalogId"] = new SelectList(subjectCatalogs, "Id", "SubjectName", subjectOffering.SubjectCatalogId);
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", subjectOffering.ProgramId);
        ViewData["SemesterId"] = new SelectList(semesters, "Id", "Name", subjectOffering.SemesterId);
        return View(subjectOffering);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var subjectOffering = await _subjectOfferingService.GetSubjectOfferingByIdAsync(id.Value);
        if (subjectOffering == null) return NotFound();

        return View(subjectOffering);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _subjectOfferingService.DeleteSubjectOfferingAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
