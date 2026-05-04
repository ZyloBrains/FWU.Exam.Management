using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace FWU.Exam.Management.Web.Controllers;

public class CollegeProgramsController : Controller
{
    private readonly ICollegeProgramService _collegeProgramService;

    public CollegeProgramsController(ICollegeProgramService collegeProgramService)
    {
        _collegeProgramService = collegeProgramService;
    }

    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await _collegeProgramService.GetCollegeProgramsAsync(page, pageSize, search, sort, sortDir);

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

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "Id", string sortDir = "asc")
    {
        var (items, totalCount) = await _collegeProgramService.GetFilteredItemsForExportAsync(page, pageSize, search, sort, sortDir);

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
        var (items, totalCount) = await _collegeProgramService.GetFilteredItemsForExportAsync(page, pageSize, search, sort, sortDir);

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

        var collegeProgram = await _collegeProgramService.GetCollegeProgramByIdAsync(id.Value);
        if (collegeProgram == null)
        {
            return NotFound();
        }

        return View(collegeProgram);
    }

    public async Task<IActionResult> Create()
    {
        var (colleges, programs) = await _collegeProgramService.GetSelectListsAsync();
        ViewData["CollegeId"] = new SelectList(colleges, "Id", "Name");
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,AffiliationDate,NumberOfStudents,Remarks,IsActive,CollegeId,ProgramId")] CollegeProgram collegeProgram)
    {
        if (ModelState.IsValid)
        {
            await _collegeProgramService.CreateCollegeProgramAsync(collegeProgram);
            return RedirectToAction(nameof(Index));
        }
        var (colleges, programs) = await _collegeProgramService.GetSelectListsAsync();
        ViewData["CollegeId"] = new SelectList(colleges, "Id", "Name", collegeProgram.CollegeId);
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", collegeProgram.ProgramId);
        return View(collegeProgram);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var collegeProgram = await _collegeProgramService.GetCollegeProgramByIdAsync(id.Value);
        if (collegeProgram == null)
        {
            return NotFound();
        }
        var (colleges, programs) = await _collegeProgramService.GetSelectListsAsync();
        ViewData["CollegeId"] = new SelectList(colleges, "Id", "Name", collegeProgram.CollegeId);
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", collegeProgram.ProgramId);
        return View(collegeProgram);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
                await _collegeProgramService.UpdateCollegeProgramAsync(collegeProgram);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _collegeProgramService.CollegeProgramExistsAsync(collegeProgram.Id))
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
        var (colleges, programs) = await _collegeProgramService.GetSelectListsAsync();
        ViewData["CollegeId"] = new SelectList(colleges, "Id", "Name", collegeProgram.CollegeId);
        ViewData["ProgramId"] = new SelectList(programs, "Id", "ProgramName", collegeProgram.ProgramId);
        return View(collegeProgram);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var collegeProgram = await _collegeProgramService.GetCollegeProgramByIdAsync(id.Value);
        if (collegeProgram == null)
        {
            return NotFound();
        }

        return View(collegeProgram);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _collegeProgramService.DeleteCollegeProgramAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
