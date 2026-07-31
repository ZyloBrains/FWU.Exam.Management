using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Interfaces;
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

    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Program", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalProgramCount) = await _subjectOfferingService.GetSubjectOfferingsAsync(page, pageSize, search, sort, sortDir);

        ViewBag.TotalCount = totalProgramCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalProgramCount / pageSize);
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

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string search = null, string sort = "Subject", string sortDir = "asc")
    {
        var items = await _subjectOfferingService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Subject Offerings");

        var headers = new[] { "Subject", "Program", "Semester", "Compulsory", "Theory Marks", "Practical Marks", "Internal Marks" };
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
            worksheet.Cell(row, 7).Value = (s.InternalTheoryFullMarks ?? 0) + (s.InternalPracticalFullMarks ?? 0);
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        var fileName = $"SubjectOfferings_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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

    [HttpGet]
    public async Task<JsonResult> GetExistingSubjects(int programId, int semesterId)
    {
        var ids = await _subjectOfferingService.GetExistingSubjectCatalogIdsAsync(programId, semesterId);
        return Json(ids);
    }

    [HttpGet]
    public async Task<JsonResult> GetSemestersByAcademicYear(int academicYearId)
    {
        var semesters = await _subjectOfferingService.GetSemestersByAcademicYearAsync(academicYearId);
        return Json(semesters.Select(s => new { id = s.Id, name = s.Name }));
    }

    public async Task<IActionResult> Create()
    {
        var (subjectCatalogs, programs, semesters) = await _subjectOfferingService.GetSelectListsAsync();
        ViewData["AcademicYearId"] = new SelectList(await _subjectOfferingService.GetAcademicYearsAsync(), "Id", "Name");
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
    [RequirePermission("subjectofferings.create")]
    public async Task<IActionResult> Create(SubjectOfferingBulkCreateViewModel model)
    {
        if (model.ProgramId <= 0)
            ModelState.AddModelError(nameof(model.ProgramId), "Program is required.");

        if (model.AcademicYearId <= 0)
            ModelState.AddModelError(nameof(model.AcademicYearId), "Academic Year is required.");

        if (model.SemesterId <= 0)
            ModelState.AddModelError(nameof(model.SemesterId), "Semester is required.");

        if (model.Subjects == null || model.Subjects.Count == 0)
            ModelState.AddModelError("", "Please add at least one subject.");
        else
        {
            for (var i = 0; i < model.Subjects.Count; i++)
            {
                if (model.Subjects[i].SubjectCatalogId <= 0)
                    ModelState.AddModelError($"Subjects[{i}].{nameof(SubjectOfferingItemViewModel.SubjectCatalogId)}", "Please select a valid subject.");
            }
        }

        if (model.AcademicYearId > 0 && model.SemesterId > 0)
        {
            var semester = await _subjectOfferingService.GetSemestersByAcademicYearAsync(model.AcademicYearId);
            if (!semester.Any(s => s.Id == model.SemesterId))
                ModelState.AddModelError(nameof(model.SemesterId), "The selected semester does not belong to the selected academic year.");
        }

        if (ModelState.IsValid)
        {
            var existingIds = await _subjectOfferingService.GetExistingSubjectCatalogIdsAsync(model.ProgramId, model.SemesterId);
            var duplicateIds = model.Subjects
                .Where(s => existingIds.Contains(s.SubjectCatalogId))
                .Select(s => s.SubjectCatalogId)
                .ToList();

            if (duplicateIds.Any())
            {
                ModelState.AddModelError("", $"{duplicateIds.Count} subject(s) already exist in this semester for the selected program.");
            }
        }

        if (ModelState.IsValid)
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

            try
            {
                await _subjectOfferingService.CreateSubjectOfferingsAsync(offerings);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "A database error occurred. The subject offering may already exist.");
                var (cats, progs, sems) = await _subjectOfferingService.GetSelectListsAsync();
                ViewData["AcademicYearId"] = new SelectList(await _subjectOfferingService.GetAcademicYearsAsync(), "Id", "Name", model.AcademicYearId);
                ViewData["ProgramId"] = new SelectList(progs, "Id", "ProgramName", model.ProgramId);
                ViewData["SemesterId"] = new SelectList(sems, "Id", "Name", model.SemesterId);
                ViewBag.SubjectCatalogsJson = JsonSerializer.Serialize(cats.Select(s => new
                {
                    id = s.Id,
                    code = s.SubjectCode,
                    name = s.SubjectName,
                    type = s.SubjectType?.Name ?? "",
                    credits = s.CreditHours
                }));
                return View(model);
            }
            TempData["SuccessMessage"] = $"{offerings.Count} subject offering(s) created successfully.";
            return RedirectToAction(nameof(Index));
        }

        var (subjectCatalogs, programs, semesters) = await _subjectOfferingService.GetSelectListsAsync();
        ViewData["AcademicYearId"] = new SelectList(await _subjectOfferingService.GetAcademicYearsAsync(), "Id", "Name", model.AcademicYearId);
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

    [RequirePermission("subjectofferings.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var subjectOffering = await _subjectOfferingService.GetSubjectOfferingByIdAsync(id.Value);
        if (subjectOffering == null) return NotFound();

        var (subjectCatalogs, programs, semesters) = await _subjectOfferingService.GetSelectListsAsync(subjectOffering.SubjectCatalogId, subjectOffering.ProgramId, subjectOffering.SemesterId);
        ViewData["SubjectCatalogId"] = new SelectList(subjectCatalogs, "Id", "SubjectName", subjectOffering.SubjectCatalogId);
        ViewData["AcademicYearId"] = new SelectList(await _subjectOfferingService.GetAcademicYearsAsync(), "Id", "Name", subjectOffering.Semester?.AcademicYearId);
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", subjectOffering.ProgramId);
        ViewData["SemesterId"] = new SelectList(semesters, "Id", "Name", subjectOffering.SemesterId);
        return View(subjectOffering);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,TenantId,SubjectCatalogId,ProgramId,SemesterId,IsCompulsory,DisplayOrder,HasTheory,HasPractical,HasInternal,TheoryFullMarks,TheoryPassMarks,PracticalFullMarks,PracticalPassMarks,InternalTheoryFullMarks,InternalTheoryPassMarks,InternalPracticalFullMarks,InternalPracticalPassMarks")] SubjectOffering subjectOffering, int academicYearId)
    {
        if (id != subjectOffering.Id) return NotFound();

        var current = await _subjectOfferingService.GetSubjectOfferingByIdAsync(id);
        if (current == null) return NotFound();

        var subjectChanged = current.SubjectCatalogId != subjectOffering.SubjectCatalogId
                          || current.ProgramId != subjectOffering.ProgramId
                          || current.SemesterId != subjectOffering.SemesterId;

        if (academicYearId > 0 && subjectOffering.SemesterId > 0)
        {
            var yearSemesters = await _subjectOfferingService.GetSemestersByAcademicYearAsync(academicYearId);
            if (!yearSemesters.Any(s => s.Id == subjectOffering.SemesterId))
                ModelState.AddModelError(nameof(subjectOffering.SemesterId), "The selected semester does not belong to the selected academic year.");
        }

        if (ModelState.IsValid && subjectChanged)
        {
            var existingIds = await _subjectOfferingService.GetExistingSubjectCatalogIdsAsync(subjectOffering.ProgramId, subjectOffering.SemesterId);
            if (existingIds.Contains(subjectOffering.SubjectCatalogId))
            {
                ModelState.AddModelError(nameof(subjectOffering.SubjectCatalogId), "This subject already exists in this semester for the selected program.");
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
        ViewData["AcademicYearId"] = new SelectList(await _subjectOfferingService.GetAcademicYearsAsync(), "Id", "Name", academicYearId);
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", subjectOffering.ProgramId);
        ViewData["SemesterId"] = new SelectList(semesters, "Id", "Name", subjectOffering.SemesterId);
        return View(subjectOffering);
    }

    [RequirePermission("subjectofferings.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var subjectOffering = await _subjectOfferingService.GetSubjectOfferingByIdAsync(id.Value);
        if (subjectOffering == null) return NotFound();

        return View(subjectOffering);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [RequirePermission("subjectofferings.delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _subjectOfferingService.DeleteSubjectOfferingAsync(id);
            TempData["SuccessMessage"] = "Subject offering deleted successfully!";
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
        [RequirePermission("subjectofferings.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await _subjectOfferingService.DeleteSubjectOfferingAsync(id); return Json(new { success = true, message = "Subject offering deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
