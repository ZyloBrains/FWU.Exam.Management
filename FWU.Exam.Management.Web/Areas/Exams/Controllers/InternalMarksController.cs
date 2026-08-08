using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Areas.Exams.Controllers;

[Area("Exams")]
[RequirePermission("marksentry.view")]
public class InternalMarksController(
    AppDbContext context,
    UserManager<AppUser> userManager,
    IUserContext userContext,
    ICollegeAdminMarksService collegeAdminMarksService) : Controller
{
    public async Task<IActionResult> Create()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var college = userContext.CollegeId.HasValue
            ? await context.Colleges.AsNoTracking().FirstOrDefaultAsync(c => c.Id == userContext.CollegeId.Value)
            : null;

        var currentAcademicYear = await context.AcademicYears
            .AsNoTracking()
            .FirstOrDefaultAsync(ay => ay.IsRunning && ay.IsActive);

        ViewBag.CollegeName = college?.Name ?? "";
        ViewBag.CollegeCode = college?.Code ?? "";
        ViewBag.AcademicYearName = currentAcademicYear?.AcademicYearName ?? "";
        ViewBag.AcademicYearCode = currentAcademicYear?.AcademicYearCode ?? "";
        ViewBag.CurrentAcademicYearId = currentAcademicYear?.Id ?? 0;

        return View();
    }

    [HttpGet]
    public async Task<JsonResult> GetLevels()
    {
        var levels = await context.Levels
            .AsNoTracking()
            .OrderBy(l => l.LevelDisplayOrder ?? 0)
            .ThenBy(l => l.LevelName)
            .ToListAsync();

        return Json(levels.Select(l => new { id = l.Id, levelName = l.LevelName, levelCode = l.LevelCode }));
    }

    [HttpGet]
    public async Task<JsonResult> GetPrograms(int levelId)
    {
        var collegeId = userContext.CollegeId;
        var facultyId = userContext.FacultyId;

        var query = context.CollegePrograms
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(cp => cp.IsActive
                      && cp.Program != null && cp.Program.LevelId == levelId && cp.Program.IsActive);

        if (facultyId.HasValue)
            query = query.Where(cp => cp.Program!.FacultyId == facultyId.Value);

        if (collegeId.HasValue)
            query = query.Where(cp => cp.CollegeId == collegeId.Value);

        var programs = await query
            .Select(cp => new { id = cp.Program!.Id, name = cp.Program.ProgramName })
            .Distinct()
            .OrderBy(p => p.name)
            .ToListAsync();

        return Json(programs);
    }

    [HttpGet]
    public async Task<JsonResult> GetSemesters(int programId)
    {
        var semesters = await context.ProgramSemesters
            .AsNoTracking()
            .Where(ps => ps.ProgramId == programId && ps.IsActive
                      && ps.Semester != null)
            .Select(ps => new { id = ps.Semester!.Id, name = ps.Semester.Name, year = ps.Semester.Year, number = ps.Semester.Number })
            .OrderBy(s => s.year).ThenBy(s => s.number)
            .ToListAsync();

        return Json(semesters);
    }

    [HttpGet]
    public async Task<JsonResult> GetExamTypes(int programId, int semesterId)
    {
        var examTypeIds = await context.ExamSchedules
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(es => es.ProgramId == programId && es.SemesterId == semesterId && es.IsActive)
            .Select(es => es.ExamTypeId)
            .Distinct()
            .ToListAsync();

        var examTypes = await context.ExamTypes
            .AsNoTracking()
            .Where(et => examTypeIds.Contains(et.Id) && et.IsActive)
            .Select(et => new { id = et.Id, name = et.Name })
            .OrderBy(et => et.name)
            .ToListAsync();

        return Json(examTypes);
    }

    [HttpGet]
    public async Task<JsonResult> GetSubjects(int programId, int semesterId)
    {
        var subjects = await context.SubjectOfferings
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(so => so.ProgramId == programId && so.SemesterId == semesterId
                      && so.SubjectCatalog != null)
            .Select(so => new
            {
                id = so.Id,
                name = so.SubjectCatalog!.SubjectName,
                code = so.SubjectCatalog.SubjectCode,
                hasInternal = so.HasInternal,
                theoryFullMarks = so.TheoryFullMarks,
                theoryPassMarks = so.TheoryPassMarks,
                practicalFullMarks = so.PracticalFullMarks,
                practicalPassMarks = so.PracticalPassMarks,
                internalTheoryFullMarks = so.InternalTheoryFullMarks,
                internalTheoryPassMarks = so.InternalTheoryPassMarks,
                internalPracticalFullMarks = so.InternalPracticalFullMarks,
                internalPracticalPassMarks = so.InternalPracticalPassMarks,
                hasPractical = so.HasPractical
            })
            .OrderBy(s => s.code)
            .ToListAsync();

        return Json(subjects);
    }

    [HttpGet]
    public async Task<JsonResult> GetExamSchedule(int programId, int semesterId, int examTypeId)
    {
        var collegeId = userContext.CollegeId;
        if (!collegeId.HasValue) return Json(new { found = false });

        var currentAcademicYearId = await context.AcademicYears
            .AsNoTracking()
            .Where(ay => ay.IsRunning && ay.IsActive)
            .Select(ay => ay.Id)
            .FirstOrDefaultAsync();

        var baseQuery = context.ExamSchedules
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(es => es.ProgramId == programId
                      && es.SemesterId == semesterId
                      && es.ExamTypeId == examTypeId
                      && (es.CollegeId == null || es.CollegeId == collegeId.Value)
                      && es.IsActive);

        var schedule = await baseQuery
            .Where(es => currentAcademicYearId == 0 || es.AcademicYearId == currentAcademicYearId)
            .OrderByDescending(es => es.AcademicYearId)
            .Select(es => new
            {
                found = true,
                scheduleId = es.Id,
                scheduleName = es.ExamScheduleName ?? "",
                academicYearId = es.AcademicYearId,
                academicYearName = es.AcademicYear!.AcademicYearName,
                academicYearCode = es.AcademicYear.AcademicYearCode
            })
            .FirstOrDefaultAsync();

        if (schedule == null)
        {
            schedule = await baseQuery
                .OrderByDescending(es => es.AcademicYearId)
                .Select(es => new
                {
                    found = true,
                    scheduleId = es.Id,
                    scheduleName = es.ExamScheduleName ?? "",
                    academicYearId = es.AcademicYearId,
                    academicYearName = es.AcademicYear!.AcademicYearName,
                    academicYearCode = es.AcademicYear.AcademicYearCode
                })
                .FirstOrDefaultAsync();
        }

        if (schedule == null)
            return Json(new { found = false });

        return Json(schedule);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Search(
        int levelId, int programId, int semesterId, int examTypeId,
        int subjectOfferingId, int academicYearId, int examScheduleId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var college = userContext.CollegeId.HasValue
            ? await context.Colleges.AsNoTracking().FirstOrDefaultAsync(c => c.Id == userContext.CollegeId.Value)
            : null;
        var collegeId = userContext.CollegeId;

        var subjectOffering = await context.SubjectOfferings
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(so => so.SubjectCatalog)
            .FirstOrDefaultAsync(so => so.Id == subjectOfferingId);

        var examSchedule = await context.ExamSchedules
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(es => es.AcademicYear)
            .FirstOrDefaultAsync(es => es.Id == examScheduleId);

        if (subjectOffering == null || examSchedule == null)
        {
            TempData["Error"] = "Subject or Exam Schedule not found.";
            return RedirectToAction(nameof(Create));
        }

        var examRegistrationsQuery = context.ExamRegistrations
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(er => er.ApplicationVoucher)
                .ThenInclude(av => av!.StudentRegistration)
            .Where(er => er.ExamScheduleId == examScheduleId
                      && er.IsActive
                      && er.Status != Domain.Enums.RegistrationStatus.Withheld
                      && er.Status != Domain.Enums.RegistrationStatus.Rejected
                      && (er.ProgramsId == null || er.ProgramsId == programId));

        if (collegeId.HasValue)
            examRegistrationsQuery = examRegistrationsQuery.Where(er => er.CollegeId == collegeId.Value);

        var examRegistrations = await examRegistrationsQuery.ToListAsync();

        var erIds = examRegistrations.Select(er => er.Id).ToList();

        var semEnrollments = await context.Set<SemesterEnrollment>()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(se => se.StudentAdmission)
            .Include(se => se.ExamRegistrations)
            .Where(se => se.ExamRegistrations!.Any(er => erIds.Contains(er.Id)))
            .ToListAsync();

        var userIds = semEnrollments
            .Select(se => se.StudentAdmission?.AppUserId)
            .Where(id => id != null).Distinct().Cast<string>().ToList();

        var userNames = new Dictionary<string, string>();
        if (userIds.Count > 0)
        {
            userNames = await context.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, Name = u.FullName ?? u.Email ?? "" })
                .ToDictionaryAsync(u => u.Id, u => u.Name);
        }

        var admissionIds = semEnrollments
            .Where(se => se.StudentAdmission != null)
            .Select(se => se.StudentAdmission!.Id)
            .Distinct().ToList();

        var regNumbers = new Dictionary<int, string>();
        if (admissionIds.Count > 0)
        {
            regNumbers = await context.StudentRegistrations!
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(sr => sr.StudentAdmissionId != null && admissionIds.Contains(sr.StudentAdmissionId!.Value) && sr.RegistrationNumber != null)
                .Select(sr => new { AdmissionId = sr.StudentAdmissionId!.Value, sr.RegistrationNumber })
                .Distinct()
                .GroupBy(x => x.AdmissionId)
                .ToDictionaryAsync(g => g.Key, g => g.First().RegistrationNumber!);
        }

        var existingResults = await context.ExamSubjectResults
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(esr => esr.SubjectOfferingId == subjectOfferingId
                       && esr.ExamScheduleId == examScheduleId)
            .ToListAsync();

        var students = new List<InternalMarksStudentDto>();
        foreach (var er in examRegistrations)
        {
            var se = semEnrollments.FirstOrDefault(s => s.ExamRegistrations!.Any(e => e.Id == er.Id));
            var appUserId = se?.StudentAdmission?.AppUserId;
            var name = appUserId != null && userNames.TryGetValue(appUserId, out var n) ? n : "";
            var regNum = se?.StudentAdmission != null && regNumbers.TryGetValue(se.StudentAdmission.Id, out var rn) ? rn : "";

            if (string.IsNullOrEmpty(name))
                name = er.ApplicationVoucher?.StudentName ?? "";
            if (string.IsNullOrEmpty(regNum))
                regNum = er.ApplicationVoucher?.StudentRegistration?.RegistrationNumber ?? "";

            var existing = existingResults.FirstOrDefault(esr => esr.ExamRegistrationId == er.Id);

            students.Add(new InternalMarksStudentDto
            {
                ExamRegistrationId = er.Id,
                ExamSubjectResultId = existing?.Id,
                StudentName = name,
                RegistrationNumber = regNum,
                SymbolNumber = er.ExamRollNumber ?? er.SymbolNumber ?? "",
                TheoryInternal = existing?.ObtainedMarksTheoryInternal,
                PracticalInternal = existing?.ObtainedMarksPracticalInternal
            });
        }

        var model = new InternalMarksSearchResultDto
        {
            LevelId = levelId,
            ProgramId = programId,
            SemesterId = semesterId,
            ExamTypeId = examTypeId,
            SubjectOfferingId = subjectOfferingId,
            ExamScheduleId = examScheduleId,
            AcademicYearId = academicYearId,
            AcademicYearDisplay = $"{examSchedule.AcademicYear?.AcademicYearCode} ( {examSchedule.AcademicYear?.AcademicYearName} )",
            ExamScheduleDisplay = examSchedule.ExamScheduleName ?? "",
            CollegeDisplay = $"{college?.Name} ( {college?.Code} )",
            SubjectName = subjectOffering.SubjectCatalog?.SubjectName ?? "",
            SubjectCode = subjectOffering.SubjectCatalog?.SubjectCode ?? "",
            HasPractical = subjectOffering.HasPractical,
            HasInternal = subjectOffering.HasInternal,
            InternalTheoryFullMarks = subjectOffering.InternalTheoryFullMarks ?? 0,
            InternalTheoryPassMarks = subjectOffering.InternalTheoryPassMarks ?? 0,
            InternalPracticalFullMarks = subjectOffering.InternalPracticalFullMarks ?? 0,
            InternalPracticalPassMarks = subjectOffering.InternalPracticalPassMarks ?? 0,
            Students = students
        };

        return View("Create", model);
    }

    [HttpPost]
    [RequirePermission("marksentry.submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(InternalMarksSaveDto dto)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var collegeId = userContext.CollegeId;
        if (!collegeId.HasValue)
        {
            TempData["Error"] = "You are not linked to a college.";
            return RedirectToAction(nameof(Create));
        }

        try
        {
            var bulkDto = new BulkMarksSaveDto
            {
                SubjectOfferingId = dto.SubjectOfferingId,
                ExamScheduleId = dto.ExamScheduleId,
                SubmitAll = dto.SubmitAll,
                Students = dto.Students.Select(s => new StudentMarksRowDto
                {
                    ExamRegistrationId = s.ExamRegistrationId,
                    ExamSubjectResultId = s.ExamSubjectResultId,
                    StudentName = "",
                    TheoryInternal = s.TheoryInternal,
                    PracticalInternal = s.PracticalInternal
                }).ToList()
            };

            var result = await collegeAdminMarksService.SaveCollegeMarksBulkAsync(bulkDto, collegeId.Value, user.Id);

            if (result.Success)
                TempData["Success"] = $"{result.SavedCount} student internal marks saved successfully.";
            else
                TempData["Error"] = $"Saved {result.SavedCount} with {result.Errors.Count} errors: {string.Join("; ", result.Errors)}";
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }

        return RedirectToAction(nameof(Create));
    }
}
