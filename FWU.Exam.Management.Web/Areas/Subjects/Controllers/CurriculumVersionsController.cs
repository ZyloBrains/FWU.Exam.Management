using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Extensions;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Subjects.Controllers;

[Area("Subjects")]
[RequirePermission("curriculumversions.view")]
public class CurriculumVersionsController : Controller
{
    private readonly ICurriculumVersionService _curriculumVersionService;
    private readonly ISubjectOfferingService _subjectOfferingService;

    public CurriculumVersionsController(ICurriculumVersionService curriculumVersionService, ISubjectOfferingService subjectOfferingService)
    {
        _curriculumVersionService = curriculumVersionService;
        _subjectOfferingService = subjectOfferingService;
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "id", string sortDir = "desc", int pageSize = 10)
    {
        var (items, totalCount) = await _curriculumVersionService.GetCurriculumVersionsAsync(page, pageSize, search, sort, sortDir);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }


    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string? search = null, string sort = "id", string sortDir = "desc")
    {
        var items = await _curriculumVersionService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Name,Program,Effective Year,Status");

        foreach (var c in items)
        {
            sb.AppendLine($"{c.Name.EscapeCsv()}," +
                           $"{(c.Program?.ProgramName ?? "-").EscapeCsv()}," +
                           $"{(c.EffectiveAcademicYear?.AcademicYearName ?? "-").EscapeCsv()}," +
                           $"{(c.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"CurriculumVersions_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string? search = null, string sort = "id", string sortDir = "desc")
    {
        var items = await _curriculumVersionService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Curriculum Versions");

        var headers = new[] { "Name", "Program", "Effective Year", "Status" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var c in items)
        {
            worksheet.Cell(row, 1).Value = c.Name ?? "-";
            worksheet.Cell(row, 2).Value = c.Program?.ProgramName ?? "-";
            worksheet.Cell(row, 3).Value = c.EffectiveAcademicYear?.AcademicYearName ?? "-";
            worksheet.Cell(row, 4).Value = c.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        var fileName = $"CurriculumVersions_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string? search = null, string sort = "id", string sortDir = "desc")
    {
        var (items, totalCount) = await _curriculumVersionService.GetCurriculumVersionsAsync(page, pageSize, search, sort, sortDir);

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

        var curriculumVersion = await _curriculumVersionService.GetCurriculumVersionByIdAsync(id.Value);
        if (curriculumVersion == null) return NotFound();

        var offerings = await _subjectOfferingService.GetSubjectOfferingsByCurriculumVersionAsync(id.Value);
        ViewBag.SubjectCount = offerings.Count;
        ViewBag.SubjectOfferings = offerings;

        return View(curriculumVersion);
    }

    [RequirePermission("subjectofferings.create")]
    public async Task<IActionResult> Manage(int? id)
    {
        if (id == null) return NotFound();

        var version = await _curriculumVersionService.GetCurriculumVersionByIdAsync(id.Value);
        if (version == null) return NotFound();

        await PopulateManageViewDataAsync(version);

        return View(new SubjectOfferingBulkCreateViewModel
        {
            ProgramId = version.ProgramId,
            AcademicYearId = version.EffectiveAcademicYearId,
            CurriculumVersionId = version.Id
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("subjectofferings.create")]
    public async Task<IActionResult> Manage(SubjectOfferingBulkCreateViewModel model)
    {
        var version = await _curriculumVersionService.GetCurriculumVersionByIdAsync(model.CurriculumVersionId);
        if (version == null) return NotFound();

        var groups = model.Semesters ?? new List<SemesterSubjectOfferingGroup>();
        var populated = groups.Where(g => g.Subjects is { Count: > 0 }).ToList();
        var removedIds = model.RemovedOfferingIds ?? new List<int>();

        if (populated.Count == 0 && removedIds.Count == 0)
            ModelState.AddModelError("", "No changes to save. Add or remove at least one subject.");

        for (var g = 0; g < groups.Count; g++)
        {
            var group = groups[g];
            if (group.Subjects == null || group.Subjects.Count == 0) continue;

            for (var i = 0; i < group.Subjects.Count; i++)
            {
                if (group.Subjects[i].SubjectCatalogId <= 0)
                    ModelState.AddModelError($"Semesters[{g}].Subjects[{i}].{nameof(SubjectOfferingItemViewModel.SubjectCatalogId)}", "Please select a valid subject.");
            }
        }

        if (populated.Count > 0)
        {
            var yearSemesters = await _subjectOfferingService.GetSemestersByAcademicYearAsync(version.EffectiveAcademicYearId, version.ProgramId);
            var yearIds = yearSemesters.Select(s => s.Id).ToHashSet();
            foreach (var group in populated)
            {
                if (!yearIds.Contains(group.SemesterId))
                    ModelState.AddModelError(nameof(model.AcademicYearId), $"Semester \"{group.SemesterName}\" does not belong to the selected academic year.");
            }
        }

        if (ModelState.IsValid && populated.Count > 0)
        {
            var existingBySemester = await _subjectOfferingService.GetExistingSubjectCatalogIdsBySemesterAsync(model.ProgramId, model.CurriculumVersionId, model.AcademicYearId);
            foreach (var group in populated)
            {
                if (existingBySemester.TryGetValue(group.SemesterId, out var existing))
                {
                    var duplicateCount = group.Subjects!.Count(s => existing.Contains(s.SubjectCatalogId));
                    if (duplicateCount > 0)
                        ModelState.AddModelError("", $"{duplicateCount} subject(s) already exist in semester \"{group.SemesterName}\" for this curriculum version.");
                }
            }
        }

        if (ModelState.IsValid)
        {
            var offerings = new List<SubjectOffering>();
            foreach (var group in populated)
            {
                offerings.AddRange(group.Subjects!.Select(s => new SubjectOffering
                {
                    SubjectCatalogId = s.SubjectCatalogId,
                    ProgramId = model.ProgramId,
                    SemesterId = group.SemesterId,
                    CurriculumVersionId = model.CurriculumVersionId,
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
                    InternalTheoryPassMarks = s.InternalTheoryPassMarks
                }));
            }

            try
            {
                foreach (var group in populated)
                    await _subjectOfferingService.EnsureSemesterAssignedToProgramAsync(model.ProgramId, group.SemesterId);

                if (offerings.Count > 0)
                    await _subjectOfferingService.CreateSubjectOfferingsAsync(offerings);

                var removed = 0;
                if (removedIds.Count > 0)
                {
                    var toRemove = await _subjectOfferingService.GetSubjectOfferingsForDeletionAsync(removedIds);
                    foreach (var o in toRemove.Where(o => o.CurriculumVersionId == model.CurriculumVersionId).ToList())
                    {
                        await _subjectOfferingService.DeleteSubjectOfferingAsync(o.Id);
                        removed++;
                    }
                }

                TempData["SuccessMessage"] = $"{offerings.Count} subject(s) added, {removed} removed.";
                return RedirectToAction(nameof(Details), new { id = model.CurriculumVersionId });
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "A database error occurred. Some subjects may already exist or are referenced by exam schedules.");
                await PopulateManageViewDataOnErrorAsync(model);
                return View(model);
            }
        }

        await PopulateManageViewDataOnErrorAsync(model);
        return View(model);
    }

    private async Task PopulateManageViewDataAsync(CurriculumVersion version)
    {
        var subjectCatalogs = (await _subjectOfferingService.GetSelectListsAsync()).SubjectCatalogs;
        var semesters = await _subjectOfferingService.GetSemestersForOfferingAsync(version.ProgramId, version.EffectiveAcademicYearId);
        var offerings = await _subjectOfferingService.GetSubjectOfferingsByCurriculumVersionAsync(version.Id);

        ViewBag.VersionName = version.Name;
        ViewBag.ProgramName = version.Program?.ProgramName ?? "-";
        ViewBag.AcademicYearName = version.EffectiveAcademicYear?.AcademicYearName ?? "-";

        var semesterJson = new List<object>();
        var existingCatalogIds = new Dictionary<int, List<int>>();
        foreach (var s in semesters)
        {
            var semOfferings = offerings.Where(o => o.SemesterId == s.SemesterId).ToList();
            existingCatalogIds[s.SemesterId] = semOfferings.Select(o => o.SubjectCatalogId).ToList();
            semesterJson.Add(new
            {
                semesterId = s.SemesterId,
                semesterName = s.SemesterName,
                existing = semOfferings.Select(o => new
                {
                    offeringId = o.Id,
                    id = o.SubjectCatalogId,
                    code = o.SubjectCatalog?.SubjectCode ?? "",
                    name = o.SubjectCatalog?.SubjectName ?? ""
                }).ToList(),
                subjects = Array.Empty<object>()
            });
        }

        ViewBag.InitialSemestersJson = JsonSerializer.Serialize(semesterJson);
        ViewBag.InitialExistingJson = JsonSerializer.Serialize(existingCatalogIds);
        ViewBag.SubjectCatalogsJson = JsonSerializer.Serialize(subjectCatalogs.Select(s => new
        {
            id = s.Id,
            code = s.SubjectCode,
            name = s.SubjectName,
            type = s.SubjectType?.Name ?? "",
            credits = s.CreditHours
        }));
    }

    private async Task PopulateManageViewDataOnErrorAsync(SubjectOfferingBulkCreateViewModel model)
    {
        var version = await _curriculumVersionService.GetCurriculumVersionByIdAsync(model.CurriculumVersionId);
        if (version == null) return;

        await PopulateManageViewDataAsync(version);

        var catalogMap = (await _subjectOfferingService.GetSelectListsAsync()).SubjectCatalogs.ToDictionary(s => s.Id);
        var offerings = await _subjectOfferingService.GetSubjectOfferingsByCurriculumVersionAsync(model.CurriculumVersionId);
        var bySemester = offerings.GroupBy(o => o.SemesterId).ToDictionary(g => g.Key, g => g.ToList());

        var semesterJson = new List<object>();
        var existingCatalogIds = new Dictionary<int, List<int>>();
        foreach (var group in model.Semesters ?? new List<SemesterSubjectOfferingGroup>())
        {
            var existing = bySemester.TryGetValue(group.SemesterId, out var ex) ? ex : new List<SubjectOffering>();
            existingCatalogIds[group.SemesterId] = existing.Select(o => o.SubjectCatalogId).ToList();
            semesterJson.Add(new
            {
                semesterId = group.SemesterId,
                semesterName = group.SemesterName ?? "",
                existing = existing.Select(o => new
                {
                    offeringId = o.Id,
                    id = o.SubjectCatalogId,
                    code = o.SubjectCatalog?.SubjectCode ?? "",
                    name = o.SubjectCatalog?.SubjectName ?? ""
                }).ToList(),
                subjects = group.Subjects?.Select(s =>
                {
                    catalogMap.TryGetValue(s.SubjectCatalogId, out var cat);
                    return new
                    {
                        id = s.SubjectCatalogId,
                        code = cat?.SubjectCode ?? "",
                        name = cat?.SubjectName ?? "",
                        isCompulsory = s.IsCompulsory,
                        displayOrder = s.DisplayOrder,
                        hasTheory = s.HasTheory,
                        hasPractical = s.HasPractical,
                        hasInternal = s.HasInternal,
                        theoryFullMarks = s.TheoryFullMarks,
                        theoryPassMarks = s.TheoryPassMarks,
                        practicalFullMarks = s.PracticalFullMarks,
                        practicalPassMarks = s.PracticalPassMarks,
                        internalTheoryFullMarks = s.InternalTheoryFullMarks,
                        internalTheoryPassMarks = s.InternalTheoryPassMarks
                    };
                }).ToList()
            });
        }

        ViewBag.InitialSemestersJson = JsonSerializer.Serialize(semesterJson);
        ViewBag.InitialExistingJson = JsonSerializer.Serialize(existingCatalogIds);
    }

    [HttpGet]
    [RequirePermission("curriculumversions.create")]
    public async Task<IActionResult> Copy(int? id)
    {
        if (id == null) return NotFound();

        var curriculumVersion = await _curriculumVersionService.GetCurriculumVersionByIdAsync(id.Value);
        if (curriculumVersion == null) return NotFound();

        var (programs, academicYears) = await _curriculumVersionService.GetSelectListsAsync();
        ViewData["EffectiveAcademicYearId"] = new SelectList(academicYears, "Id", "AcademicYearName");
        ViewData["SourceName"] = curriculumVersion.Name;
        ViewData["SourceProgram"] = curriculumVersion.Program?.ProgramName ?? "-";

        return View(curriculumVersion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("curriculumversions.create")]
    public async Task<IActionResult> Copy(int id, int effectiveAcademicYearId, string name)
    {
        if (id <= 0 || effectiveAcademicYearId <= 0 || string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError("", "Please provide a version name and target academic year.");
            var curriculumVersion = await _curriculumVersionService.GetCurriculumVersionByIdAsync(id);
            if (curriculumVersion == null) return NotFound();
            var (programs, academicYears) = await _curriculumVersionService.GetSelectListsAsync();
            ViewData["EffectiveAcademicYearId"] = new SelectList(academicYears, "Id", "AcademicYearName", effectiveAcademicYearId);
            ViewData["SourceName"] = curriculumVersion.Name;
            ViewData["SourceProgram"] = curriculumVersion.Program?.ProgramName ?? "-";
            return View(curriculumVersion);
        }

        var newVersion = await _curriculumVersionService.CopyCurriculumVersionAsync(id, effectiveAcademicYearId, name.Trim());
        if (newVersion == null)
        {
            TempData["ErrorMessage"] = "Copy failed. Source curriculum version not found.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = $"Curriculum version \"{newVersion.Name}\" copied successfully.";
        return RedirectToAction(nameof(Details), new { id = newVersion.Id });
    }

    [RequirePermission("curriculumversions.create")]
    public async Task<IActionResult> Create()
    {
        var (programs, academicYears) = await _curriculumVersionService.GetSelectListsAsync();
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName");
        ViewData["EffectiveAcademicYearId"] = new SelectList(academicYears, "Id", "AcademicYearName");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("curriculumversions.create")]
    public async Task<IActionResult> Create([Bind("Id,Name,ProgramId,EffectiveAcademicYearId,Description,IsActive")] CurriculumVersion curriculumVersion)
    {
        if (ModelState.IsValid)
        {
            await _curriculumVersionService.CreateCurriculumVersionAsync(curriculumVersion);
            TempData["SuccessMessage"] = "Curriculum version created successfully!";
            return RedirectToAction(nameof(Index));
        }
        var (programs, academicYears) = await _curriculumVersionService.GetSelectListsAsync(curriculumVersion.ProgramId, curriculumVersion.EffectiveAcademicYearId);
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", curriculumVersion.ProgramId);
        ViewData["EffectiveAcademicYearId"] = new SelectList(academicYears, "Id", "AcademicYearName", curriculumVersion.EffectiveAcademicYearId);
        return View(curriculumVersion);
    }

    [RequirePermission("curriculumversions.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var curriculumVersion = await _curriculumVersionService.GetCurriculumVersionByIdAsync(id.Value);
        if (curriculumVersion == null) return NotFound();

        var (programs, academicYears) = await _curriculumVersionService.GetSelectListsAsync(curriculumVersion.ProgramId, curriculumVersion.EffectiveAcademicYearId);
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", curriculumVersion.ProgramId);
        ViewData["EffectiveAcademicYearId"] = new SelectList(academicYears, "Id", "AcademicYearName", curriculumVersion.EffectiveAcademicYearId);
        return View(curriculumVersion);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("curriculumversions.edit")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ProgramId,EffectiveAcademicYearId,Description,IsActive")] CurriculumVersion curriculumVersion)
    {
        if (id != curriculumVersion.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await _curriculumVersionService.UpdateCurriculumVersionAsync(curriculumVersion);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _curriculumVersionService.CurriculumVersionExistsAsync(curriculumVersion.Id))
                    return NotFound();
                throw;
            }
            TempData["SuccessMessage"] = "Curriculum version updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        var (programs, academicYears) = await _curriculumVersionService.GetSelectListsAsync(curriculumVersion.ProgramId, curriculumVersion.EffectiveAcademicYearId);
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", curriculumVersion.ProgramId);
        ViewData["EffectiveAcademicYearId"] = new SelectList(academicYears, "Id", "AcademicYearName", curriculumVersion.EffectiveAcademicYearId);
        return View(curriculumVersion);
    }

    [RequirePermission("curriculumversions.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var curriculumVersion = await _curriculumVersionService.GetCurriculumVersionByIdAsync(id.Value);
        if (curriculumVersion == null) return NotFound();

        return View(curriculumVersion);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [RequirePermission("curriculumversions.delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var (deleted, skipped) = await _curriculumVersionService.DeleteCurriculumVersionAsync(id);
            if (deleted)
                TempData["SuccessMessage"] = "Curriculum version deleted successfully!";
            else
                TempData["ErrorMessage"] = $"{skipped} subject offering(s) are still referenced by exam slots, exam results, or admin assignments and were not removed. The curriculum version was kept.";
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
        [RequirePermission("curriculumversions.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try
        {
            var (deleted, skipped) = await _curriculumVersionService.DeleteCurriculumVersionAsync(id);
            return Json(deleted
                ? new { success = true, message = "Curriculum version deleted successfully!" }
                : new { success = false, message = $"{skipped} subject offering(s) are still referenced by exam slots, exam results, or admin assignments and were not removed. The curriculum version was kept." });
        }
        catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
