using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Helpers;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Domain.Extensions;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Subjects.Controllers;

[Area("Subjects")]
[RequirePermission("subjectofferings.view")]
public class SubjectOfferingsController : Controller
{
    private readonly ISubjectOfferingService _subjectOfferingService;

    public SubjectOfferingsController(ISubjectOfferingService subjectOfferingService)
    {
        _subjectOfferingService = subjectOfferingService;
    }

    private static SelectList SemesterSelectList(IEnumerable<Semester> semesters, int? selectedId = null)
    {
        return new SelectList(
            semesters.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = SemesterDisplayHelper.Format(s)
            }),
            "Value", "Text", selectedId?.ToString());
    }

    public async Task<IActionResult> Index(int? curriculumVersionId, int? programId, int? semesterId)
    {
        var (_, programs, _) = await _subjectOfferingService.GetSelectListsAsync();
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", programId);

        if (programId is > 0)
        {
            var semesters = await _subjectOfferingService.GetSemestersByProgramAsync(programId.Value, curriculumVersionId ?? 0);
            ViewData["SemesterId"] = new SelectList(semesters.Select(s => new { s.SemesterId, s.SemesterName }), "SemesterId", "SemesterName", semesterId);
        }
        else
        {
            ViewData["SemesterId"] = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
        }

        if (programId is > 0)
        {
            var versions = await _subjectOfferingService.GetCurriculumVersionsAsync(programId);
            ViewData["CurriculumVersionId"] = new SelectList(versions, "Id", "Name", curriculumVersionId);
        }
        else
        {
            ViewData["CurriculumVersionId"] = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
        }

        ViewBag.InitialProgramId = programId;
        ViewBag.InitialSemesterId = semesterId;
        ViewBag.InitialCurriculumVersionId = curriculumVersionId;
        ViewBag.CurrentProgramId = programId;
        ViewBag.CurrentSemesterId = semesterId;
        ViewBag.CurrentCurriculumVersionId = curriculumVersionId;

        var searched = curriculumVersionId is > 0 || programId is > 0 || semesterId is > 0;
        ViewBag.Searched = searched;

        var items = searched
            ? await _subjectOfferingService.GetSearchResultsAsync(curriculumVersionId, programId, semesterId)
            : new List<SubjectOffering>();
        ViewBag.ResultCount = items.Count;

        return View(items);
    }


    public async Task<IActionResult> ExportToCsv(int? curriculumVersionId, int? programId, int? semesterId)
    {
        var items = await _subjectOfferingService.GetSearchResultsAsync(curriculumVersionId, programId, semesterId);

        var sb = new StringBuilder();
        sb.AppendLine("Subject,Program,Semester,Compulsory,Theory Marks,Practical Marks,Internal Theory Marks");

        foreach (var s in items)
        {
            sb.AppendLine($"{(s.SubjectCatalog?.SubjectName ?? "-").EscapeCsv()}," +
                           $"{(s.Program?.ProgramName ?? "-").EscapeCsv()}," +
                           $"{(s.Semester?.Name ?? "-").EscapeCsv()}," +
                           $"{(s.IsCompulsory ? "Yes" : "No")}," +
                           $"{s.TheoryFullMarks}," +
                           $"{s.PracticalFullMarks ?? 0}," +
                           $"{s.InternalTheoryFullMarks ?? 0}");
        }

        var fileName = $"SubjectOfferings_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int? curriculumVersionId, int? programId, int? semesterId)
    {
        var items = await _subjectOfferingService.GetSearchResultsAsync(curriculumVersionId, programId, semesterId);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Subject Offerings");

        var headers = new[] { "Subject", "Program", "Semester", "Compulsory", "Theory Marks", "Practical Marks", "Internal Theory Marks" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var s in items)
        {
            worksheet.Cell(row, 1).Value = s.SubjectCatalog?.SubjectName ?? "-";
            worksheet.Cell(row, 2).Value = s.Program?.ProgramName ?? "-";
            worksheet.Cell(row, 3).Value = s.Semester?.Name ?? "-";
            worksheet.Cell(row, 4).Value = s.IsCompulsory ? "Yes" : "No";
            worksheet.Cell(row, 5).Value = s.TheoryFullMarks;
            worksheet.Cell(row, 6).Value = s.PracticalFullMarks ?? 0;
            worksheet.Cell(row, 7).Value = s.InternalTheoryFullMarks ?? 0;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        var fileName = $"SubjectOfferings_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int? curriculumVersionId, int? programId, int? semesterId)
    {
        var items = await _subjectOfferingService.GetSearchResultsAsync(curriculumVersionId, programId, semesterId);

        ViewBag.CurrentPage = 1;
        ViewBag.PageSize = items.Count;
        ViewBag.TotalCount = items.Count;
        ViewBag.Search = null;

        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<JsonResult> GetExistingSubjects(int programId, int semesterId, int? curriculumVersionId = null)
    {
        var ids = await _subjectOfferingService.GetExistingSubjectCatalogIdsAsync(programId, semesterId, curriculumVersionId);
        return Json(ids);
    }

    [HttpGet]
    public async Task<JsonResult> GetExistingSubjectsBySemesters(int programId, int curriculumVersionId)
    {
        var result = await _subjectOfferingService.GetExistingSubjectCatalogIdsBySemesterAsync(programId, curriculumVersionId);
        return Json(result);
    }

    [HttpGet]
    public async Task<JsonResult> GetCurriculumVersionsByProgram(int programId)
    {
        var versions = await _subjectOfferingService.GetCurriculumVersionsAsync(programId);
        return Json(versions.Select(v => new { id = v.Id, name = v.Name }));
    }

    [HttpGet]
    public async Task<JsonResult> GetSemestersByCurriculumVersion(int curriculumVersionId, int? programId = null)
    {
        var semesters = await _subjectOfferingService.GetSemestersByCurriculumVersionAsync(curriculumVersionId, programId);
        return Json(semesters.Select(s => new { id = s.Id, name = s.Name }));
    }

    [HttpGet]
    public async Task<JsonResult> GetSemestersByProgram(int programId, int? curriculumVersionId = null)
    {
        var semesters = await _subjectOfferingService.GetSemestersByProgramAsync(programId, curriculumVersionId ?? 0);
        return Json(semesters.Select(s => new { s.SemesterId, s.SemesterNumber, s.SemesterName, s.SubjectCount }));
    }

    [HttpGet]
    public async Task<JsonResult> GetSemestersForCreate(int programId, int curriculumVersionId)
    {
        var semesters = await _subjectOfferingService.GetSemestersForOfferingAsync(programId, curriculumVersionId);
        return Json(semesters.Select(s => new { s.SemesterId, s.SemesterNumber, s.SemesterName, s.SubjectCount }));
    }

    public async Task<IActionResult> Create(int? curriculumVersionId)
    {
        var (subjectCatalogs, programs, _) = await _subjectOfferingService.GetSelectListsAsync();

        var version = curriculumVersionId is > 0
            ? await _subjectOfferingService.GetCurriculumVersionByIdAsync(curriculumVersionId.Value)
            : null;

        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", version?.ProgramId);
        ViewData["CurriculumVersionId"] = version != null
            ? new SelectList(await _subjectOfferingService.GetCurriculumVersionsAsync(version.ProgramId), "Id", "Name", version.Id)
            : new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");

        var semesterJson = new List<object>();
        var initialExisting = new Dictionary<int, List<int>>();
        if (version != null)
        {
            var yearSemesters = await _subjectOfferingService.GetSemestersForOfferingAsync(version.ProgramId, version.Id);
            foreach (var s in yearSemesters)
            {
                semesterJson.Add(new { semesterId = s.SemesterId, semesterName = s.SemesterName, subjects = Array.Empty<object>() });
            }
            initialExisting = await _subjectOfferingService.GetExistingSubjectCatalogIdsBySemesterAsync(version.ProgramId, version.Id);
        }

        ViewBag.InitialSemestersJson = JsonSerializer.Serialize(semesterJson);
        ViewBag.InitialExistingJson = JsonSerializer.Serialize(initialExisting);

        ViewBag.SubjectCatalogsJson = JsonSerializer.Serialize(subjectCatalogs.Select(s => new
        {
            id = s.Id,
            code = s.SubjectCode,
            name = s.SubjectName,
            type = s.SubjectType?.Name ?? "",
            credits = s.CreditHours
        }));

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("subjectofferings.create")]
    public async Task<IActionResult> Create(SubjectOfferingBulkCreateViewModel model)
    {
        if (model.ProgramId <= 0)
            ModelState.AddModelError(nameof(model.ProgramId), "Program is required.");

        if (model.CurriculumVersionId <= 0)
            ModelState.AddModelError(nameof(model.CurriculumVersionId), "Curriculum Version is required.");

        var groups = model.Semesters ?? new List<SemesterSubjectOfferingGroup>();
        var populated = groups.Where(g => g.Subjects is { Count: > 0 }).ToList();

        if (populated.Count == 0)
            ModelState.AddModelError("", "Please add at least one subject to at least one semester.");

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

        if (model.ProgramId > 0 && model.CurriculumVersionId > 0)
        {
            if (!await _subjectOfferingService.IsCurriculumVersionForProgramAsync(model.CurriculumVersionId, model.ProgramId))
                ModelState.AddModelError(nameof(model.CurriculumVersionId), "The selected curriculum version does not belong to the selected program.");
        }

        if (model.CurriculumVersionId > 0 && model.ProgramId > 0)
        {
            var cvSemesters = await _subjectOfferingService.GetSemestersByCurriculumVersionAsync(model.CurriculumVersionId, model.ProgramId);
            var cvSemesterIds = cvSemesters.Select(s => s.Id).ToHashSet();
            foreach (var group in populated)
            {
                if (!cvSemesterIds.Contains(group.SemesterId))
                    ModelState.AddModelError(nameof(model.CurriculumVersionId), $"Semester \"{group.SemesterName}\" does not belong to the selected curriculum version's academic year.");
            }
        }

        if (ModelState.IsValid)
        {
            var existingBySemester = await _subjectOfferingService.GetExistingSubjectCatalogIdsBySemesterAsync(model.ProgramId, model.CurriculumVersionId);
            foreach (var group in populated)
            {
                if (existingBySemester.TryGetValue(group.SemesterId, out var existing))
                {
                    var duplicateCount = group.Subjects!.Count(s => existing.Contains(s.SubjectCatalogId));
                    if (duplicateCount > 0)
                        ModelState.AddModelError("", $"{duplicateCount} subject(s) already exist in semester \"{group.SemesterName}\" for the selected program and curriculum version.");
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

                await _subjectOfferingService.CreateSubjectOfferingsAsync(offerings);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "A database error occurred. One or more subject offerings may already exist.");
                await PopulateCreateViewDataOnErrorAsync(model);
                return View(model);
            }
            TempData["SuccessMessage"] = $"{offerings.Count} subject offering(s) created successfully.";
            return RedirectToAction(nameof(Index));
        }

        await PopulateCreateViewDataOnErrorAsync(model);
        return View(model);
    }

    private async Task PopulateCreateViewDataOnErrorAsync(SubjectOfferingBulkCreateViewModel model)
    {
        var (subjectCatalogs, programs, _) = await _subjectOfferingService.GetSelectListsAsync();
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", model.ProgramId);
        ViewData["CurriculumVersionId"] = model.ProgramId > 0
            ? new SelectList(await _subjectOfferingService.GetCurriculumVersionsAsync(model.ProgramId), "Id", "Name", model.CurriculumVersionId)
            : new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");

        ViewBag.SubjectCatalogsJson = JsonSerializer.Serialize(subjectCatalogs.Select(s => new
        {
            id = s.Id,
            code = s.SubjectCode,
            name = s.SubjectName,
            type = s.SubjectType?.Name ?? "",
            credits = s.CreditHours
        }));

        var catalogMap = subjectCatalogs.ToDictionary(s => s.Id);
        var semesterJson = new List<object>();
        foreach (var group in model.Semesters ?? new List<SemesterSubjectOfferingGroup>())
        {
            semesterJson.Add(new
            {
                semesterId = group.SemesterId,
                semesterName = group.SemesterName ?? "",
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

        ViewBag.InitialExistingJson = "{}";
        if (model.ProgramId > 0 && model.CurriculumVersionId > 0)
        {
            var existing = await _subjectOfferingService.GetExistingSubjectCatalogIdsBySemesterAsync(model.ProgramId, model.CurriculumVersionId);
            ViewBag.InitialExistingJson = JsonSerializer.Serialize(existing);
        }
    }

    [RequirePermission("subjectofferings.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var subjectOffering = await _subjectOfferingService.GetSubjectOfferingByIdAsync(id.Value);
        if (subjectOffering == null) return NotFound();

        var (subjectCatalogs, programs, semesters) = await _subjectOfferingService.GetSelectListsAsync(subjectOffering.SubjectCatalogId, subjectOffering.ProgramId, subjectOffering.SemesterId);
        ViewData["SubjectCatalogId"] = new SelectList(subjectCatalogs, "Id", "SubjectName", subjectOffering.SubjectCatalogId);
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", subjectOffering.ProgramId);
        ViewData["SemesterId"] = SemesterSelectList(semesters, subjectOffering.SemesterId);
        ViewData["CurriculumVersionId"] = new SelectList(await _subjectOfferingService.GetCurriculumVersionsAsync(subjectOffering.ProgramId), "Id", "Name", subjectOffering.CurriculumVersionId);
        return View(subjectOffering);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,TenantId,SubjectCatalogId,ProgramId,SemesterId,CurriculumVersionId,IsActive,IsCompulsory,DisplayOrder,HasTheory,HasPractical,HasInternal,TheoryFullMarks,TheoryPassMarks,PracticalFullMarks,PracticalPassMarks,InternalTheoryFullMarks,InternalTheoryPassMarks")] SubjectOffering subjectOffering)
    {
        if (id != subjectOffering.Id) return NotFound();

        var current = await _subjectOfferingService.GetSubjectOfferingByIdAsync(id);
        if (current == null) return NotFound();

        var subjectChanged = current.SubjectCatalogId != subjectOffering.SubjectCatalogId
                          || current.ProgramId != subjectOffering.ProgramId
                          || current.SemesterId != subjectOffering.SemesterId
                          || current.CurriculumVersionId != subjectOffering.CurriculumVersionId;

        var marksChanged = current.HasTheory != subjectOffering.HasTheory
                        || current.HasPractical != subjectOffering.HasPractical
                        || current.HasInternal != subjectOffering.HasInternal
                        || current.TheoryFullMarks != subjectOffering.TheoryFullMarks
                        || current.TheoryPassMarks != subjectOffering.TheoryPassMarks
                        || current.PracticalFullMarks != subjectOffering.PracticalFullMarks
                        || current.PracticalPassMarks != subjectOffering.PracticalPassMarks
                        || current.InternalTheoryFullMarks != subjectOffering.InternalTheoryFullMarks
                        || current.InternalTheoryPassMarks != subjectOffering.InternalTheoryPassMarks;

        if (marksChanged && await _subjectOfferingService.IsSubjectOfferingReferencedAsync(id))
        {
            ModelState.AddModelError(string.Empty,
                "The evaluation scheme (marks) cannot be edited once the subject has been used in exams or assigned to college admins. Other fields can still be updated.");
        }

        if (subjectOffering.ProgramId > 0 && subjectOffering.SemesterId > 0)
        {
            if (!await _subjectOfferingService.IsSemesterAssignedToProgramAsync(subjectOffering.ProgramId, subjectOffering.SemesterId))
                ModelState.AddModelError(nameof(subjectOffering.SemesterId), "The selected semester is not assigned to the selected program. Assign it first via Programs \u2192 Semesters.");
        }

        if (subjectOffering.ProgramId > 0 && subjectOffering.CurriculumVersionId is > 0)
        {
            if (!await _subjectOfferingService.IsCurriculumVersionForProgramAsync(subjectOffering.CurriculumVersionId.Value, subjectOffering.ProgramId))
                ModelState.AddModelError(nameof(subjectOffering.CurriculumVersionId), "The selected curriculum version does not belong to the selected program.");
        }

        if (ModelState.IsValid && subjectChanged)
        {
            var existingIds = await _subjectOfferingService.GetExistingSubjectCatalogIdsAsync(subjectOffering.ProgramId, subjectOffering.SemesterId, subjectOffering.CurriculumVersionId);
            if (existingIds.Contains(subjectOffering.SubjectCatalogId))
            {
                ModelState.AddModelError(nameof(subjectOffering.SubjectCatalogId), "This subject already exists in this semester for the selected program and curriculum version.");
            }
        }

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
            TempData["SuccessMessage"] = "Subject offering updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        var (subjectCatalogs, programs, semesters) = await _subjectOfferingService.GetSelectListsAsync(subjectOffering.SubjectCatalogId, subjectOffering.ProgramId, subjectOffering.SemesterId);
        ViewData["SubjectCatalogId"] = new SelectList(subjectCatalogs, "Id", "SubjectName", subjectOffering.SubjectCatalogId);
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", subjectOffering.ProgramId);
        ViewData["SemesterId"] = SemesterSelectList(semesters, subjectOffering.SemesterId);
        ViewData["CurriculumVersionId"] = new SelectList(await _subjectOfferingService.GetCurriculumVersionsAsync(subjectOffering.ProgramId), "Id", "Name", subjectOffering.CurriculumVersionId);
        return View(subjectOffering);
    }

    [RequirePermission("subjectofferings.delete")]
    public async Task<IActionResult> Archive(int id)
    {
        try
        {
            await _subjectOfferingService.ArchiveSubjectOfferingAsync(id);
            TempData["SuccessMessage"] = "Subject offering archived successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while archiving: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }

    [RequirePermission("subjectofferings.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ArchiveAjax(int id)
    {
        try
        {
            await _subjectOfferingService.ArchiveSubjectOfferingAsync(id);
            return Json(new { success = true, message = "Subject offering archived successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

}
