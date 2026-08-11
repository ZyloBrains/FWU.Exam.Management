using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("theorymarks.view")]
public class TheoryMarksController(ITheoryMarksService theoryMarksService) : Controller
{
    public async Task<IActionResult> Dashboard()
    {
        var page = await theoryMarksService.GetTheoryMarksPageAsync();
        var model = new MarksEntryWizardViewModel
        {
            Mode = MarksEntryMode.Theory,
            Title = "Theory Marks Entry",
            Subtitle = "Select the exam schedule and subject, then enter theory marks step by step.",
            Icon = "fa-pen-alt",
            ControllerBase = "TheoryMarks",
            SaveAction = "SaveTheoryMarks",
            IsSuperAdmin = page.IsSuperAdmin,
            IsFacultyAdmin = page.IsFacultyAdmin,
            Faculties = page.Faculties,
            Colleges = page.Colleges
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetFaculties()
    {
        return Json(await theoryMarksService.GetFacultiesAsync());
    }

    [HttpGet]
    public async Task<IActionResult> GetColleges(int? facultyId)
    {
        return Json(await theoryMarksService.GetCollegesAsync(facultyId));
    }

    [HttpGet]
    public async Task<IActionResult> GetAcademicYears(int collegeId)
    {
        return Json(await theoryMarksService.GetAcademicYearsAsync(collegeId));
    }

    [HttpGet]
    public async Task<IActionResult> GetLevels(int collegeId, int academicYearId)
    {
        return Json(await theoryMarksService.GetLevelsAsync(collegeId, academicYearId));
    }

    [HttpGet]
    public async Task<IActionResult> GetExamSchedules(int collegeId, int academicYearId, int levelId)
    {
        return Json(await theoryMarksService.GetExamSchedulesAsync(collegeId, academicYearId, levelId));
    }

    [HttpGet]
    public async Task<IActionResult> GetScheduleDetail(int examScheduleId, int collegeId)
    {
        try
        {
            return Json(await theoryMarksService.GetScheduleDetailAsync(examScheduleId, collegeId));
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
            return Json(await theoryMarksService.GetSubjectsByScheduleAsync(examScheduleId, collegeId));
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
            return Json(await theoryMarksService.GetSubjectDetailAsync(subjectOfferingId, collegeId));
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
            return Json(await theoryMarksService.GetStudentsForTheoryMarksAsync(examScheduleId, subjectOfferingId, collegeId));
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
    [RequirePermission("theorymarks.submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTheoryMarks([FromForm] TheoryMarksSaveDto dto)
    {
        try
        {
            var result = await theoryMarksService.SaveTheoryMarksAsync(dto);

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
