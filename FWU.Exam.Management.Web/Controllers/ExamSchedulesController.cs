using System.Text;
using fwu_examination_management_system.Data;
using fwu_examination_management_system.Data.Models.Exams;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace fwu_examination_management_system.Controllers
{
    public class ExamSchedulesController : Controller
    {
        private readonly AppDbContext _context;

        public ExamSchedulesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: ExamSchedules with pagination, search, and sorting
        public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10)
        {
            var query = BuildQuery(search, sort, sortDir);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new ExamSchedule
                {
                    Id = e.Id,
                    AcademicYearId = e.AcademicYearId,
                    LevelId = e.LevelId,
                    ExamTypeId = e.ExamTypeId,
                    ExamScheduleName = e.ExamScheduleName,
                    StartDateBs = e.StartDateBs,
                    EndDateBs = e.EndDateBs,
                    PublishedDate = e.PublishedDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Remarks = e.Remarks,
                    IsActive = e.IsActive,
                    ExamScheduleParentId = e.ExamScheduleParentId,
                    ExtendedDate = e.ExtendedDate,
                    ExtendedDateCharge = e.ExtendedDateCharge,
                    CollegeApprovalDate = e.CollegeApprovalDate,
                    AdmissionCardReleaseDate = e.AdmissionCardReleaseDate,
                    ExamScheduleCode = e.ExamScheduleCode,
                    AcademicYear = e.AcademicYear,
                    Level = e.Level,
                    ExamType = e.ExamType
                })
                .ToListAsync();

            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.SortDir = sortDir;

            return View(items);
        }

        private IQueryable<ExamSchedule> BuildQuery(string search, string sort, string sortDir)
        {
            var query = _context.ExamSchedules.AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    (s.ExamScheduleName != null && s.ExamScheduleName.Contains(search)) ||
                    (s.ExamScheduleCode != null && s.ExamScheduleCode.Contains(search)) ||
                    (s.Remarks != null && s.Remarks.Contains(search)) ||
                    (s.AcademicYear != null && s.AcademicYear.AcademicYearName != null && s.AcademicYear.AcademicYearName.Contains(search)) ||
                    (s.Level != null && s.Level.LevelName != null && s.Level.LevelName.Contains(search)) ||
                    (s.ExamType != null && s.ExamType.Name != null && s.ExamType.Name.Contains(search))
                );
            }

            var descending = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
            return sort.ToLower() switch
            {
                "name" => descending ? query.OrderByDescending(e => e.ExamScheduleName) : query.OrderBy(e => e.ExamScheduleName),
                "code" => descending ? query.OrderByDescending(e => e.ExamScheduleCode) : query.OrderBy(e => e.ExamScheduleCode),
                "academicyear" => descending
                    ? query.OrderByDescending(e => e.AcademicYear != null ? e.AcademicYear.AcademicYearName : string.Empty)
                    : query.OrderBy(e => e.AcademicYear != null ? e.AcademicYear.AcademicYearName : string.Empty),
                "level" => descending
                    ? query.OrderByDescending(e => e.Level != null ? e.Level.LevelName : string.Empty)
                    : query.OrderBy(e => e.Level != null ? e.Level.LevelName : string.Empty),
                "examtype" => descending
                    ? query.OrderByDescending(e => e.ExamType != null ? e.ExamType.Name : string.Empty)
                    : query.OrderBy(e => e.ExamType != null ? e.ExamType.Name : string.Empty),
                _ => descending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id)
            };
        }

        // Helper to get filtered items for export
        private async Task<List<ExamSchedule>> GetFilteredItems(string search)
        {
            var query = BuildQuery(search, "Id", "asc");

            return await query
                .Select(e => new ExamSchedule
                {
                    Id = e.Id,
                    AcademicYearId = e.AcademicYearId,
                    LevelId = e.LevelId,
                    ExamTypeId = e.ExamTypeId,
                    ExamScheduleName = e.ExamScheduleName,
                    StartDateBs = e.StartDateBs,
                    EndDateBs = e.EndDateBs,
                    PublishedDate = e.PublishedDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Remarks = e.Remarks,
                    IsActive = e.IsActive,
                    ExamScheduleParentId = e.ExamScheduleParentId,
                    ExtendedDate = e.ExtendedDate,
                    ExtendedDateCharge = e.ExtendedDateCharge,
                    CollegeApprovalDate = e.CollegeApprovalDate,
                    AdmissionCardReleaseDate = e.AdmissionCardReleaseDate,
                    ExamScheduleCode = e.ExamScheduleCode,
                    AcademicYear = e.AcademicYear,
                    Level = e.Level,
                    ExamType = e.ExamType
                })
                .ToListAsync();
        }

        // Export to CSV (all filtered items)
        public async Task<IActionResult> ExportToCsv(string search = null)
        {
            var items = await GetFilteredItems(search);

            var sb = new StringBuilder();
            sb.AppendLine("ID,Exam Schedule Name,Code,Academic Year,Level,Exam Type,Start Date (BS),End Date (BS),Published Date,Start Time,End Time,Is Active,Extended Date,Extended Date Charge,College Approval Date,Admission Card Release Date,Remarks");

            foreach (var item in items)
            {
                sb.AppendLine($"{EscapeCsv(item.Id.ToString())}," +
                              $"{EscapeCsv(item.ExamScheduleName ?? string.Empty)}," +
                              $"{EscapeCsv(item.ExamScheduleCode ?? string.Empty)}," +
                              $"{EscapeCsv(item.AcademicYear?.AcademicYearName ?? string.Empty)}," +
                              $"{EscapeCsv(item.Level?.LevelName ?? string.Empty)}," +
                              $"{EscapeCsv(item.ExamType?.Name ?? string.Empty)}," +
                              $"{EscapeCsv(item.StartDateBs ?? string.Empty)}," +
                              $"{EscapeCsv(item.EndDateBs ?? string.Empty)}," +
                              $"{EscapeCsv(item.PublishedDate?.ToString("yyyy-MM-dd") ?? string.Empty)}," +
                              $"{EscapeCsv(item.StartTime.ToString())}," +
                              $"{EscapeCsv(item.EndTime.ToString())}," +
                              $"{(item.IsActive ? "Yes" : "No")}," +
                              $"{EscapeCsv(item.ExtendedDate?.ToString("yyyy-MM-dd") ?? string.Empty)}," +
                              $"{item.ExtendedDateCharge}," +
                              $"{EscapeCsv(item.CollegeApprovalDate?.ToString("yyyy-MM-dd") ?? string.Empty)}," +
                              $"{EscapeCsv(item.AdmissionCardReleaseDate?.ToString("yyyy-MM-dd") ?? string.Empty)}," +
                              $"{EscapeCsv(item.Remarks ?? string.Empty)}");
            }

            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(csvBytes, "text/csv", "ExamSchedules.csv");
        }

        // Export to PDF via browser print view
        public async Task<IActionResult> ExportToPdf(string search = null)
        {
            var items = await GetFilteredItems(search);
            return View("PrintPdf", items);
        }

        private string EscapeCsv(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
                return $"\"{field.Replace("\"", "\"\"")}\"";
            return field;
        }

        // GET: ExamSchedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var examSchedule = await _context.ExamSchedules
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new ExamSchedule
                {
                    Id = e.Id,
                    AcademicYearId = e.AcademicYearId,
                    LevelId = e.LevelId,
                    ExamTypeId = e.ExamTypeId,
                    ExamScheduleName = e.ExamScheduleName,
                    StartDateBs = e.StartDateBs,
                    EndDateBs = e.EndDateBs,
                    PublishedDate = e.PublishedDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Remarks = e.Remarks,
                    IsActive = e.IsActive,
                    ExamScheduleParentId = e.ExamScheduleParentId,
                    ExtendedDate = e.ExtendedDate,
                    ExtendedDateCharge = e.ExtendedDateCharge,
                    CollegeApprovalDate = e.CollegeApprovalDate,
                    AdmissionCardReleaseDate = e.AdmissionCardReleaseDate,
                    ExamScheduleCode = e.ExamScheduleCode,
                    AcademicYear = e.AcademicYear,
                    Level = e.Level,
                    ExamType = e.ExamType
                })
                .FirstOrDefaultAsync();

            if (examSchedule == null) return NotFound();

            return View(examSchedule);
        }

        // GET: ExamSchedules/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: ExamSchedules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AcademicYearId,LevelId,ExamTypeId,ExamScheduleName,StartDateBs,EndDateBs,PublishedDate,StartTime,EndTime,Remarks,IsActive,ExamScheduleParentId,ExtendedDate,ExtendedDateCharge,CollegeApprovalDate,AdmissionCardReleaseDate,ExamScheduleCode")] ExamSchedule examSchedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(examSchedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateDropdowns(examSchedule);
            return View(examSchedule);
        }

        // GET: ExamSchedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var examSchedule = await _context.ExamSchedules.FindAsync(id);
            if (examSchedule == null) return NotFound();

            PopulateDropdowns(examSchedule);
            return View(examSchedule);
        }

        // POST: ExamSchedules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AcademicYearId,LevelId,ExamTypeId,ExamScheduleName,StartDateBs,EndDateBs,PublishedDate,StartTime,EndTime,Remarks,IsActive,ExamScheduleParentId,ExtendedDate,ExtendedDateCharge,CollegeApprovalDate,AdmissionCardReleaseDate,ExamScheduleCode")] ExamSchedule examSchedule)
        {
            if (id != examSchedule.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(examSchedule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExamScheduleExists(examSchedule.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateDropdowns(examSchedule);
            return View(examSchedule);
        }

        // GET: ExamSchedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var examSchedule = await _context.ExamSchedules
                .AsNoTracking()
                .Where(e => e.Id == id)
                .Select(e => new ExamSchedule
                {
                    Id = e.Id,
                    AcademicYearId = e.AcademicYearId,
                    LevelId = e.LevelId,
                    ExamTypeId = e.ExamTypeId,
                    ExamScheduleName = e.ExamScheduleName,
                    StartDateBs = e.StartDateBs,
                    EndDateBs = e.EndDateBs,
                    PublishedDate = e.PublishedDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Remarks = e.Remarks,
                    IsActive = e.IsActive,
                    ExamScheduleParentId = e.ExamScheduleParentId,
                    ExtendedDate = e.ExtendedDate,
                    ExtendedDateCharge = e.ExtendedDateCharge,
                    CollegeApprovalDate = e.CollegeApprovalDate,
                    AdmissionCardReleaseDate = e.AdmissionCardReleaseDate,
                    ExamScheduleCode = e.ExamScheduleCode,
                    AcademicYear = e.AcademicYear,
                    Level = e.Level,
                    ExamType = e.ExamType
                })
                .FirstOrDefaultAsync();

            if (examSchedule == null) return NotFound();

            return View(examSchedule);
        }

        // POST: ExamSchedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var examSchedule = await _context.ExamSchedules.FindAsync(id);
            if (examSchedule != null) _context.ExamSchedules.Remove(examSchedule);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExamScheduleExists(int id) => _context.ExamSchedules.Any(e => e.Id == id);

        private void PopulateDropdowns(ExamSchedule examSchedule = null)
        {
            ViewData["AcademicYearId"] = new SelectList(_context.AcademicYears, "Id", "AcademicYearName", examSchedule?.AcademicYearId);
            ViewData["ExamTypeId"] = new SelectList(_context.ExamTypes, "Id", "Name", examSchedule?.ExamTypeId);
            ViewData["LevelId"] = new SelectList(_context.Levels, "Id", "LevelName", examSchedule?.LevelId);
        }
    }
}