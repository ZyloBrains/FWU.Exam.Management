using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Domain.Extensions;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Students.Controllers;

[Area("Students")]
[RequirePermission("studentadmissions.view")]
public class StudentAdmissionsController(IStudentAdmissionService admissionService, UserManager<AppUser> userManager, AppDbContext context, IUserContext userContext) : Controller
{
    private async Task<List<int>> GetUserCollegeIdsAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return new List<int>();

        if (User.IsInRole(Role.SuperAdmin))
            return new List<int>();

        if (User.IsInRole(Role.FacultyAdmin) && user.FacultyId != null)
        {
            return await context.CollegePrograms
                .Where(cp => cp.Program != null && cp.Program.FacultyId == user.FacultyId)
                .Select(cp => cp.CollegeId)
                .Distinct()
                .ToListAsync();
        }

        if (User.IsInRole(Role.CollegeAdmin) && user.CollegeId != null)
        {
            return new List<int> { user.CollegeId.Value };
        }

        return new List<int>();
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null, string sort = "AdmissionDate", string sortDir = "desc", int pageSize = 10, int? collegeId = null, int? programId = null, DateTime? fromDate = null, DateTime? toDate = null, string? status = null)
    {
        var (items, totalCount) = await admissionService.GetAdmissionsAsync(page, pageSize, search, sort, sortDir, collegeId, programId, fromDate, toDate, status);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;
        ViewBag.CollegeId = collegeId;
        ViewBag.ProgramId = programId;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.Status = status;

        var colleges = await admissionService.GetCollegeSelectListAsync();
        ViewBag.Colleges = new SelectList(colleges.Select(c => new { c.Id, c.Name }), "Id", "Name", collegeId);

        if (collegeId.HasValue)
        {
            var programs = await admissionService.GetCollegeProgramsAsync(collegeId.Value);
            ViewBag.Programs = new SelectList(programs, "Id", "ProgramName", programId);
        }
        else
        {
            ViewBag.Programs = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
        }

        return View(items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var admission = await admissionService.GetAdmissionByIdAsync(id.Value);
        if (admission == null) return NotFound();

        return View(admission);
    }

    [RequirePermission("studentadmissions.create")]
    public async Task<IActionResult> Create(int? studentRegistrationId = null)
    {
        var admission = new StudentAdmission();

        if (!userContext.IsSuperAdmin && userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
        {
            ViewBag.CollegeId = new SelectList(await context.Colleges.Where(c => c.Id == userContext.CollegeId.Value).ToListAsync(), "Id", "Name", userContext.CollegeId.Value);
        }
        else
        {
            var colleges = await admissionService.GetCollegeSelectListAsync();
            ViewBag.CollegeId = new SelectList(colleges.Select(c => new { c.Id, c.Name }), "Id", "Name");
        }

        ViewBag.GenderId = new SelectList(await context.Genders.AsNoTracking().Where(g => g.IsActive).OrderBy(g => g.GenderName).ToListAsync(), "Id", "GenderName");
        ViewBag.AcademicYearId = new SelectList(await context.AcademicYears.AsNoTracking().OrderBy(ay => ay.AcademicYearName).ToListAsync(), "Id", "AcademicYearName");
        ViewBag.ProgramsId = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
        ViewBag.StudentRegistrationId = studentRegistrationId;

        if (studentRegistrationId.HasValue)
        {
            var reg = await context.StudentRegistrations
                .AsNoTracking()
                .Include(r => r.Program)
                .Include(r => r.College)
                .Include(r => r.AcademicYear)
                .FirstOrDefaultAsync(r => r.Id == studentRegistrationId.Value);
            if (reg != null)
            {
                admission.CollegeId = reg.CollegeId;
                if (reg.ProgramId.HasValue) admission.ProgramsId = reg.ProgramId.Value;
                admission.AcademicYearId = reg.AcademicYearId;
                admission.FirstName = reg.FirstName;
                admission.MiddleName = reg.MiddleName;
                admission.LastName = reg.LastName;
                admission.NepaliName = reg.NepaliName;
                admission.DateOfBirthBS = reg.DateOfBirthBS;
                admission.DateOfBirthAD = reg.DateOfBirthAD;
                admission.GenderId = reg.GenderId;
                admission.ContactNumber = reg.ContactNumber;
                admission.Phone = reg.Phone;
                admission.Email = reg.Email;
                admission.CollegeRollNumber = reg.RegistrationNumber;

                ViewBag.SelectedStudent = reg;
                ViewBag.ProgramsId = new SelectList(await admissionService.GetCollegeProgramsAsync(reg.CollegeId), "Id", "ProgramName", reg.ProgramId);
            }
        }

        return View(admission);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("studentadmissions.create")]
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

                if (string.IsNullOrEmpty(admission.FirstName))
                    admission.FirstName = reg.FirstName;
                admission.MiddleName ??= reg.MiddleName;
                if (string.IsNullOrEmpty(admission.LastName))
                    admission.LastName = reg.LastName;
                admission.NepaliName ??= reg.NepaliName;
                admission.DateOfBirthBS ??= reg.DateOfBirthBS;
                admission.DateOfBirthAD ??= reg.DateOfBirthAD;
                admission.GenderId ??= reg.GenderId;
                admission.ContactNumber ??= reg.ContactNumber;
                admission.Phone ??= reg.Phone;
                admission.Email ??= reg.Email;

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

                if (admission.AcademicYearId == 0)
                {
                    admission.AcademicYearId = reg.AcademicYearId;
                }
            }
        }

        if (ModelState.IsValid)
        {
            var admissionId = await admissionService.CreateAdmissionAsync(admission);

            if (studentRegistrationId.HasValue)
            {
                var registrationToLink = await context.StudentRegistrations
                    .FirstOrDefaultAsync(r => r.Id == studentRegistrationId.Value);
                if (registrationToLink != null)
                {
                    registrationToLink.StudentAdmissionId = admissionId;
                    await context.SaveChangesAsync();
                }
            }

            TempData["SuccessMessage"] = "Student admission created successfully!";
            return RedirectToAction(nameof(Index));
        }

        if (!userContext.IsSuperAdmin && userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
        {
            ViewBag.CollegeId = new SelectList(await context.Colleges.Where(c => c.Id == userContext.CollegeId.Value).ToListAsync(), "Id", "Name", userContext.CollegeId.Value);
        }
        else
        {
            var colleges = await admissionService.GetCollegeSelectListAsync();
            ViewBag.CollegeId = new SelectList(colleges.Select(c => new { c.Id, c.Name }), "Id", "Name", admission.CollegeId);
        }

        ViewBag.ProgramsId = new SelectList(await admissionService.GetCollegeProgramsAsync(admission.CollegeId), "Id", "ProgramName", admission.ProgramsId);
        ViewBag.StudentRegistrationId = studentRegistrationId;
        ViewBag.GenderId = new SelectList(await context.Genders.AsNoTracking().Where(g => g.IsActive).OrderBy(g => g.GenderName).ToListAsync(), "Id", "GenderName", admission.GenderId);
        ViewBag.AcademicYearId = new SelectList(await context.AcademicYears.AsNoTracking().OrderBy(ay => ay.AcademicYearName).ToListAsync(), "Id", "AcademicYearName", admission.AcademicYearId);
        return View(admission);
    }

    [RequirePermission("studentadmissions.edit")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var admission = await admissionService.GetAdmissionByIdAsync(id.Value);
        if (admission == null) return NotFound();

        var colleges = await admissionService.GetCollegeSelectListAsync();
        ViewBag.CollegeId = new SelectList(colleges.Select(c => new { c.Id, c.Name }), "Id", "Name", admission.CollegeId);
        ViewBag.ProgramsId = new SelectList(await admissionService.GetCollegeProgramsAsync(admission.CollegeId), "Id", "ProgramName", admission.ProgramsId);
        ViewBag.GenderId = new SelectList(await context.Genders.AsNoTracking().Where(g => g.IsActive).OrderBy(g => g.GenderName).ToListAsync(), "Id", "GenderName", admission.GenderId);
        ViewBag.AcademicYearId = new SelectList(await context.AcademicYears.AsNoTracking().OrderBy(ay => ay.AcademicYearName).ToListAsync(), "Id", "AcademicYearName", admission.AcademicYearId);
        return View(admission);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("studentadmissions.edit")]
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
        ViewBag.GenderId = new SelectList(await context.Genders.AsNoTracking().Where(g => g.IsActive).OrderBy(g => g.GenderName).ToListAsync(), "Id", "GenderName", admission.GenderId);
        ViewBag.AcademicYearId = new SelectList(await context.AcademicYears.AsNoTracking().OrderBy(ay => ay.AcademicYearName).ToListAsync(), "Id", "AcademicYearName", admission.AcademicYearId);
        return View(admission);
    }

    [RequirePermission("studentadmissions.delete")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var admission = await admissionService.GetAdmissionByIdAsync(id.Value);
        if (admission == null) return NotFound();

        return View(admission);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [RequirePermission("studentadmissions.delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await admissionService.DeleteAdmissionAsync(id);
            TempData["SuccessMessage"] = "Student admission deleted successfully!";
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


    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string? search = null, string sort = "AdmissionDate", string sortDir = "desc", int? collegeId = null, int? programId = null, DateTime? fromDate = null, DateTime? toDate = null, string? status = null)
    {
        var items = await admissionService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir, collegeId, programId, fromDate, toDate, status);

        var sb = new StringBuilder();
        sb.AppendLine("S.N.,Student Name,College Roll No.,College,Program,Admission Date,Status,Active");

        int sn = 1;
        foreach (var a in items)
        {
            var studentName = string.Join(" ", new[] { a.FirstName, a.MiddleName, a.LastName }.Where(p => !string.IsNullOrWhiteSpace(p)));
            sb.AppendLine($"{sn++}," +
                          $"{(studentName).EscapeCsv()}," +
                          $"{a.CollegeRollNumber.EscapeCsv()}," +
                          $"{a.College?.Name.EscapeCsv()}," +
                          $"{a.Program?.ProgramName.EscapeCsv()}," +
                          $"{a.AdmissionDate:yyyy-MM-dd}," +
                          $"{(a.IsCompleted ? "Completed" : "Pending")}," +
                          $"{(a.IsActive ? "Active" : "Inactive")}");
        }

        var fileName = $"StudentAdmissions_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(csvBytes, "text/csv", fileName);
    }

    public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string? search = null, string sort = "AdmissionDate", string sortDir = "desc", int? collegeId = null, int? programId = null, DateTime? fromDate = null, DateTime? toDate = null, string? status = null)
    {
        var items = await admissionService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir, collegeId, programId, fromDate, toDate, status);

        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalCount = (await admissionService.GetAdmissionsAsync(page, pageSize, search, sort, sortDir, collegeId, programId, fromDate, toDate, status)).TotalCount;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;

        return View("PrintPdf", items);
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string? search = null, string sort = "AdmissionDate", string sortDir = "desc", int? collegeId = null, int? programId = null, DateTime? fromDate = null, DateTime? toDate = null, string? status = null)
    {
        var items = await admissionService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir, collegeId, programId, fromDate, toDate, status);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("StudentAdmissions");

        var headers = new[] { "S.N.", "Student Name", "College Roll No.", "College", "Program", "Admission Date", "Status", "Active" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.Gray;
        }

        int row = 2;
        int sn = 1;
        foreach (var a in items)
        {
            var studentName = string.Join(" ", new[] { a.FirstName, a.MiddleName, a.LastName }.Where(p => !string.IsNullOrWhiteSpace(p)));
            worksheet.Cell(row, 1).Value = sn++;
            worksheet.Cell(row, 2).Value = studentName;
            worksheet.Cell(row, 3).Value = a.CollegeRollNumber ?? "";
            worksheet.Cell(row, 4).Value = a.College?.Name ?? "";
            worksheet.Cell(row, 5).Value = a.Program?.ProgramName ?? "";
            worksheet.Cell(row, 6).Value = a.AdmissionDate.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 7).Value = a.IsCompleted ? "Completed" : "Pending";
            worksheet.Cell(row, 8).Value = a.IsActive ? "Active" : "Inactive";
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        var fileName = $"StudentAdmissions_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet]
    public async Task<JsonResult> SearchStudents(string search, int? collegeId)
    {
        if (string.IsNullOrWhiteSpace(search) || search.Length < 2)
            return Json(new List<object>());

        var lowerSearch = search.ToLower();

        var query = context.StudentRegistrations
            .AsNoTracking()
            .Include(s => s.Program)
            .Include(s => s.College)
            .Where(s => s.IsActive)
            .Where(s => s.StudentAdmissionId == null);

        if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
        {
            query = query.Where(s => s.CollegeId == userContext.CollegeId.Value);
        }
        else if (collegeId.HasValue)
        {
            query = query.Where(s => s.CollegeId == collegeId.Value);
        }

        var students = await query
            .Where(s => (s.RegistrationNumber != null && s.RegistrationNumber.ToLower().Contains(lowerSearch))
                     || (s.FirstName != null && s.FirstName.ToLower().Contains(lowerSearch))
                     || (s.LastName != null && s.LastName.ToLower().Contains(lowerSearch))
                     || (s.Email != null && s.Email.ToLower().Contains(lowerSearch)))
            .OrderBy(s => s.RegistrationNumber)
            .Take(20)
            .Select(s => new
            {
                s.Id,
                s.RegistrationNumber,
                s.FirstName,
                s.MiddleName,
                s.LastName,
                s.NepaliName,
                s.DateOfBirthBS,
                s.DateOfBirthAD,
                s.GenderId,
                s.ContactNumber,
                s.Phone,
                s.Email,
                s.CollegeId,
                s.ProgramId,
                s.AcademicYearId,
                CollegeName = s.College != null ? s.College.Name : "",
                ProgramName = s.Program != null ? s.Program.ProgramName : "",
                FullName = (s.FirstName + " " + s.LastName).Trim()
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
        [RequirePermission("studentadmissions.delete")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await admissionService.DeleteAdmissionAsync(id); return Json(new { success = true, message = "Student admission deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}
