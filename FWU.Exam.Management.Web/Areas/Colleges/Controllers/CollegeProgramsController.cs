using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Colleges.Controllers;

[Area("Colleges")]
[RequirePermission("collegeprograms.view")]
public class CollegeProgramsController(ICollegeProgramService collegeProgramService) : Controller
{
    public async Task<IActionResult> Index(string search = null, string sort = "collegename", string sortDir = "asc")
    {
        var (items, totalCount) = await collegeProgramService.GetCollegeProgramsAsync(1, int.MaxValue, search, sort, sortDir);

        ViewBag.TotalCount = totalCount;
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

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "Id", string sortDir = "asc")
    {
        var (items, totalCount) = await collegeProgramService.GetFilteredItemsForExportAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("College Code,College Name,Program Code,Program Name,Affiliation Date,Number of Students,Remarks,Status");

        foreach (var cp in items)
        {
            sb.AppendLine($"{EscapeCsv(cp.College?.Code.ToString())}," +
                          $"{EscapeCsv(cp.College?.Name)}," +
                          $"{EscapeCsv(cp.Program?.ProgramCode)}," +
                          $"{EscapeCsv(cp.Program?.ProgramName)}," +
                          $"{cp.AffiliationDate?.ToString("yyyy-MM-dd")}," +
                          $"{cp.NumberOfStudents}," +
                          $"{EscapeCsv(cp.Remarks)}," +
                          $"{(cp.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"CollegePrograms_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "Id", string sortDir = "asc")
    {
        var (items, totalCount) = await collegeProgramService.GetFilteredItemsForExportAsync(page, pageSize, search, sort, sortDir);

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
        if (id == null)
        {
            return NotFound();
        }

        var collegeProgram = await collegeProgramService.GetCollegeProgramByIdAsync(id.Value);
        if (collegeProgram == null)
        {
            return NotFound();
        }

        return View(collegeProgram);
    }

    [RequirePermission("collegeprograms.create")]
    public async Task<IActionResult> Create()
    {
        var (colleges, programs) = await collegeProgramService.GetSelectListsAsync();
        ViewData["CollegeId"] = new SelectList(colleges, "Id", "Name");

        var programsData = programs.Select(p => new
        {
            id = p.Id,
            code = p.ProgramCode,
            name = p.ProgramName,
            shortName = p.ShortName
        });
        ViewBag.ProgramsJson = JsonSerializer.Serialize(programsData);

        return View();
    }

    [HttpGet]
    public async Task<JsonResult> GetExistingPrograms(int collegeId)
    {
        var ids = await collegeProgramService.GetExistingProgramIdsAsync(collegeId);
        return Json(ids);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("collegeprograms.create")]
    public async Task<IActionResult> Create(CollegeProgramBulkCreateViewModel model)
    {
        if (model.CollegeId <= 0)
            ModelState.AddModelError(nameof(model.CollegeId), "College is required.");

        if (model.Programs == null || model.Programs.Count == 0)
            ModelState.AddModelError("", "Please add at least one program.");

        if (ModelState.IsValid)
        {
            var existingIds = await collegeProgramService.GetExistingProgramIdsAsync(model.CollegeId);
            var duplicateIds = model.Programs
                .Where(p => existingIds.Contains(p.ProgramId))
                .Select(p => p.ProgramId)
                .ToList();

            if (duplicateIds.Any())
            {
                ModelState.AddModelError("", $"{duplicateIds.Count} program(s) already exist for this college.");
            }
        }

        if (ModelState.IsValid)
        {
            var collegePrograms = model.Programs.Select(p => new CollegeProgram
            {
                CollegeId = model.CollegeId,
                ProgramId = p.ProgramId,
                AffiliationDate = p.AffiliationDate,
                NumberOfStudents = p.NumberOfStudents,
                Remarks = p.Remarks,
                IsActive = p.IsActive
            }).ToList();

            await collegeProgramService.CreateCollegeProgramsAsync(collegePrograms);
            TempData["SuccessMessage"] = $"{collegePrograms.Count} college program(s) created successfully.";
            return RedirectToAction(nameof(Index));
        }

        var (colleges, programs) = await collegeProgramService.GetSelectListsAsync();
        ViewData["CollegeId"] = new SelectList(colleges, "Id", "Name", model.CollegeId);

        var programsData = programs.Select(p => new
        {
            id = p.Id,
            code = p.ProgramCode,
            name = p.ProgramName,
            shortName = p.ShortName
        });
        ViewBag.ProgramsJson = JsonSerializer.Serialize(programsData);

        return View(model);
    }

    [RequirePermission("collegeprograms.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var collegeProgram = await collegeProgramService.GetCollegeProgramByIdAsync(id.Value);
        if (collegeProgram == null)
        {
            return NotFound();
        }
        var (colleges, programs) = await collegeProgramService.GetSelectListsAsync();
        ViewData["CollegeId"] = new SelectList(colleges, "Id", "Name", collegeProgram.CollegeId);
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", collegeProgram.ProgramId);
        return View(collegeProgram);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("collegeprograms.edit")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,AffiliationDate,NumberOfStudents,Remarks,IsActive,CollegeId,ProgramId")] CollegeProgram collegeProgram)
    {
        if (id != collegeProgram.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await collegeProgramService.UpdateCollegeProgramAsync(collegeProgram);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await collegeProgramService.CollegeProgramExistsAsync(collegeProgram.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        var (colleges, programs) = await collegeProgramService.GetSelectListsAsync();
        ViewData["CollegeId"] = new SelectList(colleges, "Id", "Name", collegeProgram.CollegeId);
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", collegeProgram.ProgramId);
        return View(collegeProgram);
    }

    [RequirePermission("collegeprograms.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var collegeProgram = await collegeProgramService.GetCollegeProgramByIdAsync(id.Value);
        if (collegeProgram == null)
        {
            return NotFound();
        }

        return View(collegeProgram);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [RequirePermission("collegeprograms.delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await collegeProgramService.DeleteCollegeProgramAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
