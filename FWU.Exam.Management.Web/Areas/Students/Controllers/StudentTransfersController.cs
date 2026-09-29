using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
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
    IUserContext userContext,
    AppDbContext context) : Controller
{
    /// <summary>
    /// Colleges this back-office user may target, or null when unrestricted.
    /// <para>
    /// null means "no restriction" and is only ever returned for SuperAdmin. Any other role
    /// that is not recognised, or whose scope cannot be resolved, gets an EMPTY list, which
    /// restricts them to nothing. This is deliberate: an empty list used to be treated as
    /// unrestricted by every caller, so an unrecognised role silently saw every college.
    /// </para>
    /// </summary>
    private async Task<List<int>?> GetUserCollegeIdsAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return new List<int>();

        if (User.IsInRole(Role.SuperAdmin))
            return null;

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
        if (collegeIds != null)
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

        if (collegeIds != null)
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
                AdmissionId = s.StudentAdmission != null ? (int?)s.StudentAdmission.Id : null,
                AdmissionCollege = s.StudentAdmission != null && s.StudentAdmission.College != null ? s.StudentAdmission.College.Name : "",
                AdmissionCollegeId = s.StudentAdmission != null ? (int?)s.StudentAdmission.CollegeId : null,
                AdmissionProgram = s.StudentAdmission != null && s.StudentAdmission.Program != null ? s.StudentAdmission.Program.ProgramName : "",
                AdmissionProgramId = s.StudentAdmission != null ? (int?)s.StudentAdmission.ProgramsId : null,
                AdmissionAcademicYearId = s.StudentAdmission != null ? (int?)s.StudentAdmission.AcademicYearId : null
            })
            .ToListAsync();

        return Json(students);
    }

    [HttpGet]
    public async Task<IActionResult> Transfer(int id)
    {
        var student = await studentRegistrationService.GetStudentRegistrationByIdAsync(id);
        if (student == null) return NotFound();

        // Carried back from a rejected post so a validation failure does not force the
        // staff member to retype the reason they already entered.
        ViewBag.TransferReason = TempData["TransferReason"] as string ?? string.Empty;

        var collegeIds = await GetUserCollegeIdsAsync();

        // Seed the read-only tenant display with the tenant the student is on today, so it
        // reads as "current" rather than as an error before a faculty has been chosen. The
        // id is resolved the same way TenantDefaults.Resolve resolves a faculty's, so the
        // form can compare ids and never trip over a null-vs-named tenant mismatch.
        ViewBag.CurrentTenantId = student.TenantId > 0 ? student.TenantId : TenantDefaults.CentralTenantId;
        ViewBag.CurrentTenantName = await context.Tenants
            .AsNoTracking()
            .Where(t => t.Id == student.TenantId)
            .Select(t => t.Name)
            .FirstOrDefaultAsync() ?? "Central (default)";

        ViewBag.AcademicYears = new SelectList(
            await context.AcademicYears.AsNoTracking().Where(ay => ay.IsActive).OrderByDescending(ay => ay.AcademicYearCode).ToListAsync(),
            "Id", "AcademicYearName");
        ViewBag.Levels = new SelectList(
            await context.Levels.AsNoTracking().Where(l => l.IsActive).OrderBy(l => l.LevelDisplayOrder).ToListAsync(),
            "Id", "LevelName");

        var collegesQuery = context.Colleges.AsNoTracking().Where(c => c.IsActive);
        if (collegeIds != null)
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
    public async Task<IActionResult> Transfer(int id, int levelId, int facultyId, int collegeId, int programId, int academicYearId, int semesterId, string? transferReason)
    {
        var student = await context.StudentRegistrations
            .Include(s => s.StudentAdmission)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return NotFound();

        // The reason is the audit trail for the transfer and is stored on the closed
        // semester enrollments. It is mandatory, so validate it before anything is written
        // and carry the typed text through the redirect rather than making staff retype it.
        TempData["TransferReason"] = transferReason;
        if (string.IsNullOrWhiteSpace(transferReason))
        {
            TempData["ErrorMessage"] = "Please enter a reason for this transfer.";
            return RedirectToAction(nameof(Transfer), new { id });
        }

        if (transferReason.Trim().Length > 500)
        {
            TempData["ErrorMessage"] = "The transfer reason must be 500 characters or fewer.";
            return RedirectToAction(nameof(Transfer), new { id });
        }

        // The target semester is a required choice: it decides which SemesterInstance the
        // student is actually enrolled into, and it is what makes the closed history rows
        // unambiguous. Never guess it.
        if (semesterId <= 0)
        {
            TempData["ErrorMessage"] = "Please select the semester you are transferring the student into.";
            return RedirectToAction(nameof(Transfer), new { id });
        }

        // The faculty drives the rest of the cascade and, through TenantDefaults, the
        // tenant. It is a required choice, not something inferred from the program.
        if (facultyId <= 0)
        {
            TempData["ErrorMessage"] = "Please select the faculty you are transferring the student into.";
            return RedirectToAction(nameof(Transfer), new { id });
        }

        // Scoped exactly as the dropdown was, so the form can never offer a faculty that
        // the POST then rejects, and an out-of-scope faculty reads as "not found" rather
        // than leaking its existence.
        var faculty = await ApplyTransferFacultyScope(context.Faculties.AsNoTracking())
            .Where(f => f.Id == facultyId)
            .Select(f => new { f.Id, f.Name, f.TenantId, TenantName = f.Tenant != null ? f.Tenant.Name : null })
            .FirstOrDefaultAsync();
        if (faculty == null)
        {
            TempData["ErrorMessage"] = "Selected faculty not found.";
            return RedirectToAction(nameof(Transfer), new { id });
        }

        // Program has no tenant of its own, so the faculty's tenant is the tenant the
        // student ends up in. A faculty with no tenant of its own resolves to the central
        // one. This genuinely differs from the student's current tenant in normal use:
        // Engineering is administered by the Engineering Exam Office, so an OCE student
        // moving into Engineering changes tenant, and the closed history rows stay behind
        // in the old tenant. That is the intended outcome, so it is not blocked here.
        var targetTenantId = TenantDefaults.Resolve(faculty.TenantId);
        var targetTenantName = faculty.TenantName ?? "Central (default)";
        var tenantChanged = student.TenantId > 0 && targetTenantId != student.TenantId;

        var program = await context.Programs
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == programId);
        if (program == null)
        {
            TempData["ErrorMessage"] = "Selected program not found.";
            return RedirectToAction(nameof(Transfer), new { id });
        }

        // The program dropdown is driven by faculty, so a mismatched pair means a tampered
        // or stale form. Persisting it would put the student in a faculty the staff did not
        // choose, and would resolve the wrong tenant.
        if (program.FacultyId != facultyId)
        {
            TempData["ErrorMessage"] = "The selected program does not belong to the selected faculty.";
            return RedirectToAction(nameof(Transfer), new { id });
        }

        // The program dropdown is driven by level, so a mismatched pair means a tampered
        // or stale form. Persisting it would point the student at another level's program.
        if (program.LevelId != levelId)
        {
            TempData["ErrorMessage"] = "The selected program does not belong to the selected level.";
            return RedirectToAction(nameof(Transfer), new { id });
        }

        // The college dropdown is driven by the faculty's affiliations, so enforce the same
        // pairing server-side. Must mirror GetCollegesByFaculty exactly, otherwise the form
        // offers a college the POST then rejects.
        var collegeAffiliated = await context.Colleges
            .AsNoTracking()
            .AnyAsync(c => c.Id == collegeId
                           && ((c.CollegeFaculties != null
                                 && c.CollegeFaculties.Any(cf => cf.FacultyId == facultyId))
                                || c.CollegePrograms!.Any(cp => cp.Program != null
                                                                 && cp.Program.FacultyId == facultyId)));
        if (!collegeAffiliated)
        {
            TempData["ErrorMessage"] = "The selected college is not affiliated with the selected faculty.";
            return RedirectToAction(nameof(Transfer), new { id });
        }

        // The program dropdown is also driven by college; enforce the same pairing server-side.
        var collegeProgramExists = await context.CollegePrograms
            .AsNoTracking()
            .AnyAsync(cp => cp.CollegeId == collegeId && cp.ProgramId == programId);
        if (!collegeProgramExists)
        {
            TempData["ErrorMessage"] = "The selected program is not offered by the selected college.";
            return RedirectToAction(nameof(Transfer), new { id });
        }

        // Authorisation was previously UI-only: the college list was scoped but the POST
        // accepted any id. Check the target is actually reachable by this user.
        if (!await CanTransferToTargetAsync(collegeId, programId))
        {
            TempData["ErrorMessage"] = "You are not allowed to transfer a student into that college or program.";
            return RedirectToAction(nameof(Transfer), new { id });
        }

        bool programChanged = student.ProgramId != programId;
        bool collegeChanged = student.CollegeId != collegeId;
        bool academicYearChanged = student.AcademicYearId != academicYearId;
        bool facultyChanged = student.FacultyId != program.FacultyId;

        try
        {
            using var transaction = await context.Database.BeginTransactionAsync();

            if (student.StudentAdmission != null)
            {
                var hasAnyEnrollment = await context.SemesterEnrollments
                    .AnyAsync(se => se.StudentAdmissionId == student.StudentAdmission.Id);
                var recreateEnrollments = programChanged || academicYearChanged || !hasAnyEnrollment;

                // Closing enrollments and re-pointing the exam registrations are no longer
                // mutually exclusive: previously a program change skipped the college
                // cascade entirely, leaving ExamRegistration.CollegeId stale.
                if (recreateEnrollments)
                {
                    var transferred = await semesterEnrollmentService.TransferEnrollmentsAsync(
                        student.StudentAdmission.Id, programId, academicYearId, semesterId, transferReason);
                    if (!transferred)
                    {
                        await transaction.RollbackAsync();
                        TempData["ErrorMessage"] = "Transfer failed: no active semester instance exists for the selected semester, program and academic year. No changes were made.";
                        return RedirectToAction(nameof(Transfer), new { id });
                    }
                }

                // The exam registrations follow their semester: rows on a semester that was
                // just closed are deactivated (which is what removes the student from the
                // previous program), rows on a still-active semester just follow the new
                // college. See UpdateExamRegistrationCollegesAsync for why ProgramsId is
                // left alone on history.
                if (programChanged || collegeChanged)
                    await UpdateExamRegistrationCollegesAsync(student.StudentAdmission.Id, collegeId);
            }

            student.LevelId = levelId;
            student.CollegeId = collegeId;
            student.ProgramId = programId;
            student.FacultyId = program.FacultyId;
            student.AcademicYearId = academicYearId;
            // Stamped explicitly rather than left to TenantSaveChangesInterceptor, which only
            // fills in Added rows. This also repairs a legacy student whose tenant was never
            // set, since TenantId 0 falls through to the faculty's tenant.
            student.TenantId = targetTenantId;

            context.StudentRegistrations.Update(student);
            await context.SaveChangesAsync();

            if (student.StudentAdmission != null)
            {
                var admission = student.StudentAdmission;
                admission.CollegeId = collegeId;
                admission.ProgramsId = programId;
                admission.AcademicYearId = academicYearId;
                admission.TenantId = targetTenantId;

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

            var studentName = $"{student.FirstName} {student.LastName}";
            string message;
            if (programChanged)
            {
                message = $"Student {studentName} transferred to {program.ProgramName}. Prior semester enrollments were closed and kept as history.";
            }
            else if (facultyChanged)
            {
                message = $"Student {studentName} faculty updated to {faculty.Name} (enrollments preserved).";
            }
            else
            {
                message = $"Student {studentName} college updated to {(await context.Colleges.FindAsync(collegeId))?.Name ?? "N/A"} (enrollments preserved).";
            }
            if (tenantChanged)
                message += $" The student now sits under tenant '{targetTenantName}'; their closed history stays in the previous tenant.";
            TempData["SuccessMessage"] = message;
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Transfer failed: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// True when the signed-in back-office user may target the given college/program.
    /// SuperAdmin is unrestricted; CollegeAdmin is limited to its own college;
    /// FacultyAdmin is limited to programs of its faculty.
    /// </summary>
    private async Task<bool> CanTransferToTargetAsync(int collegeId, int programId)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return false;

        if (User.IsInRole(Role.SuperAdmin)) return true;

        if (User.IsInRole(Role.CollegeAdmin) && user.CollegeId != null)
            return user.CollegeId.Value == collegeId;

        if (User.IsInRole(Role.FacultyAdmin) && user.FacultyId != null)
        {
            var program = await context.Programs
                .AsNoTracking()
                .Where(p => p.Id == programId)
                .Select(p => new { p.FacultyId })
                .FirstOrDefaultAsync();
            return program != null && program.FacultyId == user.FacultyId.Value;
        }

        return false;
    }

    /// <summary>
    /// ExamRegistration rows follow the lifecycle of the semester they were taken in.
    /// Covers both ways a registration reaches the table: through its SemesterEnrollment and
    /// through its ApplicationVoucher. ExamRegistration has no StudentRegistrationId of its
    /// own, so voucher linkage is the only fallback for rows predating enrollment linkage.
    /// <para>
    /// Rows on a CLOSED (history) semester are deactivated and otherwise left untouched.
    /// Every exam, marks, roll-number, admit-card, exam-centre and triplicate query filters
    /// on IsActive, so deactivating is what removes the student from the previous program
    /// entirely. Their ProgramsId/CollegeId are deliberately NOT rewritten: they are
    /// denormalised copies and still describe the exam that was actually sat, which is what
    /// reports and historical marks sheets key on. ResultRecord is keyed by registration
    /// number and is untouched, so the marks themselves are never at risk.
    /// </para>
    /// <para>
    /// Rows on a still-ACTIVE semester do follow the student: that is the college-only
    /// change case, where the student keeps the same program and still has to sit the
    /// current semester's exam, at their new college.
    /// </para>
    /// </summary>
    private async Task UpdateExamRegistrationCollegesAsync(int admissionId, int newCollegeId)
    {
        var registrationIds = await context.StudentRegistrations
            .Where(s => s.StudentAdmission != null && s.StudentAdmission.Id == admissionId)
            .Select(s => s.Id)
            .ToListAsync();

        var voucherIds = registrationIds.Count == 0
            ? new List<int>()
            : await context.ApplicationVouchers
                .Where(av => av.StudentRegistrationId != null
                          && registrationIds.Contains(av.StudentRegistrationId.Value))
                .Select(av => av.Id)
                .ToListAsync();

        var examRegistrations = await context.ExamRegistrations
            .Where(er => (er.SemesterEnrollment != null
                          && er.SemesterEnrollment.StudentAdmissionId == admissionId)
                      || (er.ApplicationVoucherId != null
                          && voucherIds.Contains(er.ApplicationVoucherId.Value)))
            .ToListAsync();

        foreach (var er in examRegistrations)
        {
            var isHistorical = er.SemesterEnrollment != null
                && er.SemesterEnrollment.EnrollmentStatus != StudentEnrollmentStatus.Active;

            if (isHistorical)
            {
                er.IsActive = false;
            }
            else
            {
                er.CollegeId = newCollegeId;
            }
        }

        if (examRegistrations.Count > 0)
            await context.SaveChangesAsync();
    }

    /// <summary>
    /// Faculties this user may target, as targets for a transfer.
    /// <para>
    /// Mirrors UserScopeExtensions.ApplyScope for the FacultyAdmin case and widens the
    /// CollegeAdmin case: that helper resolves a CollegeAdmin's faculties purely from
    /// CollegeFaculty, which is near-empty in practice (the dev database has 4 rows, all
    /// for one faculty), so a CollegeAdmin would otherwise see a single faculty. The
    /// widened branch accepts a faculty the college actually teaches, which is the same
    /// union GetCollegesByFaculty uses, so the faculty and college lists always agree.
    /// Unrecognised roles still resolve to nothing.
    /// </para>
    /// </summary>
    private IQueryable<Faculty> ApplyTransferFacultyScope(IQueryable<Faculty> query)
    {
        if (userContext.IsSuperAdmin) return query;

        if (userContext.IsCollegeAdmin && userContext.CollegeId.HasValue)
        {
            var collegeId = userContext.CollegeId.Value;
            return query.Where(f => f.Id == userContext.FacultyId
                                   || (f.CollegeFaculties != null
                                       && f.CollegeFaculties.Any(cf => cf.CollegeId == collegeId))
                                   || context.CollegePrograms.Any(cp => cp.CollegeId == collegeId
                                                                          && cp.Program != null
                                                                          && cp.Program.FacultyId == f.Id));
        }

        if (userContext.FacultyId.HasValue)
            return query.Where(f => f.Id == userContext.FacultyId.Value);

        return query.Where(f => false);
    }

    /// <summary>
    /// Faculties that actually offer a program at the given level, restricted to the
    /// faculties this user may manage. Faculty has no level of its own, so the level is
    /// reached through Program.
    /// <para>
    /// The response carries the resolved tenant so the form can display it. A faculty
    /// without a tenant of its own resolves to the central tenant, matching the rule in
    /// TenantDefaults.Resolve.
    /// </para>
    /// </summary>
    [HttpGet]
    public async Task<JsonResult> GetFacultiesByLevel(int levelId)
    {
        var facultyIds = context.Programs
            .AsNoTracking()
            .Where(p => p.LevelId == levelId && p.IsActive && p.FacultyId != null)
            .Select(p => p.FacultyId!.Value);

        var faculties = await ApplyTransferFacultyScope(context.Faculties.AsNoTracking())
            .Where(f => facultyIds.Contains(f.Id))
            .OrderBy(f => f.Name)
            .Select(f => new
            {
                f.Id,
                f.Name,
                TenantId = TenantDefaults.Resolve(f.TenantId),
                TenantName = f.Tenant != null ? f.Tenant.Name : "Central (default)"
            })
            .ToListAsync();

        return Json(faculties);
    }

    /// <summary>
    /// Every college affiliated with the selected faculty, even if it offers no program at
    /// the currently selected level.
    /// <para>
    /// Affiliation is recorded in TWO places, and either one counts. CollegeFaculty is the
    /// explicit table, but in practice it is close to empty (the dev database has 4 rows,
    /// all for a single faculty), while CollegeProgram is where the bulk of the real
    /// faculty-to-college relationship lives. Keying this list off CollegeFaculty alone made
    /// the dropdown come back empty for almost every faculty. ApplyScope is still
    /// fail-closed on top of that: an unrecognised role resolves to no colleges.
    /// </para>
    /// </summary>
    [HttpGet]
    public async Task<JsonResult> GetCollegesByFaculty(int facultyId)
    {
        if (facultyId <= 0)
            return Json(new List<SelectOption>());

        var colleges = await context.Colleges
            .ApplyScope(userContext)
            .AsNoTracking()
            .Where(c => c.IsActive
                        && ((c.CollegeFaculties != null
                             && c.CollegeFaculties.Any(cf => cf.FacultyId == facultyId))
                            || c.CollegePrograms!.Any(cp => cp.Program != null
                                                             && cp.Program.FacultyId == facultyId)))
            .OrderBy(c => c.Name)
            .Select(c => new SelectOption { Id = c.Id, Name = c.Name })
            .ToListAsync();
        return Json(colleges);
    }

    [HttpGet]
    public async Task<JsonResult> GetProgramsByCollege(int collegeId, int? levelId = null, int? facultyId = null)
    {
        var collegeIds = await GetUserCollegeIdsAsync();
        if (collegeIds != null && !collegeIds.Contains(collegeId))
            return Json(new List<SelectOption>());

        var query = context.CollegePrograms
            .Where(cp => cp.CollegeId == collegeId && cp.Program != null && cp.Program.ProgramName != null);
        if (levelId.HasValue)
            query = query.Where(cp => cp.Program!.LevelId == levelId.Value);
        if (facultyId.HasValue)
            query = query.Where(cp => cp.Program!.FacultyId == facultyId.Value);

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
    public async Task<JsonResult> GetSemestersForProgram(int programId, int? academicYearId = null)
    {
        // Only offer semesters that actually have a SemesterInstance for the target
        // program + academic year. Previously any active ProgramSemester was offered and
        // the service silently fell back to the lowest semester number, so the user could
        // select Semester 1 and get something they never picked.
        var semesters = await context.ProgramSemesters
            .AsNoTracking()
            .Where(ps => ps.ProgramId == programId
                      && ps.IsActive
                      && ps.Semester != null
                      && (!academicYearId.HasValue
                          || context.SemesterInstances!.Any(si => si.ProgramId == programId
                                                             && si.AcademicYearId == academicYearId.Value
                                                             && si.SemesterId == ps.SemesterId)))
            .OrderBy(ps => ps.Semester!.Number)
            .ThenBy(ps => ps.SemesterId)
            .Select(ps => new SelectOption { Id = ps.SemesterId, Name = ps.Semester!.Name })
            .ToListAsync();
        return Json(semesters);
    }
}
