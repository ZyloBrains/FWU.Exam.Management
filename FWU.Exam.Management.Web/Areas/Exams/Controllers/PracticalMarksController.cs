using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("practicalmarks.view")]
public class PracticalMarksController(IPracticalMarksService practicalMarksService) : Controller
{
    public async Task<IActionResult> Dashboard()
    {
        var page = await practicalMarksService.GetPracticalMarksPageAsync();
        var model = new MarksEntryWizardViewModel
        {
            Mode = MarksEntryMode.Practical,
            Title = "Practical Marks Entry",
            Subtitle = "Select the exam schedule and subject, then enter practical marks step by step.",
            Icon = "flask",
            ControllerBase = "PracticalMarks",
            SaveAction = "SavePracticalMarks",
            IsSuperAdmin = page.IsSuperAdmin,
            IsFacultyAdmin = page.IsFacultyAdmin,
            IsCollegeAdmin = page.IsCollegeAdmin,
            CollegeId = page.CollegeId,
            Faculties = page.Faculties,
            Colleges = page.Colleges
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetFaculties()
    {
        return Json(await practicalMarksService.GetFacultiesAsync());
    }

    [HttpGet]
    public async Task<IActionResult> GetColleges(int? facultyId)
    {
        return Json(await practicalMarksService.GetCollegesAsync(facultyId));
    }

    [HttpGet]
    public async Task<IActionResult> GetAcademicYears(int collegeId)
    {
        return Json(await practicalMarksService.GetAcademicYearsAsync(collegeId));
    }

    [HttpGet]
    public async Task<IActionResult> GetLevels(int collegeId, int academicYearId)
    {
        return Json(await practicalMarksService.GetLevelsAsync(collegeId, academicYearId));
    }

    [HttpGet]
    public async Task<IActionResult> GetExamSchedules(int collegeId, int academicYearId, int levelId)
    {
        return Json(await practicalMarksService.GetExamSchedulesAsync(collegeId, academicYearId, levelId));
    }

    [HttpGet]
    public async Task<IActionResult> GetScheduleDetail(int examScheduleId, int collegeId)
    {
        try
        {
            return Json(await practicalMarksService.GetScheduleDetailAsync(examScheduleId, collegeId));
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
            return Json(await practicalMarksService.GetSubjectsByScheduleAsync(examScheduleId, collegeId));
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
            return Json(await practicalMarksService.GetSubjectDetailAsync(subjectOfferingId, collegeId));
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
            return Json(await practicalMarksService.GetStudentsForPracticalMarksAsync(examScheduleId, subjectOfferingId, collegeId));
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
    [RequirePermission("practicalmarks.submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePracticalMarks([FromForm] PracticalMarksSaveDto dto)
    {
        try
        {
            var result = await practicalMarksService.SavePracticalMarksAsync(dto);

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
