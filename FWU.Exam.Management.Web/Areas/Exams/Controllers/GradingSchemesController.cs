using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Domain.Extensions;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("gradingschemes.view")]
public class GradingSchemesController(
    IGradingSchemeService gradingSchemeService) : Controller
{
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await gradingSchemeService.GetGradingSchemesAsync(page, pageSize, search, sort, sortDir);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }

    [RequirePermission("gradingschemes.create")]
    public async Task<IActionResult> Create()
    {
        var selectLists = await gradingSchemeService.GetSelectListDataAsync();
        PopulateDropdowns(selectLists);
        var model = new GradingScheme { IsActive = true };
        model.GradeDefinitions = GetDefaultGradeDefinitions();
        return View(model);
    }

    [HttpPost]
    [RequirePermission("gradingschemes.create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GradingScheme gradingScheme, string? gradeLetters, string? minPercentages, string? maxPercentages, string? gradePoints, string? remarks, string? isPasses, string? displayOrders)
    {
        if (ModelState.IsValid)
        {
            gradingScheme.GradeDefinitions = ParseGradeDefinitions(
                gradeLetters, minPercentages, maxPercentages,
                gradePoints, remarks, isPasses, displayOrders);

            await gradingSchemeService.CreateGradingSchemeAsync(gradingScheme);
            TempData["SuccessMessage"] = "Grading scheme created successfully!";
            return RedirectToAction(nameof(Index));
        }
        var selectLists = await gradingSchemeService.GetSelectListDataAsync();
        PopulateDropdowns(selectLists, gradingScheme);
        gradingScheme.GradeDefinitions ??= GetDefaultGradeDefinitions();
        return View(gradingScheme);
    }

    [RequirePermission("gradingschemes.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var gradingScheme = await gradingSchemeService.GetGradingSchemeByIdAsync(id.Value);
        if (gradingScheme == null) return NotFound();

        var selectLists = await gradingSchemeService.GetSelectListDataAsync();
        PopulateDropdowns(selectLists, gradingScheme);
        return View(gradingScheme);
    }

    [HttpPost]
    [RequirePermission("gradingschemes.edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GradingScheme gradingScheme, string? gradeLetters, string? minPercentages, string? maxPercentages, string? gradePoints, string? remarks, string? isPasses, string? displayOrders)
    {
        if (id != gradingScheme.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                gradingScheme.GradeDefinitions = ParseGradeDefinitions(
                    gradeLetters, minPercentages, maxPercentages,
                    gradePoints, remarks, isPasses, displayOrders, gradingScheme.Id);

                await gradingSchemeService.UpdateGradingSchemeAsync(gradingScheme);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await gradingSchemeService.GradingSchemeExistsAsync(gradingScheme.Id))
                    return NotFound();
                throw;
            }
            TempData["SuccessMessage"] = "Grading scheme updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        var selectLists = await gradingSchemeService.GetSelectListDataAsync();
        PopulateDropdowns(selectLists, gradingScheme);
        return View(gradingScheme);
    }

    [RequirePermission("gradingschemes.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var gradingScheme = await gradingSchemeService.GetGradingSchemeByIdAsync(id.Value);
        if (gradingScheme == null) return NotFound();

        return View(gradingScheme);
    }

    [HttpPost, ActionName("Delete")]
    [RequirePermission("gradingschemes.delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await gradingSchemeService.DeleteGradingSchemeAsync(id);
            TempData["SuccessMessage"] = "Grading scheme deleted successfully!";
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

        var gradingScheme = await gradingSchemeService.GetGradingSchemeByIdAsync(id.Value);
        if (gradingScheme == null) return NotFound();

        return View(gradingScheme);
    }

    public async Task<IActionResult> ExportToCsv(string? search = null)
    {
        var items = await gradingSchemeService.GetFilteredItemsAsync(search);

        var sb = new StringBuilder();
        sb.AppendLine("ID,Name,Program,Academic Year,Description,IsActive");

        foreach (var item in items)
        {
            sb.AppendLine($"{item.Id},{item.Name.EscapeCsv()},{(item.Program?.ProgramName ?? "").EscapeCsv()},{(item.AcademicYear?.AcademicYearName ?? "").EscapeCsv()},{(item.Description ?? "").EscapeCsv()},{(item.IsActive ? "Yes" : "No")}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", "GradingSchemes.csv");
    }

    public async Task<IActionResult> ExportToPdf(string? search = null)
    {
        var items = await gradingSchemeService.GetFilteredItemsAsync(search);
        return View("PrintPdf", items);
    }

    [RequirePermission("gradingschemes.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try
        {
            await gradingSchemeService.DeleteGradingSchemeAsync(id);
            return Json(new { success = true, message = "Grading scheme deleted successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    private void PopulateDropdowns(GradingSchemeSelectListsDto selectLists, GradingScheme? gradingScheme = null)
    {
        ViewData["ProgramId"] = new SelectList(selectLists.Programs, "Id", "Name", gradingScheme?.ProgramId);
        ViewData["AcademicYearId"] = new SelectList(selectLists.AcademicYears, "Id", "Name", gradingScheme?.AcademicYearId);
    }


    private static List<GradeDefinition> GetDefaultGradeDefinitions()
    {
        return
        [
            new() { GradeLetter = "A", MinPercentage = 90, MaxPercentage = 100, GradePoint = 4.0m, IsPass = true, DisplayOrder = 1 },
            new() { GradeLetter = "B+", MinPercentage = 80, MaxPercentage = 89.99m, GradePoint = 3.6m, IsPass = true, DisplayOrder = 2 },
            new() { GradeLetter = "B", MinPercentage = 70, MaxPercentage = 79.99m, GradePoint = 3.2m, IsPass = true, DisplayOrder = 3 },
            new() { GradeLetter = "C+", MinPercentage = 60, MaxPercentage = 69.99m, GradePoint = 2.8m, IsPass = true, DisplayOrder = 4 },
            new() { GradeLetter = "C", MinPercentage = 50, MaxPercentage = 59.99m, GradePoint = 2.4m, IsPass = true, DisplayOrder = 5 },
            new() { GradeLetter = "D", MinPercentage = 40, MaxPercentage = 49.99m, GradePoint = 2.0m, IsPass = true, DisplayOrder = 6 },
            new() { GradeLetter = "E", MinPercentage = 30, MaxPercentage = 39.99m, GradePoint = 1.6m, IsPass = false, DisplayOrder = 7 },
            new() { GradeLetter = "F", MinPercentage = 0, MaxPercentage = 29.99m, GradePoint = 0.0m, IsPass = false, DisplayOrder = 8 }
        ];
    }

    private static List<GradeDefinition> ParseGradeDefinitions(string? gradeLetters, string? minPercentages, string? maxPercentages, string? gradePoints, string? remarks, string? isPasses, string? displayOrders, int gradingSchemeId = 0)
    {
        var definitions = new List<GradeDefinition>();
        if (string.IsNullOrEmpty(gradeLetters)) return definitions;

        var letters = gradeLetters.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var mins = (minPercentages ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var maxs = (maxPercentages ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var points = (gradePoints ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var remarkList = (remarks ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var passList = (isPasses ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var orderList = (displayOrders ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        for (int i = 0; i < letters.Length; i++)
        {
            var gd = new GradeDefinition
            {
                GradingSchemeId = gradingSchemeId,
                GradeLetter = letters[i],
                DisplayOrder = i + 1
            };

            if (i < mins.Length && decimal.TryParse(mins[i], out var minVal)) gd.MinPercentage = minVal;
            if (i < maxs.Length && decimal.TryParse(maxs[i], out var maxVal)) gd.MaxPercentage = maxVal;
            if (i < points.Length && decimal.TryParse(points[i], out var ptVal)) gd.GradePoint = ptVal;
            if (i < remarkList.Length) gd.Remark = remarkList[i];
            if (i < passList.Length) gd.IsPass = passList[i] == "true";
            if (i < orderList.Length && int.TryParse(orderList[i], out var ordVal)) gd.DisplayOrder = ordVal;

            definitions.Add(gd);
        }

        return definitions;
    }
}
