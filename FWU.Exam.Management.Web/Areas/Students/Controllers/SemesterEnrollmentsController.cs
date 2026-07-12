using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace FWU.Exam.Management.Web.Areas.Students.Controllers;

[Area("Students")]
[Authorize(Roles = "SuperAdmin,FacultyAdmin,CollegeAdmin")]
public class SemesterEnrollmentsController(ISemesterEnrollmentService enrollmentService, UserManager<AppUser> userManager, AppDbContext context) : Controller
{
    private async Task<List<int>> GetUserCollegeIdsAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return [];

        if (User.IsInRole(Role.SuperAdmin))
            return [];

        if (User.IsInRole(Role.FacultyAdmin) && user.FacultyId != null)
        {
            return await context.Colleges
                .Where(c => c.Faculties.Any(f => f.Id == user.FacultyId))
                .Select(c => c.Id)
                .ToListAsync();
        }

        if (User.IsInRole(Role.CollegeAdmin) && user.CollegeId != null)
            return [user.CollegeId.Value];

        return [];
    }

    public async Task<IActionResult> Index(int page = 1, string search = "", string sort = "EnrolledDate", string sortDir = "desc", int pageSize = 10, int? admissionId = null)
    {
        var (items, totalCount) = await enrollmentService.GetEnrollmentsAsync(page, pageSize, search, sort, sortDir, admissionId);

        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.PageSize = pageSize;
        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.SortDir = sortDir;
        ViewBag.AdmissionId = admissionId;

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
        var admissions = await enrollmentService.GetActiveAdmissionsAsync();
        ViewBag.StudentAdmissionId = new SelectList(admissions.Select(a => new
        {
            a.Id,
            DisplayName = $"{a.CollegeRollNumber} - {a.Program?.ProgramName} ({a.College?.Name})"
        }), "Id", "DisplayName", studentAdmissionId);

        ViewBag.SemesterId = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
        ViewBag.EnrollmentTypeList = new SelectList(Enum.GetValues<EnrollmentType>());
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
                ViewBag.SemesterId = new SelectList(semesters, "Id", "Name");
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

        var admissions = await enrollmentService.GetActiveAdmissionsAsync();
        ViewBag.StudentAdmissionId = new SelectList(admissions.Select(a => new
        {
            a.Id,
            DisplayName = $"{a.CollegeRollNumber} - {a.Program?.ProgramName} ({a.College?.Name})"
        }), "Id", "DisplayName", enrollment.StudentAdmissionId);

        var semesters = await enrollmentService.GetSemestersByProgramAsync(
            admissions.FirstOrDefault(a => a.Id == enrollment.StudentAdmissionId)?.ProgramsId ?? 0);
        ViewBag.SemesterId = new SelectList(semesters, "Id", "Name", enrollment.SemesterId);
        ViewBag.EnrollmentTypeList = new SelectList(Enum.GetValues<EnrollmentType>(), enrollment.EnrollmentType);

        return View(enrollment);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var enrollment = await enrollmentService.GetEnrollmentByIdAsync(id.Value);
        if (enrollment == null) return NotFound();

        var admissions = await enrollmentService.GetActiveAdmissionsAsync();
        ViewBag.StudentAdmissionId = new SelectList(admissions.Select(a => new
        {
            a.Id,
            DisplayName = $"{a.CollegeRollNumber} - {a.Program?.ProgramName} ({a.College?.Name})"
        }), "Id", "DisplayName", enrollment.StudentAdmissionId);

        var admission = await context.StudentAdmissions.AsNoTracking().FirstOrDefaultAsync(a => a.Id == enrollment.StudentAdmissionId);
        var semesters = admission != null
            ? await enrollmentService.GetSemestersByProgramAsync(admission.ProgramsId)
            : await context.Semesters.AsNoTracking().ToListAsync();
        ViewBag.SemesterId = new SelectList(semesters, "Id", "Name", enrollment.SemesterId);
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
        await enrollmentService.DeleteEnrollmentAsync(id);
        TempData["SuccessMessage"] = "Semester enrollment deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<JsonResult> GetSemestersByAdmission(int admissionId)
    {
        var admission = await context.StudentAdmissions
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == admissionId);

        if (admission == null) return Json(new List<object>());

        var semesters = await enrollmentService.GetSemestersByProgramAsync(admission.ProgramsId);
        return Json(semesters.Select(s => new { id = s.Id, name = s.Name }));
    }

    public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = "", string sort = "EnrolledDate", string sortDir = "desc", int? admissionId = null)
    {
        var items = await enrollmentService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir, admissionId);

        var sb = new StringBuilder();
        sb.AppendLine("S.N.,Admission,Semester,Status,Type,Payment,Total Fee,Total Credits,Result");
        int sn = 1;
        foreach (var e in items)
        {
            sb.AppendLine($"{sn++},{e.StudentAdmission?.CollegeRollNumber},{e.Semester?.Name},{e.EnrollmentStatus},{e.EnrollmentType},{e.PaymentStatus},{e.TotalFee},{e.TotalCredits},{e.ResultStatus}");
        }

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"SemesterEnrollments_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> ExportToExcel(int page = 1, int pageSize = 10, string search = "", string sort = "EnrolledDate", string sortDir = "desc", int? admissionId = null)
    {
        var items = await enrollmentService.GetFilteredItemsAsync(page, pageSize, search, sort, sortDir, admissionId);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("SemesterEnrollments");

        var headers = new[] { "S.N.", "Admission", "Semester", "Status", "Type", "Payment", "Total Fee", "Total Credits", "Result" };
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
            worksheet.Cell(row, 2).Value = e.StudentAdmission?.CollegeRollNumber ?? "";
            worksheet.Cell(row, 3).Value = e.Semester?.Name ?? "";
            worksheet.Cell(row, 4).Value = e.EnrollmentStatus.ToString();
            worksheet.Cell(row, 5).Value = e.EnrollmentType.ToString();
            worksheet.Cell(row, 6).Value = e.PaymentStatus.ToString();
            worksheet.Cell(row, 7).Value = e.TotalFee;
            worksheet.Cell(row, 8).Value = e.TotalCredits;
            worksheet.Cell(row, 9).Value = e.ResultStatus.ToString();
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        var fileName = $"SemesterEnrollments_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    private string EscapeCsv(string field)
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