using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using fwu_examination_management_system.Data;
using fwu_examination_management_system.Data.Models.Subjects;

namespace fwu_examination_management_system.Controllers
{
    public class SubjectDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubjectDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: SubjectDetails1 with pagination, search, and sorting
        public async Task<IActionResult> Index(int page = 1, string search = null, string sort = "SubjectCode", string sortDir = "asc", int pageSize = 10)
        {
            var query = _context.SubjectDetails
                .Include(s => s.Program)
                .Include(s => s.SubjectGroup)
                .Include(s => s.SubjectType)
                .Include(s => s.YearPart)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.SubjectCode.Contains(search) ||
                    s.SubjectName.Contains(search) ||
                    s.ShortName.Contains(search) ||
                    s.Remarks.Contains(search) ||
                    (s.Program != null && s.Program.ProgramCode.Contains(search)) ||
                    (s.Program != null && s.Program.ProgramName.Contains(search)) ||
                    (s.SubjectGroup != null && s.SubjectGroup.SubjectGroupName.Contains(search)) ||
                    (s.SubjectType != null && s.SubjectType.SubjectTypeName.Contains(search)) ||
                    (s.YearPart != null && s.YearPart.YearPartName.Contains(search))
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

        private static System.Linq.Expressions.Expression<Func<SubjectDetail, object>> GetSortProperty(string sort)
        {
            return sort.ToLower() switch
            {
                "subjectcode" => s => s.SubjectCode,
                "subjectname" => s => s.SubjectName,
                "shortname" => s => s.ShortName,
                "program" => s => s.Program.ProgramCode,
                "subjectgroup" => s => s.SubjectGroup.SubjectGroupName,
                "subjecttype" => s => s.SubjectType.SubjectTypeName,
                "yearpart" => s => s.YearPart.YearPartName,
                "theoryfullmarks" => s => s.TheoryFullMarks,
                "practicalfullmarks" => s => s.PracticalFullMarks,
                "credithours" => s => s.CreditHours,
                "displayorder" => s => s.DisplayOrder,
                "isactive" => s => s.IsActive,
                "year" => s => s.Year,
                "part" => s => s.Part,
                _ => s => s.SubjectCode
            };
        }

        // Helper to get filtered items for export (with pagination)
        private async Task<(List<SubjectDetail> Items, int TotalCount)> GetFilteredItemsForExport(int page, int pageSize, string search, string sort, string sortDir)
        {
            var query = _context.SubjectDetails
                .Include(s => s.Program)
                .Include(s => s.SubjectGroup)
                .Include(s => s.SubjectType)
                .Include(s => s.YearPart)
                .AsNoTracking();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.SubjectCode.Contains(search) ||
                    s.SubjectName.Contains(search) ||
                    s.ShortName.Contains(search) ||
                    s.Remarks.Contains(search) ||
                    (s.Program != null && s.Program.ProgramCode.Contains(search)) ||
                    (s.Program != null && s.Program.ProgramName.Contains(search)) ||
                    (s.SubjectGroup != null && s.SubjectGroup.SubjectGroupName.Contains(search)) ||
                    (s.SubjectType != null && s.SubjectType.SubjectTypeName.Contains(search)) ||
                    (s.YearPart != null && s.YearPart.YearPartName.Contains(search))
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
        public async Task<IActionResult> ExportToCsv(int page = 1, int pageSize = 10, string search = null, string sort = "SubjectCode", string sortDir = "asc")
        {
            var (items, totalCount) = await GetFilteredItemsForExport(page, pageSize, search, sort, sortDir);

            var sb = new StringBuilder();

            // CSV header
            sb.AppendLine("Subject Code,Subject Name,Short Name,Program,Subject Group,Subject Type,Year Part,Year,Part,Theory Full Marks,Theory Pass Marks,Practical Full Marks,Practical Pass Marks,Internal Theory Full Marks,Internal Theory Pass Marks,Internal Practical Full Marks,Internal Practical Pass Marks,Credit Hours,Has Theory,Has Practical,Has Internal,Is Compulsory,Display Order,Remarks,Status");

            foreach (var s in items)
            {
                sb.AppendLine($"{EscapeCsv(s.SubjectCode)}," +
                              $"{EscapeCsv(s.SubjectName)}," +
                              $"{EscapeCsv(s.ShortName)}," +
                              $"{EscapeCsv(s.Program?.ProgramCode)}," +
                              $"{EscapeCsv(s.SubjectGroup?.SubjectGroupName)}," +
                              $"{EscapeCsv(s.SubjectType?.SubjectTypeName)}," +
                              $"{EscapeCsv(s.YearPart?.YearPartName)}," +
                              $"{s.Year}," +
                              $"{s.Part}," +
                              $"{s.TheoryFullMarks}," +
                              $"{s.TheoryPassMarks}," +
                              $"{s.PracticalFullMarks}," +
                              $"{s.PracticalPassMarks}," +
                              $"{s.InternalTheoryFullMarks}," +
                              $"{s.InternalTheoryPassMarks}," +
                              $"{s.InternalPracticalFullMarks}," +
                              $"{s.InternalPracticalPassMarks}," +
                              $"{s.CreditHours}," +
                              $"{(s.HasTheory ? "Yes" : "No")}," +
                              $"{(s.HasPractical ? "Yes" : "No")}," +
                              $"{(s.HasInternal ? "Yes" : "No")}," +
                              $"{(s.IsCompulsory ? "Yes" : "No")}," +
                              $"{s.DisplayOrder}," +
                              $"{EscapeCsv(s.Remarks)}," +
                              $"{(s.IsActive ? "Active" : "Inactive")}");
            }

            var fileName = $"Subjects_Page{page}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(csvBytes, "text/csv", fileName);
        }

        // Export to PDF (Current Page with pagination)
        public async Task<IActionResult> ExportToPdf(int page = 1, int pageSize = 10, string search = null, string sort = "SubjectCode", string sortDir = "asc")
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

        // GET: SubjectDetails1/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectDetail = await _context.SubjectDetails
                .Include(s => s.Program)
                .Include(s => s.SubjectGroup)
                .Include(s => s.SubjectType)
                .Include(s => s.YearPart)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subjectDetail == null)
            {
                return NotFound();
            }

            return View(subjectDetail);
        }

        // GET: SubjectDetails1/Create
        public IActionResult Create()
        {
            ViewData["ProgramsId"] = new SelectList(_context.Programs, "Id", "ProgramCode", "ProgramName");
            ViewData["SubjectGroupId"] = new SelectList(_context.SubjectGroups, "Id", "SubjectGroupName");
            ViewData["SubjectTypeId"] = new SelectList(_context.SubjectTypes, "Id", "SubjectTypeName");
            ViewData["YearPartId"] = new SelectList(_context.YearParts, "Id", "YearPartName");
            return View();
        }

        // POST: SubjectDetails1/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SubjectGroupId,ProgramsId,YearPartId,SubjectCode,SubjectName,TheoryFullMarks,TheoryPassMarks,PracticalFullMarks,PracticalPassMarks,InternalTheoryFullMarks,InternalTheoryPassMarks,InternalPracticalFullMarks,InternalPracticalPassMarks,CreditHours,HasPractical,HasInternal,DisplayOrder,Remarks,IsActive,IsCompulsory,ShortName,ConcurrentSubjectCode,SubjectTypeId,HasTheory,Year,Part")] SubjectDetail subjectDetail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(subjectDetail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProgramsId"] = new SelectList(_context.Programs, "Id", "ProgramCode", subjectDetail.ProgramsId);
            ViewData["SubjectGroupId"] = new SelectList(_context.SubjectGroups, "Id", "SubjectGroupName", subjectDetail.SubjectGroupId);
            ViewData["SubjectTypeId"] = new SelectList(_context.SubjectTypes, "Id", "SubjectTypeName", subjectDetail.SubjectTypeId);
            ViewData["YearPartId"] = new SelectList(_context.YearParts, "Id", "YearPartName", subjectDetail.YearPartId);
            return View(subjectDetail);
        }

        // GET: SubjectDetails1/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectDetail = await _context.SubjectDetails.FindAsync(id);
            if (subjectDetail == null)
            {
                return NotFound();
            }
            ViewData["ProgramsId"] = new SelectList(_context.Programs, "Id", "ProgramCode", subjectDetail.ProgramsId);
            ViewData["SubjectGroupId"] = new SelectList(_context.SubjectGroups, "Id", "SubjectGroupName", subjectDetail.SubjectGroupId);
            ViewData["SubjectTypeId"] = new SelectList(_context.SubjectTypes, "Id", "SubjectTypeName", subjectDetail.SubjectTypeId);
            ViewData["YearPartId"] = new SelectList(_context.YearParts, "Id", "YearPartName", subjectDetail.YearPartId);
            return View(subjectDetail);
        }

        // POST: SubjectDetails1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SubjectGroupId,ProgramsId,YearPartId,SubjectCode,SubjectName,TheoryFullMarks,TheoryPassMarks,PracticalFullMarks,PracticalPassMarks,InternalTheoryFullMarks,InternalTheoryPassMarks,InternalPracticalFullMarks,InternalPracticalPassMarks,CreditHours,HasPractical,HasInternal,DisplayOrder,Remarks,IsActive,IsCompulsory,ShortName,ConcurrentSubjectCode,SubjectTypeId,HasTheory,Year,Part")] SubjectDetail subjectDetail)
        {
            if (id != subjectDetail.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(subjectDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectDetailExists(subjectDetail.Id))
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
            ViewData["ProgramsId"] = new SelectList(_context.Programs, "Id", "ProgramCode", subjectDetail.ProgramsId);
            ViewData["SubjectGroupId"] = new SelectList(_context.SubjectGroups, "Id", "SubjectGroupName", subjectDetail.SubjectGroupId);
            ViewData["SubjectTypeId"] = new SelectList(_context.SubjectTypes, "Id", "SubjectTypeName", subjectDetail.SubjectTypeId);
            ViewData["YearPartId"] = new SelectList(_context.YearParts, "Id", "YearPartName", subjectDetail.YearPartId);
            return View(subjectDetail);
        }

        // GET: SubjectDetails1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subjectDetail = await _context.SubjectDetails
                .Include(s => s.Program)
                .Include(s => s.SubjectGroup)
                .Include(s => s.SubjectType)
                .Include(s => s.YearPart)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (subjectDetail == null)
            {
                return NotFound();
            }

            return View(subjectDetail);
        }

        // POST: SubjectDetails1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subjectDetail = await _context.SubjectDetails.FindAsync(id);
            if (subjectDetail != null)
            {
                _context.SubjectDetails.Remove(subjectDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SubjectDetailExists(int id)
        {
            return _context.SubjectDetails.Any(e => e.Id == id);
        }
    }
}