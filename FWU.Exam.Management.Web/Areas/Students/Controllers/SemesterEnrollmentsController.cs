using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.Helpers;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace FWU.Exam.Management.Web.Areas.Students.Controllers;

[Area("Students")]
[Authorize(Roles = "SuperAdmin,FacultyAdmin,CollegeAdmin")]
public class SemesterEnrollmentsController(ISemesterEnrollmentService enrollmentService, UserManager<AppUser> userManager, IUserContext userContext, AppDbContext context) : Controller
{
    private static SelectList SemesterSelectList(IEnumerable<Semester> semesters, string? programShortName, int? selectedId = null)
    {
        return new SelectList(
            semesters.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = SemesterDisplayHelper.FormatForProgram(s, programShortName)
            }),
            "Value", "Text", selectedId?.ToString());
    }
    private async Task<List<int>> GetUserCollegeIdsAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return [];

        if (User.IsInRole(Role.SuperAdmin))
            return [];

        if (User.IsInRole(Role.FacultyAdmin) && user.FacultyId != null)
        {
            return await context.CollegePrograms
                .Where(cp => cp.Program != null && cp.Program.FacultyId == user.FacultyId)
                .Select(cp => cp.CollegeId)
                .Distinct()
                .ToListAsync();
        }

        if (User.IsInRole(Role.CollegeAdmin) && user.CollegeId != null)
            return [user.CollegeId.Value];

        return [];
    }

    public async Task<IActionResult> Index(int page = 1, string search = "", string sort = "EnrolledDate", string sortDir = "desc", int pageSize = 10, int? admissionId = null, int? collegeId = null, int? programId = null, int? semesterId = null, int? academicYearId = null)
    {
        var (items, totalCount) = await enrollmentService.GetEnrollmentsAsync(page, pageSize, search, sort, sortDir, admissionId, collegeId, programId, semesterId, academicYearId);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;
        ViewBag.AdmissionId = admissionId;
        ViewBag.CollegeId = collegeId;
        ViewBag.ProgramId = programId;
        ViewBag.SemesterId = semesterId;
        ViewBag.AcademicYearId = academicYearId;

        await PopulateFilterDropdownsAsync(collegeId, programId, semesterId, academicYearId);

        return View(items);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var enrollment = await enrollmentService.GetEnrollmentByIdAsync(id.Value);
        if (enrollment == null) return NotFound();
        return View(enrollment);
    }

    public async Task<IActionResult> Create(int? studentAdmissionId = null)
    {
        ViewBag.StudentAdmissionId = await GetAdmissionSelectListAsync(studentAdmissionId);

        ViewBag.SemesterId = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
        ViewBag.EnrollmentTypeList = new SelectList(Enum.GetValues<EnrollmentType>());
        ViewBag.AcademicYearId = await GetAcademicYearsAsync();
        ViewBag.StudentAdmissionIdValue = studentAdmissionId;

        if (studentAdmissionId.HasValue)
        {
            var admission = await context.StudentAdmissions
                .AsNoTracking()
                .Include(a => a.Program)
                .FirstOrDefaultAsync(a => a.Id == studentAdmissionId.Value);
            if (admission?.Program != null)
            {
                var semesters = await enrollmentService.GetSemestersByProgramAsync(admission.ProgramsId);
                ViewBag.SemesterId = SemesterSelectList(semesters, admission.Program.ShortName);
            }
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SemesterEnrollment enrollment)
    {
        if (ModelState.IsValid)
        {
            await enrollmentService.CreateEnrollmentAsync(enrollment);
            TempData["SuccessMessage"] = "Semester enrollment created successfully!";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.StudentAdmissionId = await GetAdmissionSelectListAsync(enrollment.StudentAdmissionId);
        ViewBag.AcademicYearId = await GetAcademicYearsAsync();

        var selectedAdmission = await context.StudentAdmissions.AsNoTracking().FirstOrDefaultAsync(a => a.Id == enrollment.StudentAdmissionId);
        var semesters = await enrollmentService.GetSemestersByProgramAsync(selectedAdmission?.ProgramsId ?? 0);
        ViewBag.SemesterId = SemesterSelectList(semesters, selectedAdmission?.Program?.ShortName, enrollment.SemesterId);
        ViewBag.EnrollmentTypeList = new SelectList(Enum.GetValues<EnrollmentType>(), enrollment.EnrollmentType);

        return View(enrollment);
    }

    public async Task<IActionResult> BatchCreate(string search = "", int page = 1, int pageSize = 25, int? academicYearId = null, int? collegeId = null, int? programId = null, int? semesterId = null)
    {
        ViewBag.Search = search;
        ViewBag.AcademicYearId = academicYearId;
        ViewBag.CollegeId = collegeId;
        ViewBag.ProgramId = programId;
        ViewBag.SemesterId = semesterId;

        await PopulateFilterDropdownsAsync(collegeId, programId, semesterId, academicYearId);

        if (programId.HasValue)
        {
            var semesters = await enrollmentService.GetSemestersByProgramAsync(programId.Value, academicYearId);
            ViewData["SemesterFilter"] = new SelectList(semesters, "Id", "Name", semesterId);
        }
        else
        {
            ViewData["SemesterFilter"] = new SelectList(Enumerable.Empty<SelectListItem>(), "Id", "Name");
        }

        var allSemestersQuery = context.Semesters.AsNoTracking().ApplyScope(userContext).AsQueryable();
        if (academicYearId.HasValue)
            allSemestersQuery = allSemestersQuery.Where(s => s.AcademicYearId == academicYearId.Value);
        var allSemesters = await allSemestersQuery.OrderBy(s => s.Name).ToListAsync();
        ViewBag.EnrollSemesterList = new SelectList(allSemesters, "Id", "Name", semesterId);

        var (candidates, totalCount) = await enrollmentService.GetEnrollmentCandidatesAsync(search, academicYearId, collegeId, programId, semesterId, page, pageSize);
        var totalPages = pageSize > 0 ? Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize)) : 1;
        if (page > totalPages && totalPages > 0)
        {
            page = totalPages;
            (candidates, totalCount) = await enrollmentService.GetEnrollmentCandidatesAsync(search, academicYearId, collegeId, programId, semesterId, page, pageSize);
        }
        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = totalPages;
        return View(candidates);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BatchCreate(List<int> admissionIds, int semesterId, string enrollAction = "selected", string search = "", int? academicYearId = null, int? collegeId = null, int? programId = null)
    {
        if (semesterId <= 0)
        {
            TempData["ErrorMessage"] = "Please select a semester to enroll students.";
            return RedirectToAction(nameof(BatchCreate), new { search, academicYearId, collegeId, programId });
        }

        if (enrollAction == "all")
        {
            var (created, skipped) = await enrollmentService.BulkCreateAllEnrollmentsAsync(search, academicYearId, collegeId, programId, semesterId);
            TempData["SuccessMessage"] = created > 0
                ? $"{created} matching student(s) enrolled in the selected semester." + (skipped > 0 ? $" {skipped} already enrolled and skipped." : "")
                : (skipped > 0 ? $"All {skipped} matching student(s) were already enrolled in this semester." : "No matching students found to enroll.");
            return RedirectToAction(nameof(BatchCreate), new { search, academicYearId, collegeId, programId, semesterId });
        }

        if (admissionIds == null || admissionIds.Count == 0)
        {
            TempData["ErrorMessage"] = "Please select at least one student to enroll.";
            return RedirectToAction(nameof(BatchCreate), new { search, academicYearId, collegeId, programId, semesterId });
        }

        var (createdSelected, skippedSelected) = await enrollmentService.BulkCreateEnrollmentsAsync(admissionIds, semesterId);
        TempData["SuccessMessage"] = createdSelected > 0
            ? $"{createdSelected} student(s) enrolled in the selected semester." + (skippedSelected > 0 ? $" {skippedSelected} already enrolled and skipped." : "")
            : (skippedSelected > 0 ? $"All {skippedSelected} selected student(s) were already enrolled in this semester." : "No students were enrolled.");

        return RedirectToAction(nameof(BatchCreate), new { search, academicYearId, collegeId, programId, semesterId });
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var enrollment = await enrollmentService.GetEnrollmentByIdAsync(id.Value);
        if (enrollment == null) return NotFound();

        var admission = await context.StudentAdmissions.AsNoTracking().FirstOrDefaultAsync(a => a.Id == enrollment.StudentAdmissionId);
        var semesters = admission != null
            ? await enrollmentService.GetSemestersByProgramAsync(admission.ProgramsId, admission.AcademicYearId)
            : await context.Semesters.AsNoTracking().ApplyScope(userContext).ToListAsync();
        ViewBag.SemesterId = SemesterSelectList(semesters, admission?.Program?.ShortName, enrollment.SemesterId);
        ViewBag.EnrollmentStatusList = new SelectList(Enum.GetValues<StudentEnrollmentStatus>(), enrollment.EnrollmentStatus);
        ViewBag.EnrollmentTypeList = new SelectList(Enum.GetValues<EnrollmentType>(), enrollment.EnrollmentType);
        ViewBag.PaymentStatusList = new SelectList(Enum.GetValues<PaymentStatus>(), enrollment.PaymentStatus);
        ViewBag.ResultStatusList = new SelectList(Enum.GetValues<ResultStatus>(), enrollment.ResultStatus);

        return View(enrollment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SemesterEnrollment enrollment)
    {
        if (id != enrollment.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await enrollmentService.UpdateEnrollmentAsync(enrollment);
                TempData["SuccessMessage"] = "Semester enrollment updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await enrollmentService.EnrollmentExistsAsync(enrollment.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        return View(enrollment);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var enrollment = await enrollmentService.GetEnrollmentByIdAsync(id.Value);
        if (enrollment == null) return NotFound();
        return View(enrollment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await enrollmentService.DeleteEnrollmentAsync(id);
            TempData["SuccessMessage"] = "Semester enrollment deleted successfully!";
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

    [HttpGet]
    public async Task<JsonResult> GetSemestersByAdmission(int admissionId)
    {
        var admission = await context.StudentAdmissions
            .AsNoTracking()
            .Include(a => a.Program)
            .FirstOrDefaultAsync(a => a.Id == admissionId);

        if (admission == null) return Json(new List<object>());

        var semesters = await enrollmentService.GetSemestersByProgramAsync(admission.ProgramsId);
        return Json(semesters.Select(s => new { id = s.Id, name = SemesterDisplayHelper.FormatForProgram(s, admission.Program?.ShortName) }));
    }

    [HttpGet]
    public async Task<JsonResult> SearchAdmissions(string? q = null, int pageSize = 20, int? academicYearId = null)
    {
        var query = context.StudentAdmissions
            .AsNoTracking()
            .Where(sa => sa.IsActive);

        if (academicYearId.HasValue)
            query = query.Where(sa => sa.AcademicYearId == academicYearId.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(sa =>
                (sa.CollegeRollNumber != null && sa.CollegeRollNumber.Contains(q)) ||
                (sa.Program != null && sa.Program.ProgramName.Contains(q)) ||
                (sa.College != null && sa.College.Name.Contains(q)) ||
                context.StudentRegistrations.Any(r =>
                    r.StudentAdmissionId == sa.Id &&
                    ((r.FirstName + (r.MiddleName != null ? " " + r.MiddleName : "") + (r.LastName != null ? " " + r.LastName : "")).Contains(q) ||
                     (r.RegistrationNumber != null && r.RegistrationNumber.Contains(q)))));
        }

        if (!userContext.IsSuperAdmin && userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
            query = query.Where(sa => sa.CollegeId == userContext.CollegeId.Value);

        var data = await query
            .OrderBy(sa => sa.CollegeRollNumber)
            .Take(pageSize)
            .Select(sa => new
            {
                sa.Id,
                sa.CollegeRollNumber,
                ProgramName = sa.Program != null ? sa.Program.ProgramName : null,
                CollegeName = sa.College != null ? sa.College.Name : null,
                StudentName = context.StudentRegistrations
                    .Where(r => r.StudentAdmissionId == sa.Id)
                    .Select(r => r.FirstName + (r.MiddleName != null ? " " + r.MiddleName : "") + (r.LastName != null ? " " + r.LastName : ""))
                    .FirstOrDefault(),
                RegistrationNumber = context.StudentRegistrations
                    .Where(r => r.StudentAdmissionId == sa.Id)
                    .Select(r => r.RegistrationNumber)
                    .FirstOrDefault()
            })
            .ToListAsync();

        var results = data.Select(sa =>
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(sa.StudentName))
                parts.Add(string.IsNullOrWhiteSpace(sa.RegistrationNumber)
                    ? sa.StudentName
                    : $"{sa.StudentName} ({sa.RegistrationNumber})");
            if (!string.IsNullOrWhiteSpace(sa.CollegeRollNumber))
                parts.Add(sa.CollegeRollNumber);
            if (!string.IsNullOrWhiteSpace(sa.ProgramName))
                parts.Add(sa.ProgramName);
            if (!string.IsNullOrWhiteSpace(sa.CollegeName))
                parts.Add($"({sa.CollegeName})");
            return new { id = sa.Id, text = string.Join(" - ", parts) };
        });

        return Json(new { results });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunPromotion()
    {
        var count = await enrollmentService.PromoteCompletedSemestersAsync();
        TempData["SuccessMessage"] = count > 0
            ? $"{count} student(s) promoted to the next semester."
            : "No students were eligible for promotion right now.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = "", string sort = "EnrolledDate", string sortDir = "desc", int? admissionId = null, int? collegeId = null, int? programId = null, int? semesterId = null, int? academicYearId = null)
    {
        var items = await enrollmentService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir, admissionId, collegeId, programId, semesterId, academicYearId);

        var sb = new StringBuilder();
        sb.AppendLine("S.N.,Student Name,Registration No,Roll Number,Program,College,Semester,Academic Year,Status,Type,Payment,Total Fee,Total Credits,Result");
        int sn = 1;
        foreach (var e in items)
        {
            sb.AppendLine($"{sn++},{EscapeCsv(e.StudentName)},{EscapeCsv(e.RegistrationNumber)},{EscapeCsv(e.CollegeRollNumber)},{EscapeCsv(e.ProgramName)},{EscapeCsv(e.CollegeName)},{EscapeCsv(e.SemesterName)},{EscapeCsv(e.AcademicYearName)},{e.EnrollmentStatus},{e.EnrollmentType},{e.PaymentStatus},{e.TotalFee},{e.TotalCredits},{e.ResultStatus}");
        }

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"SemesterEnrollments_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string search = "", string sort = "EnrolledDate", string sortDir = "desc", int? admissionId = null, int? collegeId = null, int? programId = null, int? semesterId = null, int? academicYearId = null)
    {
        var items = await enrollmentService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir, admissionId, collegeId, programId, semesterId, academicYearId);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("SemesterEnrollments");

        var headers = new[] { "S.N.", "Student Name", "Registration No", "Roll Number", "Program", "College", "Semester", "Academic Year", "Status", "Type", "Payment", "Total Fee", "Total Credits", "Result" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.Gray;
        }

        int row = 2;
        int sn = 1;
        foreach (var e in items)
        {
            worksheet.Cell(row, 1).Value = sn++;
            worksheet.Cell(row, 2).Value = e.StudentName ?? "";
            worksheet.Cell(row, 3).Value = e.RegistrationNumber ?? "";
            worksheet.Cell(row, 4).Value = e.CollegeRollNumber ?? "";
            worksheet.Cell(row, 5).Value = e.ProgramName ?? "";
            worksheet.Cell(row, 6).Value = e.CollegeName ?? "";
            worksheet.Cell(row, 7).Value = e.SemesterName ?? "";
            worksheet.Cell(row, 8).Value = e.AcademicYearName ?? "";
            worksheet.Cell(row, 9).Value = e.EnrollmentStatus.ToString();
            worksheet.Cell(row, 10).Value = e.EnrollmentType.ToString();
            worksheet.Cell(row, 11).Value = e.PaymentStatus.ToString();
            worksheet.Cell(row, 12).Value = e.TotalFee;
            worksheet.Cell(row, 13).Value = e.TotalCredits;
            worksheet.Cell(row, 14).Value = e.ResultStatus.ToString();
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        var fileName = $"SemesterEnrollments_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private async Task PopulateFilterDropdownsAsync(int? collegeId, int? programId, int? semesterId, int? academicYearId = null)
    {
        var collegeQuery = context.Colleges.AsNoTracking().AsQueryable();
        var programQuery = context.Programs.AsNoTracking().AsQueryable();

        if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
        {
            var cid = userContext.CollegeId.Value;
            collegeQuery = collegeQuery.Where(c => c.Id == cid);
            programQuery = programQuery.Where(p => context.CollegePrograms.Any(cp => cp.ProgramId == p.Id && cp.CollegeId == cid));
        }
        else if (collegeId.HasValue)
        {
            programQuery = programQuery.Where(p => context.CollegePrograms.Any(cp => cp.ProgramId == p.Id && cp.CollegeId == collegeId.Value));
        }

        ViewData["CollegeFilter"] = new SelectList(
            await collegeQuery.OrderBy(c => c.Name).Select(c => new { c.Id, c.Name }).ToListAsync(), "Id", "Name", collegeId);
        ViewData["ProgramFilter"] = new SelectList(
            await programQuery.OrderBy(p => p.ProgramName).Select(p => new { p.Id, p.ProgramName }).ToListAsync(), "Id", "ProgramName", programId);
        var semesterQuery = context.Semesters.AsNoTracking().AsQueryable();
        if (academicYearId.HasValue)
            semesterQuery = semesterQuery.Where(s => s.AcademicYearId == academicYearId.Value);

        ViewData["SemesterFilter"] = new SelectList(
            await semesterQuery.OrderBy(s => s.Number).Select(s => new { s.Id, s.Name }).ToListAsync(), "Id", "Name", semesterId);
        ViewData["AcademicYearFilter"] = await GetAcademicYearsAsync(academicYearId);
    }

    private async Task<SelectList> GetAdmissionSelectListAsync(int? selectedId)
    {
        if (selectedId.HasValue)
        {
            var admission = await context.StudentAdmissions
                .Include(a => a.Program)
                .Include(a => a.College)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == selectedId.Value);
            if (admission != null)
            {
                var registration = await context.StudentRegistrations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.StudentAdmissionId == admission.Id);

                var parts = new List<string>();
                var name = string.Join(" ", new[] { registration?.FirstName, registration?.MiddleName, registration?.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
                if (!string.IsNullOrWhiteSpace(name))
                    parts.Add(string.IsNullOrWhiteSpace(registration?.RegistrationNumber)
                        ? name
                        : $"{name} ({registration.RegistrationNumber})");
                if (!string.IsNullOrWhiteSpace(admission.CollegeRollNumber))
                    parts.Add(admission.CollegeRollNumber);
                if (!string.IsNullOrWhiteSpace(admission.Program?.ProgramName))
                    parts.Add(admission.Program.ProgramName);
                if (!string.IsNullOrWhiteSpace(admission.College?.Name))
                    parts.Add($"({admission.College.Name})");

                return new SelectList(new[]
                {
                    new { id = admission.Id, text = string.Join(" - ", parts) }
                }, "id", "text", admission.Id);
            }
        }

        return new SelectList(Enumerable.Empty<object>(), "id", "text");
    }

    private async Task<SelectList> GetAcademicYearsAsync(int? selectedId = null)
    {
        return new SelectList(
            await context.AcademicYears.AsNoTracking()
                .Where(ay => ay.IsActive)
                .OrderByDescending(ay => ay.AcademicYearCode)
                .Select(ay => new { ay.Id, ay.AcademicYearCode })
                .ToListAsync(),
            "Id", "AcademicYearCode", selectedId);
    }

    private string EscapeCsv(string? field)
    {
        if (string.IsNullOrEmpty(field)) return "";
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
        [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAjax(int id)
    {
        try { await enrollmentService.DeleteEnrollmentAsync(id); return Json(new { success = true, message = "Semester enrollment deleted successfully!" }); } catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
    }

}