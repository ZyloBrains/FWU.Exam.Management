using System.Text;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
public class ProgramsController(IProgramService programService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "ProgramCode", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await programService.GetProgramsAsync(page, pageSize, search, sort, sortDir);

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

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "ProgramCode", string sortDir = "asc")
    {
        var items = await programService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir);

        var sb = new StringBuilder();
        sb.AppendLine("Program Code,Program Name,Short Name,Level,Faculty,Board,Program Period Type,Duration,Grand Total Marks,Has Multiple Intakes,Number of Seats,Scholarship Seats,Roll Number Prefix,Remarks,Status");

        foreach (var p in items)
        {
            sb.AppendLine($"{EscapeCsv(p.ProgramCode)}," +
                           $"{EscapeCsv(p.ProgramName)}," +
                           $"{EscapeCsv(p.ShortName)}," +
                           $"{EscapeCsv(p.Level?.LevelName)}," +
                           $"{EscapeCsv(p.Faculty?.FacultyCode)}," +
                           $"{EscapeCsv(p.Board?.BoardName)}," +
                           $"{p.Duration}," +
                           $"{p.GrandTotalMarks}," +
                           $"{(p.HasMultipleIntakes ? "Yes" : "No")}," +
                           $"{p.NumberOfSeats}," +
                           $"{p.ScholarshipSeats}," +
                           $"{EscapeCsv(p.RollNumberPrefix)}," +
                           $"{EscapeCsv(p.Remarks)}," +
                           $"{(p.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"Programs_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "ProgramCode", string sortDir = "asc")
    {
        var (items, totalCount) = await programService.GetProgramsAsync(page, pageSize, search, sort, sortDir);

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

        var program = await programService.GetProgramByIdAsync(id.Value);
        if (program == null) return NotFound();

        return View(program);
    }

    public async Task<IActionResult> Create()
    {
        var (boards, faculties, levels) = await programService.GetSelectListsAsync();
        ViewData["BoardId"] = new SelectList(boards, "Id", "BoardName");
        ViewData["FacultyId"] = new SelectList(faculties, "Id", "FacultyCode");
        ViewData["LevelId"] = new SelectList(levels, "Id", "LevelName");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,LevelId,FacultyId,BoardId,ProgramCode,ProgramName,ShortName,Duration,GrandTotalMarks,HasMultipleIntakes,NumberOfSeats,ScholarshipSeats,Remarks,IsActive,RollNumberPrefix")] Program program)
    {
        if (ModelState.IsValid)
        {
            await programService.CreateProgramAsync(program);
            return RedirectToAction(nameof(Index));
        }

        var (boards, faculties, levels) = await programService.GetSelectListsAsync(program.BoardId, program.FacultyId, program.LevelId);
        ViewData["BoardId"] = new SelectList(boards, "Id", "BoardName", program.BoardId);
        ViewData["FacultyId"] = new SelectList(faculties, "Id", "FacultyCode", program.FacultyId);
        ViewData["LevelId"] = new SelectList(levels, "Id", "LevelName", program.LevelId);
        return View(program);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var program = await programService.GetProgramByIdAsync(id.Value);
        if (program == null) return NotFound();

        var (boards, faculties, levels) = await programService.GetSelectListsAsync(program.BoardId, program.FacultyId, program.LevelId);
        ViewData["BoardId"] = new SelectList(boards, "Id", "BoardName", program.BoardId);
        ViewData["FacultyId"] = new SelectList(faculties, "Id", "FacultyCode", program.FacultyId);
        ViewData["LevelId"] = new SelectList(levels, "Id", "LevelName", program.LevelId);
        return View(program);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,LevelId,FacultyId,BoardId,ProgramCode,ProgramName,ShortName,Duration,GrandTotalMarks,HasMultipleIntakes,NumberOfSeats,ScholarshipSeats,Remarks,IsActive,RollNumberPrefix")] Program program)
    {
        if (id != program.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await programService.UpdateProgramAsync(program);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await programService.ProgramExistsAsync(program.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        var (boards, faculties, levels) = await programService.GetSelectListsAsync(program.BoardId, program.FacultyId, program.LevelId);
        ViewData["BoardId"] = new SelectList(boards, "Id", "BoardName", program.BoardId);
        ViewData["FacultyId"] = new SelectList(faculties, "Id", "FacultyCode", program.FacultyId);
        ViewData["LevelId"] = new SelectList(levels, "Id", "LevelName", program.LevelId);
        return View(program);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var program = await programService.GetProgramByIdAsync(id.Value);
        if (program == null) return NotFound();

        return View(program);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await programService.DeleteProgramAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
