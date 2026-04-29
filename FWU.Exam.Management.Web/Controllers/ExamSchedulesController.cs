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
        private readonly ApplicationDbContext _context;

        public ExamSchedulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ExamSchedules with pagination, search, and sorting
        public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10)
        {
            var query = _context.ExamSchedules
                .Include(e => e.AcademicYear)
                .Include(e => e.ExamScheduleParent)
                .Include(e => e.ExamType)
                .Include(e => e.Level)
                .Include(e => e.YearPart)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.ExamScheduleName.Contains(search) ||
                    s.ExamScheduleCode.Contains(search) ||
                    s.Remarks.Contains(search) ||
                    s.AcademicYear.AcademicYearName.Contains(search) ||
                    s.Level.LevelName.Contains(search) ||
                    s.YearPart.YearPartName.Contains(search) ||
                    s.ExamType.Name.Contains(search)
                );
            }

            // Apply sorting
            query = sortDir.ToLower() == "desc"
                ? query.OrderByDescending(GetSortProperty(sort))
                : query.OrderBy(GetSortProperty(sort));

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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

        private static System.Linq.Expressions.Expression<Func<ExamSchedule, object>> GetSortProperty(string sort)
        {
            return sort.ToLower() switch
            {
                "name" => e => e.ExamScheduleName,
                "code" => e => e.ExamScheduleCode,
                "startdate" => e => e.StartDateAd,
                "enddate" => e => e.EndDateAd,
                "academicyear" => e => e.AcademicYear.AcademicYearName,
                "level" => e => e.Level.LevelName,
                "examtype" => e => e.ExamType.Name,
                _ => e => e.Id
            };
        }

        // Helper to get filtered items for export
        private async Task<List<ExamSchedule>> GetFilteredItems(string search)
        {
            var query = _context.ExamSchedules
                .Include(e => e.AcademicYear)
                .Include(e => e.ExamScheduleParent)
                .Include(e => e.ExamType)
                .Include(e => e.Level)
                .Include(e => e.YearPart)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.ExamScheduleName.Contains(search) ||
                    s.ExamScheduleCode.Contains(search) ||
                    s.Remarks.Contains(search) ||
                    s.AcademicYear.AcademicYearName.Contains(search) ||
                    s.Level.LevelName.Contains(search) ||
                    s.YearPart.YearPartName.Contains(search) ||
                    s.ExamType.Name.Contains(search)
                );
            }
            return await query.OrderBy(e => e.Id).ToListAsync();
        }

        // Export to CSV (all filtered items)
        public async Task<IActionResult> ExportToCsv(string search = null)
        {
            var items = await GetFilteredItems(search);

            var sb = new StringBuilder();
            sb.AppendLine("ID,Exam Schedule Name,Code,Academic Year,Level,Year Part,Exam Type,Parent Schedule,Start Date (AD),End Date (AD),Start Date (BS),End Date (BS),Published Date,Start Time,End Time,Is Active,Negative Marks,Program IDs,Regular Batch IDs,Partial Batch IDs,Extended Date,Extended Date Charge,College Approval Date,Admission Card Release Date,Remarks");

            foreach (var item in items)
            {
                sb.AppendLine($"{EscapeCsv(item.Id.ToString())}," +
                              $"{EscapeCsv(item.ExamScheduleName)}," +
                              $"{EscapeCsv(item.ExamScheduleCode)}," +
                              $"{EscapeCsv(item.AcademicYear?.AcademicYearName)}," +
                              $"{EscapeCsv(item.Level?.LevelName)}," +
                              $"{EscapeCsv(item.YearPart?.YearPartName)}," +
                              $"{EscapeCsv(item.ExamType?.Name)}," +
                              $"{EscapeCsv(item.ExamScheduleParent?.ExamScheduleParentName)}," +
                              $"{EscapeCsv(item.StartDateAd?.ToString("yyyy-MM-dd"))}," +
                              $"{EscapeCsv(item.EndDateAd?.ToString("yyyy-MM-dd"))}," +
                              $"{EscapeCsv(item.StartDateBs)}," +
                              $"{EscapeCsv(item.EndDateBs)}," +
                              $"{EscapeCsv(item.PublishedDate?.ToString("yyyy-MM-dd"))}," +
                              $"{EscapeCsv(item.StartTime.ToString())}," +
                              $"{EscapeCsv(item.EndTime.ToString())}," +
                              $"{(item.IsActive ? "Yes" : "No")}," +
                              $"{item.NegativeMarks}," +
                              $"{EscapeCsv(item.ProgramIds)}," +
                              $"{EscapeCsv(item.RegularBatchIds)}," +
                              $"{EscapeCsv(item.PartialBatchIds)}," +
                              $"{EscapeCsv(item.ExtendedDate?.ToString("yyyy-MM-dd"))}," +
                              $"{item.ExtendedDateCharge}," +
                              $"{EscapeCsv(item.CollegeApprovalDate?.ToString("yyyy-MM-dd"))}," +
                              $"{EscapeCsv(item.AdmissionCardReleaseDate?.ToString("yyyy-MM-dd"))}," +
                              $"{EscapeCsv(item.Remarks)}");
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
                .Include(e => e.AcademicYear)
                .Include(e => e.ExamScheduleParent)
                .Include(e => e.ExamType)
                .Include(e => e.Level)
                .Include(e => e.YearPart)
                .FirstOrDefaultAsync(m => m.Id == id);

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
        public async Task<IActionResult> Create([Bind("Id,AcademicYearId,LevelId,YearPartId,ExamTypeId,ExamScheduleName,StartDateAd,EndDateAd,StartDateBs,EndDateBs,PublishedDate,StartTime,EndTime,Remarks,IsActive,ExamScheduleParentId,NegativeMarks,ProgramIds,RegularBatchIds,PartialBatchIds,ExtendedDate,ExtendedDateCharge,CollegeApprovalDate,AdmissionCardReleaseDate,ExamScheduleCode")] ExamSchedule examSchedule)
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,AcademicYearId,LevelId,YearPartId,ExamTypeId,ExamScheduleName,StartDateAd,EndDateAd,StartDateBs,EndDateBs,PublishedDate,StartTime,EndTime,Remarks,IsActive,ExamScheduleParentId,NegativeMarks,ProgramIds,RegularBatchIds,PartialBatchIds,ExtendedDate,ExtendedDateCharge,CollegeApprovalDate,AdmissionCardReleaseDate,ExamScheduleCode")] ExamSchedule examSchedule)
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
                .Include(e => e.AcademicYear)
                .Include(e => e.ExamScheduleParent)
                .Include(e => e.ExamType)
                .Include(e => e.Level)
                .Include(e => e.YearPart)
                .FirstOrDefaultAsync(m => m.Id == id);

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
            ViewData["ExamScheduleParentId"] = new SelectList(_context.ExamScheduleParents, "Id", "ExamScheduleParentName", examSchedule?.ExamScheduleParentId);
            ViewData["ExamTypeId"] = new SelectList(_context.ExamTypes, "Id", "Name", examSchedule?.ExamTypeId);
            ViewData["LevelId"] = new SelectList(_context.Levels, "Id", "LevelName", examSchedule?.LevelId);
            ViewData["YearPartId"] = new SelectList(_context.YearParts, "Id", "YearPartName", examSchedule?.YearPartId);
        }
    }
}