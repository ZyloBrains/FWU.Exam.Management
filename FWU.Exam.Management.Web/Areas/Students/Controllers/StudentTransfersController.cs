using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using FWU.Exam.Management.Web.Authorization;

namespace FWU.Exam.Management.Web.Areas.Students.Controllers;

[Area("Students")]
[Authorize(Roles = Role.BackOfficeRoles)]
[RequirePermission("students.edit")]
public class StudentTransfersController(
    IStudentRegistrationService studentRegistrationService,
    ISemesterEnrollmentService semesterEnrollmentService,
    UserManager<AppUser> userManager,
    AppDbContext context) : Controller
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

    public async Task<IActionResult> Index()
    {
        var collegeIds = await GetUserCollegeIdsAsync();
        var isSuperAdmin = User.IsInRole(Role.SuperAdmin);

        ViewBag.AcademicYears = new SelectList(
            await context.AcademicYears.AsNoTracking().Where(ay => ay.IsActive).OrderByDescending(ay => ay.AcademicYearCode).ToListAsync(),
            "Id", "AcademicYearName");
        ViewBag.Levels = new SelectList(
            await context.Levels.AsNoTracking().Where(l => l.IsActive).OrderBy(l => l.LevelDisplayOrder).ToListAsync(),
            "Id", "LevelName");

        var collegesQuery = context.Colleges.AsNoTracking().Where(c => c.IsActive);
        if (!isSuperAdmin && collegeIds.Count > 0)
            collegesQuery = collegesQuery.Where(c => collegeIds.Contains(c.Id));
        ViewBag.Colleges = new SelectList(
            await collegesQuery.OrderBy(c => c.Name).ToListAsync(),
            "Id", "Name");

        ViewBag.IsSuperAdmin = isSuperAdmin;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Search(string searchTerm, int? levelId, int? collegeId, int? programId, int? academicYearId)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Trim().Length < 2)
            return Json(new List<object>());

        var collegeIds = await GetUserCollegeIdsAsync();
        var isSuperAdmin = User.IsInRole(Role.SuperAdmin);
        var term = searchTerm.Trim().ToLower();

        var query = context.StudentRegistrations
            .Include(s => s.AcademicYear)
            .Include(s => s.Level)
            .Include(s => s.Faculty)
            .Include(s => s.Program)
            .Include(s => s.College)
            .Include(s => s.StudentAdmission)
                .ThenInclude(sa => sa!.College)
            .Include(s => s.StudentAdmission)
                .ThenInclude(sa => sa!.Program)
            .AsNoTracking()
            .Where(s =>
                (s.RegistrationNumber != null && s.RegistrationNumber.ToLower().Contains(term)) ||
                (s.FirstName != null && s.FirstName.ToLower().Contains(term)) ||
                (s.LastName != null && s.LastName.ToLower().Contains(term)) ||
                ((s.FirstName + " " + s.LastName).ToLower().Contains(term)));

        if (!isSuperAdmin && collegeIds.Count > 0)
            query = query.Where(s => collegeIds.Contains(s.CollegeId));

        if (levelId.HasValue)
            query = query.Where(s => s.LevelId == levelId.Value);
        if (collegeId.HasValue)
            query = query.Where(s => s.CollegeId == collegeId.Value);
        if (programId.HasValue)
            query = query.Where(s => s.ProgramId == programId.Value);
        if (academicYearId.HasValue)
            query = query.Where(s => s.AcademicYearId == academicYearId.Value);

        var students = await query
            .OrderBy(s => s.RegistrationNumber)
            .Take(20)
            .Select(s => new
            {
                s.Id,
                RegistrationNumber = s.RegistrationNumber ?? "",
                FullName = (s.FirstName + " " + (s.MiddleName != null ? s.MiddleName + " " : "") + s.LastName).Trim(),
                CurrentLevel = s.Level != null ? s.Level.LevelName : "",
                CurrentLevelId = s.LevelId,
                CurrentCollege = s.College != null ? s.College.Name : "",
                CurrentCollegeId = s.CollegeId,
                CurrentFaculty = s.Faculty != null ? s.Faculty.Name : "",
                CurrentFacultyId = s.FacultyId,
                CurrentProgram = s.Program != null ? s.Program.ProgramName : "",
                CurrentProgramId = s.ProgramId,
                CurrentAcademicYear = s.AcademicYear != null ? s.AcademicYear.AcademicYearName : "",
                CurrentAcademicYearId = s.AcademicYearId,
                HasAdmission = s.StudentAdmission != null,
                AdmissionId = (int?)s.StudentAdmission.Id,
                AdmissionCollege = s.StudentAdmission != null && s.StudentAdmission.College != null ? s.StudentAdmission.College.Name : "",
                AdmissionCollegeId = (int?)s.StudentAdmission.CollegeId,
                AdmissionProgram = s.StudentAdmission != null && s.StudentAdmission.Program != null ? s.StudentAdmission.Program.ProgramName : "",
                AdmissionProgramId = (int?)s.StudentAdmission.ProgramsId,
                AdmissionAcademicYearId = (int?)s.StudentAdmission.AcademicYearId
            })
            .ToListAsync();

        return Json(students);
    }

    [HttpGet]
    public async Task<IActionResult> Transfer(int id)
    {
        var student = await studentRegistrationService.GetStudentRegistrationByIdAsync(id);
        if (student == null) return NotFound();

        var collegeIds = await GetUserCollegeIdsAsync();
        var isSuperAdmin = User.IsInRole(Role.SuperAdmin);

        ViewBag.AcademicYears = new SelectList(
            await context.AcademicYears.AsNoTracking().Where(ay => ay.IsActive).OrderByDescending(ay => ay.AcademicYearCode).ToListAsync(),
            "Id", "AcademicYearName");
        ViewBag.Levels = new SelectList(
            await context.Levels.AsNoTracking().Where(l => l.IsActive).OrderBy(l => l.LevelDisplayOrder).ToListAsync(),
            "Id", "LevelName");

        var collegesQuery = context.Colleges.AsNoTracking().Where(c => c.IsActive);
        if (!isSuperAdmin && collegeIds.Count > 0)
            collegesQuery = collegesQuery.Where(c => collegeIds.Contains(c.Id));
        ViewBag.Colleges = new SelectList(
            await collegesQuery.OrderBy(c => c.Name).ToListAsync(),
            "Id", "Name");

        var admission = await context.StudentAdmissions
            .Include(sa => sa.Program)
                .ThenInclude(p => p!.Faculty)
            .Include(sa => sa.College)
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.StudentRegistration != null && sa.StudentRegistration.Id == id);

        if (admission != null)
        {
            var enrollments = await context.SemesterEnrollments
                .Include(se => se.SemesterInstance).ThenInclude(si => si!.Semester)
                .Include(se => se.SemesterInstance).ThenInclude(si => si!.AcademicYear)
                .Include(se => se.ExamRegistrations).ThenInclude(er => er!.ExamSchedule)
                .Where(se => se.StudentAdmissionId == admission.Id)
                .OrderBy(se => se.SemesterInstance!.Semester!.Number)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Enrollments = enrollments;
            ViewBag.AffectedExamCount = enrollments.Sum(e => e.ExamRegistrations?.Count ?? 0);
            ViewBag.AdmissionProgramId = admission.ProgramsId;
            ViewBag.AdmissionCollegeId = admission.CollegeId;
        }

        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transfer(int id, int levelId, int collegeId, int programId, int academicYearId, int? semesterId = null)
    {
        var student = await context.StudentRegistrations
            .Include(s => s.StudentAdmission)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return NotFound();

        var program = await context.Programs.FindAsync(programId);
        if (program == null)
        {
            TempData["ErrorMessage"] = "Selected program not found.";
            return RedirectToAction(nameof(Transfer), new { id });
        }

        bool programChanged = student.ProgramId != programId;
        bool collegeChanged = student.CollegeId != collegeId;
        bool academicYearChanged = student.AcademicYearId != academicYearId;

        try
        {
            using var transaction = await context.Database.BeginTransactionAsync();

            // Validate the target semester has an active instance BEFORE touching
            // the student/admission so a failed transfer never deletes enrollments.
            if (student.StudentAdmission != null)
            {
                var hasAnyEnrollment = await context.SemesterEnrollments
                    .AnyAsync(se => se.StudentAdmissionId == student.StudentAdmission.Id);
                var recreateEnrollments = programChanged || academicYearChanged || !hasAnyEnrollment;
                if (recreateEnrollments)
                {
                    var transferred = await semesterEnrollmentService.TransferEnrollmentsAsync(student.StudentAdmission.Id, programId, academicYearId, semesterId);
                    if (!transferred)
                    {
                        await transaction.RollbackAsync();
                        TempData["ErrorMessage"] = "Transfer failed: no active semester instance exists for the selected semester, program and academic year. No changes were made.";
                        return RedirectToAction(nameof(Transfer), new { id });
                    }
                }
                else if (collegeChanged)
                {
                    await UpdateExamRegistrationCollegesAsync(student.StudentAdmission.Id, collegeId);
                }
            }

            student.LevelId = levelId;
            student.CollegeId = collegeId;
            student.ProgramId = programId;
            student.FacultyId = program.FacultyId;
            student.AcademicYearId = academicYearId;

            context.StudentRegistrations.Update(student);
            await context.SaveChangesAsync();

            if (student.StudentAdmission != null)
            {
                var admission = student.StudentAdmission;
                admission.CollegeId = collegeId;
                admission.ProgramsId = programId;
                admission.AcademicYearId = academicYearId;

                context.StudentAdmissions.Update(admission);
                await context.SaveChangesAsync();
            }

            var user = await userManager.FindByNameAsync(student.RegistrationNumber ?? "");
            if (user != null)
            {
                var needsUpdate = false;
                if (user.CollegeId != collegeId) { user.CollegeId = collegeId; needsUpdate = true; }
                if (user.FacultyId != program.FacultyId) { user.FacultyId = program.FacultyId; needsUpdate = true; }
                if (needsUpdate)
                    await userManager.UpdateAsync(user);
            }

            await transaction.CommitAsync();

            var message = programChanged
                ? $"Student {student.FirstName} {student.LastName} transferred to {program.ProgramName} (new program — enrollments recreated)."
                : $"Student {student.FirstName} {student.LastName} college updated to {(await context.Colleges.FindAsync(collegeId))?.Name ?? "N/A"} (enrollments preserved).";
            TempData["SuccessMessage"] = message;
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Transfer failed: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task UpdateExamRegistrationCollegesAsync(int admissionId, int newCollegeId)
    {
        var examRegistrations = await context.ExamRegistrations
            .Where(er => er.SemesterEnrollment != null &&
                         er.SemesterEnrollment.StudentAdmissionId == admissionId)
            .ToListAsync();

        foreach (var er in examRegistrations)
        {
            er.CollegeId = newCollegeId;
        }

        if (examRegistrations.Count > 0)
            await context.SaveChangesAsync();
    }

    [HttpGet]
    public async Task<JsonResult> GetCollegesByLevel(int levelId)
    {
        var collegeIds = await GetUserCollegeIdsAsync();
        var isSuperAdmin = User.IsInRole(Role.SuperAdmin);

        var query = context.CollegePrograms
            .Where(cp => cp.Program != null && cp.Program.LevelId == levelId && cp.College != null && cp.College.Name != null);

        if (!isSuperAdmin && collegeIds.Count > 0)
            query = query.Where(cp => collegeIds.Contains(cp.CollegeId));

        var colleges = await query
            .Select(cp => new SelectOption { Id = cp.College!.Id, Name = cp.College.Name! })
            .Distinct().AsNoTracking().ToListAsync();
        return Json(colleges);
    }

    [HttpGet]
    public async Task<JsonResult> GetProgramsByCollege(int collegeId, int? levelId = null)
    {
        var query = context.CollegePrograms
            .Where(cp => cp.CollegeId == collegeId && cp.Program != null && cp.Program.ProgramName != null)
            .Include(cp => cp.Program).AsQueryable();
        if (levelId.HasValue)
            query = query.Where(cp => cp.Program!.LevelId == levelId.Value);
        var programs = await query
            .Select(cp => new SelectOption { Id = cp.Program!.Id, Name = cp.Program.ProgramName })
            .AsNoTracking().ToListAsync();
        return Json(programs);
    }

    [HttpGet]
    public async Task<JsonResult> GetProgramsByLevel(int levelId)
    {
        var programs = await context.Programs
            .AsNoTracking()
            .Where(p => p.LevelId == levelId && p.IsActive && p.ProgramName != null)
            .OrderBy(p => p.ProgramName)
            .Select(p => new SelectOption { Id = p.Id, Name = p.ProgramName! })
            .ToListAsync();
        return Json(programs);
    }

    [HttpGet]
    public async Task<JsonResult> GetFacultyByProgram(int programId)
    {
        var faculty = await context.Programs
            .AsNoTracking()
            .Where(p => p.Id == programId)
            .Select(p => new { FacultyName = p.Faculty != null ? p.Faculty.Name : "" })
            .FirstOrDefaultAsync();
        return Json(faculty ?? new { FacultyName = "" });
    }

    [HttpGet]
    public async Task<JsonResult> GetSemestersForProgram(int programId)
    {
        var semesters = await context.ProgramSemesters
            .AsNoTracking()
            .Include(ps => ps.Semester)
            .Where(ps => ps.ProgramId == programId && ps.IsActive && ps.Semester != null)
            .OrderBy(ps => ps.DisplayOrder)
            .Select(ps => new SelectOption { Id = ps.SemesterId, Name = ps.Semester!.Name })
            .ToListAsync();
        return Json(semesters);
    }
}
