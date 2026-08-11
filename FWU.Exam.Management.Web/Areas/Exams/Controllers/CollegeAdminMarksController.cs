using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("marksentry.view")]
public class CollegeAdminMarksController(
    ICollegeAdminMarksService collegeAdminMarksService) : Controller
{
    public async Task<IActionResult> Dashboard()
    {
        var page = await collegeAdminMarksService.GetInternalMarksPageAsync();
        var model = new MarksEntryWizardViewModel
        {
            Mode = MarksEntryMode.Internal,
            Title = "Internal Marks Entry",
            Subtitle = "Select the exam schedule and subject, then enter internal marks step by step.",
            Icon = "fa-pen-alt",
            ControllerBase = "CollegeAdminMarks",
            SaveAction = "SaveInternalMarks",
            IsSuperAdmin = page.IsSuperAdmin,
            IsFacultyAdmin = page.IsFacultyAdmin,
            IsCollegeAdmin = page.IsCollegeAdmin,
            Faculties = page.Faculties,
            Colleges = page.Colleges
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetFaculties()
    {
        return Json(await collegeAdminMarksService.GetFacultiesAsync());
    }

    [HttpGet]
    public async Task<IActionResult> GetColleges(int? facultyId)
    {
        return Json(await collegeAdminMarksService.GetCollegesAsync(facultyId));
    }

    [HttpGet]
    public async Task<IActionResult> GetAcademicYears(int collegeId)
    {
        return Json(await collegeAdminMarksService.GetAcademicYearsAsync(collegeId));
    }

    [HttpGet]
    public async Task<IActionResult> GetLevels(int collegeId, int academicYearId)
    {
        return Json(await collegeAdminMarksService.GetLevelsAsync(collegeId, academicYearId));
    }

    [HttpGet]
    public async Task<IActionResult> GetExamSchedules(int collegeId, int academicYearId, int levelId)
    {
        return Json(await collegeAdminMarksService.GetExamSchedulesAsync(collegeId, academicYearId, levelId));
    }

    [HttpGet]
    public async Task<IActionResult> GetScheduleDetail(int examScheduleId, int collegeId)
    {
        try
        {
            return Json(await collegeAdminMarksService.GetScheduleDetailAsync(examScheduleId, collegeId));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetSubjects(int examScheduleId, int collegeId)
    {
        try
        {
            return Json(await collegeAdminMarksService.GetSubjectsByScheduleAsync(examScheduleId, collegeId));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetSubjectDetail(int subjectOfferingId, int collegeId)
    {
        try
        {
            return Json(await collegeAdminMarksService.GetSubjectDetailAsync(subjectOfferingId, collegeId));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetStudents(int examScheduleId, int subjectOfferingId, int collegeId)
    {
        try
        {
            return Json(await collegeAdminMarksService.GetStudentsForInternalMarksAsync(examScheduleId, subjectOfferingId, collegeId));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [RequirePermission("marksentry.submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveInternalMarks([FromForm] InternalMarksSaveDto dto)
    {
        try
        {
            var result = await collegeAdminMarksService.SaveInternalMarksAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, message = $"{result.SavedCount} student(s) saved successfully.", savedCount = result.SavedCount });
            }

            return Json(new { success = false, message = $"Saved {result.SavedCount} record(s) with {result.Errors.Count} error(s): {string.Join("; ", result.Errors)}" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
