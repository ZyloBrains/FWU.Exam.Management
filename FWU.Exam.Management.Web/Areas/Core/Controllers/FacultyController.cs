using System.Text;
using ClosedXML.Excel;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Extensions;
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
    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "Name", string sortDir = "asc", int pageSize = 10)
    {
        var (items, totalCount) = await facultyService.GetFacultiesPagedAsync(page, pageSize, search, sort, sortDir);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel()
    {
        var items = await facultyService.GetAllFacultiesAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Faculties");

        var headers = new[] { "Name", "Short Name", "Office Code", "Contact Number", "Email", "Address" };
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
            worksheet.Cell(row, 2).Value = f.ShortName;
            worksheet.Cell(row, 3).Value = f.OfficeCode;
            worksheet.Cell(row, 4).Value = f.ContactNumber;
            worksheet.Cell(row, 5).Value = f.Email;
            worksheet.Cell(row, 6).Value = f.Address;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        var fileName = $"Faculties_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    public async Task<IActionResult> ExportToCsv(string? search = null, string sort = "Name", string sortDir = "asc")
    {
        var faculties = await facultyService.GetAllFacultiesAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            faculties = faculties.Where(f =>
                (f.Name?.ToLower().Contains(s) ?? false) ||
                (f.OfficeCode?.ToLower().Contains(s) ?? false) ||
                (f.ShortName?.ToLower().Contains(s) ?? false) ||
                (f.Email?.ToLower().Contains(s) ?? false) ||
                (f.ContactNumber?.ToLower().Contains(s) ?? false) ||
                (f.Address?.ToLower().Contains(s) ?? false)
            ).ToList();
        }

        faculties = (sort?.ToLower()) switch
        {
            "name" => sortDir == "desc" ? faculties.OrderByDescending(f => f.Name).ToList() : faculties.OrderBy(f => f.Name).ToList(),
            "shortname" => sortDir == "desc" ? faculties.OrderByDescending(f => f.ShortName).ToList() : faculties.OrderBy(f => f.ShortName).ToList(),
            "officecode" => sortDir == "desc" ? faculties.OrderByDescending(f => f.OfficeCode).ToList() : faculties.OrderBy(f => f.OfficeCode).ToList(),
            "email" => sortDir == "desc" ? faculties.OrderByDescending(f => f.Email).ToList() : faculties.OrderBy(f => f.Email).ToList(),
            "contactnumber" => sortDir == "desc" ? faculties.OrderByDescending(f => f.ContactNumber).ToList() : faculties.OrderBy(f => f.ContactNumber).ToList(),
            "address" => sortDir == "desc" ? faculties.OrderByDescending(f => f.Address).ToList() : faculties.OrderBy(f => f.Address).ToList(),
            _ => sortDir == "desc" ? faculties.OrderByDescending(f => f.Name).ToList() : faculties.OrderBy(f => f.Name).ToList()
        };

        var sb = new StringBuilder();
        sb.AppendLine("Name,Short Name,Office Code,Contact Number,Email,Address");

        foreach (var f in faculties)
        {
            sb.AppendLine($"{f.Name.EscapeCsv()},{f.ShortName.EscapeCsv()},{f.OfficeCode.EscapeCsv()},{f.ContactNumber.EscapeCsv()},{f.Email.EscapeCsv()},{f.Address.EscapeCsv()}");
        }

        var fileName = $"Faculties_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(string? search = null, string sort = "Name", string sortDir = "asc")
    {
        var faculties = await facultyService.GetAllFacultiesAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            faculties = faculties.Where(f =>
                (f.Name?.ToLower().Contains(s) ?? false) ||
                (f.OfficeCode?.ToLower().Contains(s) ?? false) ||
                (f.ShortName?.ToLower().Contains(s) ?? false) ||
                (f.Email?.ToLower().Contains(s) ?? false) ||
                (f.ContactNumber?.ToLower().Contains(s) ?? false) ||
                (f.Address?.ToLower().Contains(s) ?? false)
            ).ToList();
        }

        faculties = (sort?.ToLower()) switch
        {
            "name" => sortDir == "desc" ? faculties.OrderByDescending(f => f.Name).ToList() : faculties.OrderBy(f => f.Name).ToList(),
            "shortname" => sortDir == "desc" ? faculties.OrderByDescending(f => f.ShortName).ToList() : faculties.OrderBy(f => f.ShortName).ToList(),
            "officecode" => sortDir == "desc" ? faculties.OrderByDescending(f => f.OfficeCode).ToList() : faculties.OrderBy(f => f.OfficeCode).ToList(),
            "email" => sortDir == "desc" ? faculties.OrderByDescending(f => f.Email).ToList() : faculties.OrderBy(f => f.Email).ToList(),
            "contactnumber" => sortDir == "desc" ? faculties.OrderByDescending(f => f.ContactNumber).ToList() : faculties.OrderBy(f => f.ContactNumber).ToList(),
            "address" => sortDir == "desc" ? faculties.OrderByDescending(f => f.Address).ToList() : faculties.OrderBy(f => f.Address).ToList(),
            _ => sortDir == "desc" ? faculties.OrderByDescending(f => f.Name).ToList() : faculties.OrderBy(f => f.Name).ToList()
        };

        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View("PrintPdf", faculties);
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

            TempData["SuccessMessage"] = "Faculty created successfully!";
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
            TempData["SuccessMessage"] = "Faculty updated successfully!";
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
        var (canDelete, blockingEntities) = await facultyService.CheckDeleteDependenciesAsync(id);
        if (!canDelete)
        {
            TempData["ErrorMessage"] = $"Cannot delete this Faculty. It is referenced by: {string.Join(", ", blockingEntities)}. Please remove or reassign these records first.";
            return RedirectToAction(nameof(Index));
        }

        await facultyService.DeleteFacultyAsync(id);
        TempData["SuccessMessage"] = "Faculty deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    [RequirePermission("faculties.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        var (canDelete, blockingEntities) = await facultyService.CheckDeleteDependenciesAsync(id);
        if (!canDelete)
        {
            return Json(new { success = false, message = $"Cannot delete this Faculty. It is referenced by: {string.Join(", ", blockingEntities)}. Please remove or reassign these records first." });
        }

        try
        {
            await facultyService.DeleteFacultyAsync(id);
            return Json(new { success = true, message = "Faculty deleted successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

}
