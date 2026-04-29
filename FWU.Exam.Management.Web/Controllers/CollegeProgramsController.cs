using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using fwu_examination_management_system.Data;
using fwu_examination_management_system.Data.Models.Colleges;

namespace fwu_examination_management_system.Controllers
{
    public class CollegeProgramsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CollegeProgramsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CollegePrograms1 with pagination, search, and sorting
        public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "Id", string sortDir = "asc", int pageSize = 10)
        {
            var query = _context.CollegePrograms
                .Include(cp => cp.College)
                .Include(cp => cp.Program)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(cp =>
                    cp.College.Code.ToString().Contains(search) ||
                    cp.College.Name.Contains(search) ||
                    cp.Program.ProgramCode.Contains(search) ||
                    cp.Program.ProgramName.Contains(search) ||
                    cp.Remarks.Contains(search) ||
                    cp.NumberOfStudents.ToString().Contains(search)
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

        private static System.Linq.Expressions.Expression<Func<CollegeProgram, object>> GetSortProperty(string sort)
        {
            return sort.ToLower() switch
            {
                "collegecode" => cp => cp.College.Code,
                "collegename" => cp => cp.College.Name,
                "programcode" => cp => cp.Program.ProgramCode,
                "programname" => cp => cp.Program.ProgramName,
                "affiliationdate" => cp => cp.AffiliationDate,
                "numberofstudents" => cp => cp.NumberOfStudents,
                "isactive" => cp => cp.IsActive,
                "remarks" => cp => cp.Remarks,
                _ => cp => cp.Id
            };
        }

        // Helper to get filtered items for export (with pagination)
        private async Task<(List<CollegeProgram> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string search, string sort, string sortDir)
        {
            var query = _context.CollegePrograms
                .Include(cp => cp.College)
                .Include(cp => cp.Program)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(cp =>
                    cp.College.Code.ToString().Contains(search) ||
                    cp.College.Name.Contains(search) ||
                    cp.Program.ProgramCode.Contains(search) ||
                    cp.Program.ProgramName.Contains(search) ||
                    cp.Remarks.Contains(search) ||
                    cp.NumberOfStudents.ToString().Contains(search)
                );
            }

            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortDir.ToLower() == "desc"
                ? query.OrderByDescending(GetSortProperty(sort))
                : query.OrderBy(GetSortProperty(sort));

            // Apply pagination
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        // Helper method to escape CSV fields
        private string EscapeCsv(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
                return $"\"{field.Replace("\"", "\"\"")}\"";
            return field;
        }

        // Export to CSV (Current Page with pagination)
        public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "Id", string sortDir = "asc")
        {
            var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

            var sb = new StringBuilder();

            // CSV header
            sb.AppendLine("College Code,College Name,Program Code,Program Name,Affiliation Date,Number of Students,Remarks,Status");

            foreach (var cp in items)
            {
                sb.AppendLine($"{EscapeCsv(cp.College?.Code.ToString())}," +
                              $"{EscapeCsv(cp.College?.Name)}," +
                              $"{EscapeCsv(cp.Program?.ProgramCode)}," +
                              $"{EscapeCsv(cp.Program?.ProgramName)}," +
                              $"{cp.AffiliationDate?.ToString("yyyy-MM-dd")}," +
                              $"{cp.NumberOfStudents}," +
                              $"{EscapeCsv(cp.Remarks)}," +
                              $"{(cp.IsActive ? "Active" : "Inactive")}");
            }

            var fileName = $"CollegePrograms_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(csvBytes, "text/csv", fileName);
        }

        // Export to PDF (Current Page with pagination)
        public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "Id", string sortDir = "asc")
        {
            var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.Search = search;
            ViewBag.Sort = sort;
            ViewBag.SortDir = sortDir;

            return View("PrintPdf", items);
        }

        // GET: CollegePrograms1/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var collegeProgram = await _context.CollegePrograms
                .Include(cp => cp.College)
                .Include(cp => cp.Program)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (collegeProgram == null)
            {
                return NotFound();
            }

            return View(collegeProgram);
        }

        // GET: CollegePrograms1/Create
        public IActionResult Create()
        {
            ViewData["CollegeId"] = new SelectList(_context.Colleges, "Id", "Name", "Code");
            ViewData["ProgramId"] = new SelectList(_context.Programs, "Id", "ProgramName", "ProgramCode");
            return View();
        }

        // POST: CollegePrograms1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AffiliationDate,NumberOfStudents,Remarks,IsActive,CollegeId,ProgramId")] CollegeProgram collegeProgram)
        {
            if (ModelState.IsValid)
            {
                _context.Add(collegeProgram);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CollegeId"] = new SelectList(_context.Colleges, "Id", "Name", collegeProgram.CollegeId);
            ViewData["ProgramId"] = new SelectList(_context.Programs, "Id", "ProgramName", collegeProgram.ProgramId);
            return View(collegeProgram);
        }

        // GET: CollegePrograms1/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var collegeProgram = await _context.CollegePrograms.FindAsync(id);
            if (collegeProgram == null)
            {
                return NotFound();
            }
            ViewData["CollegeId"] = new SelectList(_context.Colleges, "Id", "Name", collegeProgram.CollegeId);
            ViewData["ProgramId"] = new SelectList(_context.Programs, "Id", "ProgramName", collegeProgram.ProgramId);
            return View(collegeProgram);
        }

        // POST: CollegePrograms1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AffiliationDate,NumberOfStudents,Remarks,IsActive,CollegeId,ProgramId")] CollegeProgram collegeProgram)
        {
            if (id != collegeProgram.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(collegeProgram);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CollegeProgramExists(collegeProgram.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CollegeId"] = new SelectList(_context.Colleges, "Id", "Name", collegeProgram.CollegeId);
            ViewData["ProgramId"] = new SelectList(_context.Programs, "Id", "ProgramName", collegeProgram.ProgramId);
            return View(collegeProgram);
        }

        // GET: CollegePrograms1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var collegeProgram = await _context.CollegePrograms
                .Include(cp => cp.College)
                .Include(cp => cp.Program)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (collegeProgram == null)
            {
                return NotFound();
            }

            return View(collegeProgram);
        }

        // POST: CollegePrograms1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var collegeProgram = await _context.CollegePrograms.FindAsync(id);
            if (collegeProgram != null)
            {
                _context.CollegePrograms.Remove(collegeProgram);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CollegeProgramExists(int id)
        {
            return _context.CollegePrograms.Any(e => e.Id == id);
        }
    }
}