using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[RequirePermission("faculties.view")]
public class FacultyController(IFacultyService facultyService, IFileUploadHelper fileUploadHelper) : Controller
{
    public async Task<IActionResult> Index()
    {
        var faculties = await facultyService.GetAllFacultiesAsync();
        return View(faculties);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel()
    {
        var items = await facultyService.GetAllFacultiesAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Faculties");

        var headers = new[] { "Name", "Office Code", "Contact Number", "Email", "Address" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        int row = 2;
        foreach (var f in items)
        {
            worksheet.Cell(row, 1).Value = f.Name;
            worksheet.Cell(row, 2).Value = f.OfficeCode;
            worksheet.Cell(row, 3).Value = f.ContactNumber;
            worksheet.Cell(row, 4).Value = f.Email;
            worksheet.Cell(row, 5).Value = f.Address;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"Faculties_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var faculty = await facultyService.GetFacultyByIdAsync(id.Value);
        if (faculty == null) return NotFound();

        return View(faculty);
    }

    [RequirePermission("faculties.create")]
    public IActionResult Create()
    {
        return View();
    }

    [RequirePermission("faculties.create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Faculty faculty, IFormFile? logoFile, string? adminPassword)
    {
        if (ModelState.IsValid)
        {
            if (logoFile != null)
            {
                faculty.LogoPath = await fileUploadHelper.UploadAsync(logoFile);
            }

            var resultPassword = await facultyService.CreateFacultyAsync(faculty, adminPassword ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(resultPassword))
            {
                TempData["OrgLoginEmail"] = faculty.Email;
                TempData["OrgLoginPassword"] = resultPassword;
                TempData["OrgOfficeCode"] = faculty.OfficeCode;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(faculty);
    }

    [RequirePermission("faculties.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var faculty = await facultyService.GetFacultyByIdAsync(id.Value);
        if (faculty == null) return NotFound();

        return View(faculty);
    }

    [RequirePermission("faculties.edit")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Faculty faculty, IFormFile? logoFile)
    {
        if (id != faculty.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                if (logoFile != null)
                {
                    faculty.LogoPath = await fileUploadHelper.UploadAsync(logoFile);
                }

                await facultyService.UpdateFacultyAsync(faculty);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await facultyService.FacultyExistsAsync(faculty.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        return View(faculty);
    }

    [RequirePermission("faculties.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var faculty = await facultyService.GetFacultyByIdAsync(id.Value);
        if (faculty == null) return NotFound();

        return View(faculty);
    }

    [RequirePermission("faculties.delete")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await facultyService.DeleteFacultyAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
