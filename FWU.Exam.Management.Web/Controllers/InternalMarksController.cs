using fwu_examination_management_system.Data;
using fwu_examination_management_system.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace fwu_examination_management_system.Controllers
{
    [Authorize]
    public class InternalMarksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InternalMarksController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpGet]
        public async Task<IActionResult> EntryList()
        {
            var model = await BuildModelAsync(new InternalMarksEntryListViewModel());
            return View(model);
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EntryList(InternalMarksEntryListViewModel model)
        {
            model = await BuildModelAsync(model);
            model.HasSearched = true;
            model.Results = await BuildResultsAsync(model);
            return View(model);
        }

        private async Task<List<InternalMarksEntryResultViewModel>> BuildResultsAsync(InternalMarksEntryListViewModel filter)
        {
            var query = _context.ExamSubjectRegistrationInternals
                .AsNoTracking()
                .Include(x => x.AcademicYear)
                .Include(x => x.SubjectDetail)
                    .ThenInclude(x => x.Program)
                        .ThenInclude(x => x.Level)
                .Include(x => x.StudentProgramYearPart)
                    .ThenInclude(x => x.YearPart)
                .Include(x => x.StudentProgramYearPart)
                    .ThenInclude(x => x.StudentAdmission)
                        .ThenInclude(x => x.College)
                .Include(x => x.StudentProgramYearPart)
                    .ThenInclude(x => x.StudentAdmission)
                        .ThenInclude(x => x.Program)
                .Include(x => x.StudentProgramYearPart)
                    .ThenInclude(x => x.StudentAdmission)
                        .ThenInclude(x => x.StudentRegistration)
                .AsQueryable();

            if (filter.AcademicYearId.HasValue)
                query = query.Where(x => x.EntryAcademicYearId == filter.AcademicYearId.Value);

            if (filter.CollegeId.HasValue)
                query = query.Where(x => x.StudentProgramYearPart.StudentAdmission.CollegeId == filter.CollegeId.Value);

            if (filter.LevelId.HasValue)
                query = query.Where(x => x.StudentProgramYearPart.StudentAdmission.Program.LevelId == filter.LevelId.Value);

            if (filter.ProgramsId.HasValue)
                query = query.Where(x => x.StudentProgramYearPart.StudentAdmission.ProgramsId == filter.ProgramsId.Value);

            if (filter.YearPartId.HasValue)
                query = query.Where(x => x.StudentProgramYearPart.YearPartId == filter.YearPartId.Value);

            if (filter.SubjectDetailId.HasValue)
                query = query.Where(x => x.SubjectDetailId == filter.SubjectDetailId.Value);

            return await query
                .OrderByDescending(x => x.ExamSubjectRegistrationInternalId)
                .Select(x => new InternalMarksEntryResultViewModel
                {
                    ExamSubjectRegistrationInternalId = x.ExamSubjectRegistrationInternalId,
                    AcademicYearName = x.AcademicYear.AcademicYearName,
                    CollegeName = x.StudentProgramYearPart.StudentAdmission.College.CollegeName,
                    LevelName = x.StudentProgramYearPart.StudentAdmission.Program.Level.LevelName,
                    ProgramName = x.StudentProgramYearPart.StudentAdmission.Program.ProgramName,
                    YearPartName = x.StudentProgramYearPart.YearPart.YearPartName,
                    SubjectName = x.SubjectDetail.SubjectName,
                    StudentName = string.Join(" ", new[]
                    {
                        x.StudentProgramYearPart.StudentAdmission.StudentRegistration.FirstName,
                        x.StudentProgramYearPart.StudentAdmission.StudentRegistration.MiddleName,
                        x.StudentProgramYearPart.StudentAdmission.StudentRegistration.LastName
                    }.Where(n => !string.IsNullOrWhiteSpace(n))),
                    ObtainedMarksTheoryInternal = x.ObtainedMarksTheoryInternal,
                    ObtainedMarksPracticalInternal = x.ObtainedMarksPracticalInternal,
                    Remarks = x.Remarks ?? string.Empty,
                    IsActive = x.IsActive,
                    CreatedDate = x.CreatedDate
                })
                .ToListAsync();
        }

        private async Task<InternalMarksEntryListViewModel> BuildModelAsync(InternalMarksEntryListViewModel model)
        {
            model.AcademicYears = [new SelectListItem("All Academic Years", "")];
            model.AcademicYears.AddRange(await _context.AcademicYears
                .AsNoTracking()
                .OrderBy(x => x.AcademicYearName)
                .Select(x => new SelectListItem(x.AcademicYearName, x.AcademicYearId.ToString()))
                .ToListAsync());

            model.Colleges = [new SelectListItem("All Colleges", "")];
            model.Colleges.AddRange(await _context.Colleges
                .AsNoTracking()
                .OrderBy(x => x.CollegeName)
                .Select(x => new SelectListItem(x.CollegeName, x.CollegeId.ToString()))
                .ToListAsync());

            model.Levels = [new SelectListItem("All Levels", "")];
            model.Levels.AddRange(await _context.Levels
                .AsNoTracking()
                .OrderBy(x => x.LevelName)
                .Select(x => new SelectListItem(x.LevelName, x.LevelId.ToString()))
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

            model.Subjects = [new SelectListItem("All Subjects", "")];
            model.Subjects.AddRange(await _context.SubjectDetails
                .AsNoTracking()
                .OrderBy(x => x.SubjectName)
                .Select(x => new SelectListItem(x.SubjectName, x.SubjectDetailId.ToString()))
                .ToListAsync());

            return model;
        }
    }
}
