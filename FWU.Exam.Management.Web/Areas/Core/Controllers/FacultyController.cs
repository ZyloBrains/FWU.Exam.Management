using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Core.Controllers;

[Area("Core")]
[Authorize(Roles = "SuperAdmin")]
public class FacultyController(IFacultyService facultyService, IFileUploadHelper fileUploadHelper) : Controller
{
    public async Task<IActionResult> Index()
    {
        var faculties = await facultyService.GetAllFacultiesAsync();
        return View(faculties);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var faculty = await facultyService.GetFacultyByIdAsync(id.Value);
        if (faculty == null) return NotFound();

        return View(faculty);
    }

    public IActionResult Create()
    {
        return View();
    }

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

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var faculty = await facultyService.GetFacultyByIdAsync(id.Value);
        if (faculty == null) return NotFound();

        return View(faculty);
    }

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

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var faculty = await facultyService.GetFacultyByIdAsync(id.Value);
        if (faculty == null) return NotFound();

        return View(faculty);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await facultyService.DeleteFacultyAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
