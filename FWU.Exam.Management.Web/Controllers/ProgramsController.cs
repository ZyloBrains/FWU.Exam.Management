using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using fwu_examination_management_system.Data;
using fwu_examination_management_system.Models;

namespace fwu_examination_management_system.Controllers
{
    public class ProgramsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProgramsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Programs
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Programs.Include(p => p.Board).Include(p => p.Faculty).Include(p => p.Level).Include(p => p.ProgramPeriodType);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Programs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var programs = await _context.Programs
                .Include(p => p.Board)
                .Include(p => p.Faculty)
                .Include(p => p.Level)
                .Include(p => p.ProgramPeriodType)
                .FirstOrDefaultAsync(m => m.ProgramId == id);
            if (programs == null)
            {
                return NotFound();
            }

            return View(programs);
        }

        // GET: Programs/Create
        public IActionResult Create()
        {
            ViewData["BoardId"] = new SelectList(_context.Boards, "BoardId", "BoardName");
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyCode");
            ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName");
            ViewData["ProgramPeriodTypeId"] = new SelectList(_context.ProgramPeriodTypes, "ProgramPeriodTypeId", "ProgramPeriodTypeName");
            return View();
        }

        // POST: Programs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProgramId,LevelId,FacultyId,BoardId,ProgramPeriodTypeId,ProgramCode,ProgramName,ShortName,Duration,GrandTotalMarks,HasMultipleIntakes,NumberOfSeats,ScholarshipSeats,Remarks,IsActive,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate,RollNumberPrefix")] Programs programs)
        {
            if (ModelState.IsValid)
            {
                _context.Add(programs);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BoardId"] = new SelectList(_context.Boards, "BoardId", "BoardName", programs.BoardId);
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyCode", programs.FacultyId);
            ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName", programs.LevelId);
            ViewData["ProgramPeriodTypeId"] = new SelectList(_context.ProgramPeriodTypes, "ProgramPeriodTypeId", "ProgramPeriodTypeName", programs.ProgramPeriodTypeId);
            return View(programs);
        }

        // GET: Programs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var programs = await _context.Programs.FindAsync(id);
            if (programs == null)
            {
                return NotFound();
            }
            ViewData["BoardId"] = new SelectList(_context.Boards, "BoardId", "BoardName", programs.BoardId);
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyCode", programs.FacultyId);
            ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName", programs.LevelId);
            ViewData["ProgramPeriodTypeId"] = new SelectList(_context.ProgramPeriodTypes, "ProgramPeriodTypeId", "ProgramPeriodTypeName", programs.ProgramPeriodTypeId);
            return View(programs);
        }

        // POST: Programs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProgramId,LevelId,FacultyId,BoardId,ProgramPeriodTypeId,ProgramCode,ProgramName,ShortName,Duration,GrandTotalMarks,HasMultipleIntakes,NumberOfSeats,ScholarshipSeats,Remarks,IsActive,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate,RollNumberPrefix")] Programs programs)
        {
            if (id != programs.ProgramId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(programs);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProgramsExists(programs.ProgramId))
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
            ViewData["BoardId"] = new SelectList(_context.Boards, "BoardId", "BoardName", programs.BoardId);
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "FacultyId", "FacultyCode", programs.FacultyId);
            ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName", programs.LevelId);
            ViewData["ProgramPeriodTypeId"] = new SelectList(_context.ProgramPeriodTypes, "ProgramPeriodTypeId", "ProgramPeriodTypeName", programs.ProgramPeriodTypeId);
            return View(programs);
        }

        // GET: Programs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var programs = await _context.Programs
                .Include(p => p.Board)
                .Include(p => p.Faculty)
                .Include(p => p.Level)
                .Include(p => p.ProgramPeriodType)
                .FirstOrDefaultAsync(m => m.ProgramId == id);
            if (programs == null)
            {
                return NotFound();
            }

            return View(programs);
        }

        // POST: Programs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var programs = await _context.Programs.FindAsync(id);
            if (programs != null)
            {
                _context.Programs.Remove(programs);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProgramsExists(int id)
        {
            return _context.Programs.Any(e => e.ProgramId == id);
        }
    }
}
