using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace FWU.Exam.Management.Web.Areas.Students.Controllers;

[Area("Students")]
[Authorize(Roles = "SuperAdmin,FacultyAdmin,CollegeAdmin,DepartmentAdmin")]
public class StudentAdmissionsController(IStudentAdmissionService admissionService, IStudentRegistrationService studentService, UserManager<AppUser> userManager, AppDbContext context) : Controller
{
    private async Task<List<int>> GetUserCollegeIdsAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return new List<int>();

        if (User.IsInRole(Role.SuperAdmin))
            return new List<int>();

        if (User.IsInRole(Role.FacultyAdmin) && user.FacultyId != null)
        {
            return await context.Colleges
                .Where(c => c.Faculties.Any(f => f.Id == user.FacultyId))
                .Select(c => c.Id)
                .ToListAsync();
        }

        if (User.IsInRole(Role.CollegeAdmin) && user.CollegeId != null)
        {
            return new List<int> { user.CollegeId.Value };
        }

        if (User.IsInRole(Role.DepartmentAdmin) && user.CollegeId != null)
        {
            return new List<int> { user.CollegeId.Value };
        }

        return new List<int>();
    }

    public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "AdmissionDate", string sortDir = "desc", int pageSize = 10)
    {
        var collegeIds = await GetUserCollegeIdsAsync();
        int? collegeId = collegeIds.Count == 1 ? collegeIds[0] : (int?)null;

        var (items, totalCount) = await admissionService.GetAdmissionsAsync(page, pageSize, search, sort, sortDir, collegeIds.Count > 0 ? collegeId : null);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View(items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var admission = await admissionService.GetAdmissionByIdAsync(id.Value);
        if (admission == null) return NotFound();

        return View(admission);
    }

    public async Task<IActionResult> Create(int? studentRegistrationId = null)
    {
        var collegeIds = await GetUserCollegeIdsAsync();

        if (collegeIds.Count == 1)
        {
            ViewBag.CollegeId = new SelectList(await context.Colleges.Where(c => c.Id == collegeIds[0]).ToListAsync(), "Id", "Name", collegeIds[0]);
        }
        else
        {
            var colleges = await admissionService.GetCollegeSelectListAsync();
            ViewBag.CollegeId = new SelectList(colleges.Select(c => new { c.Id, c.Name }), "Id", "Name");
        }

        ViewBag.ProgramsId = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
        ViewBag.StudentRegistrationId = studentRegistrationId;

        if (studentRegistrationId.HasValue)
        {
            var reg = await context.StudentRegistrations
                .AsNoTracking()
                .Include(r => r.Program)
                .FirstOrDefaultAsync(r => r.Id == studentRegistrationId.Value);
            if (reg != null)
            {
                ViewBag.SelectedStudent = reg;
            }
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentAdmission admission, int? studentRegistrationId)
    {
        if (studentRegistrationId.HasValue)
        {
            var reg = await context.StudentRegistrations
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == studentRegistrationId.Value);

            if (reg != null && !string.IsNullOrEmpty(reg.Email))
            {
                var appUserId = await admissionService.GetAppUserIdByEmailAsync(reg.Email);
                if (!string.IsNullOrEmpty(appUserId))
                {
                    admission.AppUserId = appUserId;
                }

                if (string.IsNullOrEmpty(admission.CollegeRollNumber))
                {
                    admission.CollegeRollNumber = reg.RegistrationNumber;
                }

                if (admission.ProgramsId == 0 && reg.ProgramId.HasValue)
                {
                    admission.ProgramsId = reg.ProgramId.Value;
                }

                if (admission.CollegeId == 0)
                {
                    admission.CollegeId = reg.CollegeId;
                }
            }
        }

        if (ModelState.IsValid)
        {
            await admissionService.CreateAdmissionAsync(admission);
            TempData["SuccessMessage"] = "Student admission created successfully!";
            return RedirectToAction(nameof(Index));
        }

        var collegeIds = await GetUserCollegeIdsAsync();
        if (collegeIds.Count == 1)
        {
            ViewBag.CollegeId = new SelectList(await context.Colleges.Where(c => c.Id == collegeIds[0]).ToListAsync(), "Id", "Name", collegeIds[0]);
        }
        else
        {
            var colleges = await admissionService.GetCollegeSelectListAsync();
            ViewBag.CollegeId = new SelectList(colleges.Select(c => new { c.Id, c.Name }), "Id", "Name", admission.CollegeId);
        }

        ViewBag.ProgramsId = new SelectList(await admissionService.GetCollegeProgramsAsync(admission.CollegeId), "Id", "ProgramName", admission.ProgramsId);
        ViewBag.StudentRegistrationId = studentRegistrationId;
        return View(admission);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var admission = await admissionService.GetAdmissionByIdAsync(id.Value);
        if (admission == null) return NotFound();

        var colleges = await admissionService.GetCollegeSelectListAsync();
        ViewBag.CollegeId = new SelectList(colleges.Select(c => new { c.Id, c.Name }), "Id", "Name", admission.CollegeId);
        ViewBag.ProgramsId = new SelectList(await admissionService.GetCollegeProgramsAsync(admission.CollegeId), "Id", "ProgramName", admission.ProgramsId);
        return View(admission);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StudentAdmission admission)
    {
        if (id != admission.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await admissionService.UpdateAdmissionAsync(admission);
                TempData["SuccessMessage"] = "Student admission updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await admissionService.AdmissionExistsAsync(admission.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        var colleges = await admissionService.GetCollegeSelectListAsync();
        ViewBag.CollegeId = new SelectList(colleges.Select(c => new { c.Id, c.Name }), "Id", "Name", admission.CollegeId);
        ViewBag.ProgramsId = new SelectList(await admissionService.GetCollegeProgramsAsync(admission.CollegeId), "Id", "ProgramName", admission.ProgramsId);
        return View(admission);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var admission = await admissionService.GetAdmissionByIdAsync(id.Value);
        if (admission == null) return NotFound();

        return View(admission);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await admissionService.DeleteAdmissionAsync(id);
        TempData["SuccessMessage"] = "Student admission deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    private string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "AdmissionDate", string sortDir = "desc")
    {
        var collegeIds = await GetUserCollegeIdsAsync();
        int? collegeId = collegeIds.Count == 1 ? collegeIds[0] : (int?)null;
        var items = await admissionService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir, collegeIds.Count > 0 ? collegeId : null);

        var sb = new StringBuilder();
        sb.AppendLine("S.N.,College Roll No.,College,Program,Admission Date,Status,Active");

        int sn = 1;
        foreach (var a in items)
        {
            sb.AppendLine($"{sn++}," +
                          $"{EscapeCsv(a.CollegeRollNumber)}," +
                          $"{EscapeCsv(a.College?.Name)}," +
                          $"{EscapeCsv(a.Program?.ProgramName)}," +
                          $"{a.AdmissionDate:yyyy-MM-dd}," +
                          $"{(a.IsCompleted ? "Completed" : "Pending")}," +
                          $"{(a.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"StudentAdmissions_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "AdmissionDate", string sortDir = "desc")
    {
        var collegeIds = await GetUserCollegeIdsAsync();
        int? collegeId = collegeIds.Count == 1 ? collegeIds[0] : (int?)null;
        var items = await admissionService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir, collegeIds.Count > 0 ? collegeId : null);

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = (await admissionService.GetAdmissionsAsync(page, pageSize, search, sort, sortDir, collegeIds.Count > 0 ? collegeId : null)).TotalCount;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<JsonResult> SearchStudents(string search, int collegeId)
    {
        if (string.IsNullOrWhiteSpace(search) || search.Length < 2)
            return Json(new List<object>());

        var lowerSearch = search.ToLower();
        var students = await context.StudentRegistrations
            .AsNoTracking()
            .Include(s => s.Program)
            .Where(s => s.CollegeId == collegeId && s.IsActive)
            .Where(s => (s.RegistrationNumber != null && s.RegistrationNumber.ToLower().Contains(lowerSearch))
                     || (s.FirstName != null && s.FirstName.ToLower().Contains(lowerSearch))
                     || (s.LastName != null && s.LastName.ToLower().Contains(lowerSearch))
                     || (s.Email != null && s.Email.ToLower().Contains(lowerSearch)))
            .OrderBy(s => s.RegistrationNumber)
            .Take(20)
            .Select(s => new
            {
                s.Id,
                RegistrationNumber = s.RegistrationNumber ?? "",
                FullName = (s.FirstName + " " + s.LastName).Trim(),
                Email = s.Email ?? "",
                Program = s.Program != null ? s.Program.ProgramName : ""
            })
            .ToListAsync();

        return Json(students);
    }

    [HttpGet]
    public async Task<JsonResult> GetCollegePrograms(int collegeId)
    {
        var programs = await admissionService.GetCollegeProgramsAsync(collegeId);
        return Json(programs.Select(p => new { id = p.Id, name = p.ProgramName }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteAdmission(int id)
    {
        var user = await userManager.GetUserAsync(User);
        await admissionService.CompleteAdmissionAsync(id, user?.Id ?? "");
        TempData["SuccessMessage"] = "Admission completed successfully!";
        return RedirectToAction(nameof(Index));
    }
}
