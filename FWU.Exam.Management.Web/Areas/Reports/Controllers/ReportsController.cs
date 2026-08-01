using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Web.Authorization;
using FWU.Exam.Management.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Reports.Controllers;

[Area("Reports")]
[RequirePermission("reports.summary")]
public class ReportsController(AppDbContext context) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [RequirePermission("reports.collegepayments")]
    public async Task<IActionResult> CollegePayments(ReportFilterViewModel filter)
    {
        await PopulateSelectLists(filter);
        return View(filter);
    }

    [RequirePermission("reports.subjectcount")]
    public async Task<IActionResult> SubjectCount(ReportFilterViewModel filter)
    {
        await PopulateSelectLists(filter);
        return View(filter);
    }

    [RequirePermission("reports.examtriplicate")]
    public async Task<IActionResult> ExamTriplicate(ReportFilterViewModel filter)
    {
        await PopulateSelectLists(filter);
        return View(filter);
    }

    [RequirePermission("reports.summary")]
    public async Task<IActionResult> Summary(ReportFilterViewModel filter)
    {
        await PopulateSelectLists(filter);
        return View(filter);
    }

    [RequirePermission("reports.tabulationtriplicate")]
    public async Task<IActionResult> TabulationTriplicate(ReportFilterViewModel filter)
    {
        await PopulateSelectLists(filter);
        return View(filter);
    }

    [RequirePermission("reports.programwisestudent")]
    public async Task<IActionResult> ProgramWiseStudent(ReportFilterViewModel filter)
    {
        await PopulateSelectLists(filter);
        return View(filter);
    }

    [RequirePermission("reports.attendanceheet")]
    public async Task<IActionResult> AttendanceSheet(ReportFilterViewModel filter)
    {
        await PopulateSelectLists(filter);
        return View(filter);
    }

    [RequirePermission("reports.marksfoil")]
    public async Task<IActionResult> MarksFoil(ReportFilterViewModel filter)
    {
        await PopulateSelectLists(filter);
        return View(filter);
    }

    [RequirePermission("reports.bankvoucherlist")]
    public async Task<IActionResult> BankVoucherList(ReportFilterViewModel filter)
    {
        await PopulateSelectLists(filter);
        return View(filter);
    }

    private async Task PopulateSelectLists(ReportFilterViewModel filter)
    {
        ViewData["AcademicYearId"] = new SelectList(
            await context.AcademicYears.AsNoTracking()
                .OrderByDescending(a => a.AcademicYearCode)
                .Select(a => new { a.Id, a.AcademicYearCode })
                .ToListAsync(),
            "Id", "AcademicYearCode", filter.AcademicYearId);

        ViewData["ExamScheduleId"] = new SelectList(
            await context.ExamSchedules.AsNoTracking()
                .OrderByDescending(e => e.ExamScheduleCode)
                .Select(e => new { e.Id, e.ExamScheduleName })
                .ToListAsync(),
            "Id", "ExamScheduleName", filter.ExamScheduleId);

        ViewData["ProgramId"] = new SelectList(
            await context.Programs.AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.ProgramName)
                .Select(p => new { p.Id, p.ProgramName })
                .ToListAsync(),
            "Id", "ProgramName", filter.ProgramId);

        ViewData["CollegeId"] = new SelectList(
            await context.Colleges.AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new { c.Id, c.Name })
                .ToListAsync(),
            "Id", "Name", filter.CollegeId);

        ViewData["SemesterId"] = new SelectList(
            await context.Semesters.AsNoTracking()
                .OrderBy(s => s.Code)
                .Select(s => new
                {
                    s.Id,
                    Name = s.Name + " (" + s.Code + " - " + s.AcademicYear!.AcademicYearName + ")"
                })
                .ToListAsync(),
            "Id", "Name", filter.SemesterId);

        ViewData["ExamTypeId"] = new SelectList(
            await context.ExamTypes.AsNoTracking()
                .OrderBy(e => e.Name)
                .Select(e => new { e.Id, e.Name })
                .ToListAsync(),
            "Id", "Name", filter.ExamTypeId);
    }
}
