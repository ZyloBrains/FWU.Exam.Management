using fwu_examination_management_system.Data;
using fwu_examination_management_system.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace fwu_examination_management_system.Controllers
{
    [Authorize]
    public class PrintAdmitCardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrintAdmitCardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await BuildModelAsync(new PrintAdmitCardViewModel());
            return View(model);
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(PrintAdmitCardViewModel model)
        {
            model = await BuildModelAsync(model);
            model.HasSearched = true;
            model.Results = await BuildResultsAsync(model);
            return View(model);
        }

        private async Task<PrintAdmitCardViewModel> BuildModelAsync(PrintAdmitCardViewModel model)
        {
            model.AcademicYears = [new SelectListItem("All Academic Years", "")];
            model.AcademicYears.AddRange(await _context.AcademicYears
                .AsNoTracking()
                .OrderByDescending(x => x.AcademicYearCode)
                .Select(x => new SelectListItem(x.AcademicYearName, x.AcademicYearId.ToString()))
                .ToListAsync());

            model.Colleges = [new SelectListItem("All Colleges", "")];
            model.Colleges.AddRange(await _context.Colleges
                .AsNoTracking()
                .OrderBy(x => x.CollegeName)
                .Select(x => new SelectListItem(x.CollegeName, x.CollegeId.ToString()))
                .ToListAsync());

            model.ExamSchedules = [new SelectListItem("All Exam Schedules", "")];
            model.ExamSchedules.AddRange(await _context.ExamSchedules
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new SelectListItem(x.ExamScheduleName, x.ExamScheduleId.ToString()))
                .ToListAsync());

            model.Programs = [new SelectListItem("All Programs", "")];
            model.Programs.AddRange(await _context.Programs
                .AsNoTracking()
                .OrderBy(x => x.ProgramName)
                .Select(x => new SelectListItem(x.ProgramName, x.ProgramsId.ToString()))
                .ToListAsync());

            model.YearParts = [new SelectListItem("All Year Parts", "")];
            model.YearParts.AddRange(await _context.YearParts
                .AsNoTracking()
                .OrderBy(x => x.YearPartName)
                .Select(x => new SelectListItem(x.YearPartName, x.YearPartId.ToString()))
                .ToListAsync());

            model.ExamTypes = [new SelectListItem("All Exam Types", "")];
            model.ExamTypes.AddRange(await _context.ExamTypes
                .AsNoTracking()
                .OrderBy(x => x.ExamTypeName)
                .Select(x => new SelectListItem(x.ExamTypeName, x.ExamTypeId.ToString()))
                .ToListAsync());

            return model;
        }

        private async Task<List<PrintAdmitCardResultViewModel>> BuildResultsAsync(PrintAdmitCardViewModel filter)
        {
            var query = _context.ExamRegistrations
                .AsNoTracking()
                .Include(x => x.AcademicYear)
                .Include(x => x.College)
                .Include(x => x.ExamSchedule)
                    .ThenInclude(x => x.ExamType)
                .Include(x => x.Program)
                .Include(x => x.StudentProgramYearPart)
                    .ThenInclude(x => x.YearPart)
                .Include(x => x.StudentProgramYearPart)
                    .ThenInclude(x => x.StudentAdmission)
                        .ThenInclude(x => x.StudentRegistration)
                .AsQueryable();

            if (filter.AcademicYearId.HasValue)
                query = query.Where(x => x.AcademicYearId == filter.AcademicYearId.Value);

            if (filter.CollegeId.HasValue)
                query = query.Where(x => x.CollegeId == filter.CollegeId.Value);

            if (filter.ExamScheduleId.HasValue)
                query = query.Where(x => x.ExamScheduleId == filter.ExamScheduleId.Value);

            if (filter.ProgramsId.HasValue)
                query = query.Where(x => x.ProgramsId == filter.ProgramsId.Value);

            if (filter.YearPartId.HasValue)
                query = query.Where(x => x.StudentProgramYearPart.YearPartId == filter.YearPartId.Value);

            if (filter.ExamTypeId.HasValue)
                query = query.Where(x => x.ExamSchedule.ExamTypeId == filter.ExamTypeId.Value);

            if (filter.AppliedByStudentsOnly)
                query = query.Where(x => x.IsAppliedByStudent == true);

            return await query
                .OrderByDescending(x => x.ExamRegistrationId)
                .Select(x => new PrintAdmitCardResultViewModel
                {
                    ExamRegistrationId = x.ExamRegistrationId,
                    StudentName = string.Join(" ", new[]
                    {
                        x.StudentProgramYearPart.StudentAdmission.StudentRegistration.FirstName,
                        x.StudentProgramYearPart.StudentAdmission.StudentRegistration.MiddleName,
                        x.StudentProgramYearPart.StudentAdmission.StudentRegistration.LastName
                    }.Where(n => !string.IsNullOrWhiteSpace(n))),
                    RegistrationNumber = x.StudentProgramYearPart.StudentAdmission.StudentRegistration.RegistrationNumber,
                    ExamRollNumber = x.ExamRollNumber ?? string.Empty,
                    AcademicYearName = x.AcademicYear.AcademicYearName,
                    CollegeName = x.College.CollegeName,
                    ExamScheduleName = x.ExamSchedule.ExamScheduleName,
                    ProgramName = x.Program != null ? x.Program.ProgramName : string.Empty,
                    YearPartName = x.StudentProgramYearPart.YearPart.YearPartName,
                    ExamTypeName = x.ExamSchedule.ExamType.ExamTypeName,
                    IsAppliedByStudent = x.IsAppliedByStudent,
                    IsActive = x.IsActive
                })
                .ToListAsync();
        }
    }
}
