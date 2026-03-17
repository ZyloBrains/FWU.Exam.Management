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
    public class ExamSchedulesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExamSchedulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ExamSchedules
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ExamSchedules.Include(e => e.AcademicYear).Include(e => e.ExamScheduleParent).Include(e => e.ExamType).Include(e => e.Level).Include(e => e.YearPart);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ExamSchedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examSchedule = await _context.ExamSchedules
                .Include(e => e.AcademicYear)
                .Include(e => e.ExamScheduleParent)
                .Include(e => e.ExamType)
                .Include(e => e.Level)
                .Include(e => e.YearPart)
                .FirstOrDefaultAsync(m => m.ExamScheduleId == id);
            if (examSchedule == null)
            {
                return NotFound();
            }

            return View(examSchedule);
        }

        // GET: ExamSchedules/Create
        public IActionResult Create()
        {
            ViewData["AcademicYearId"] = new SelectList(_context.AcademicYears, "AcademicYearId", "AcademicYearName");
            ViewData["ExamScheduleParentId"] = new SelectList(_context.ExamScheduleParents, "ExamScheduleParentId", "ExamScheduleParentName");
            ViewData["ExamTypeId"] = new SelectList(_context.ExamTypes, "ExamTypeId", "ExamTypeName");
            ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName");
            ViewData["YearPartId"] = new SelectList(_context.YearParts, "YearPartId", "YearPartName");
            return View();
        }

        // POST: ExamSchedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ExamScheduleId,AcademicYearId,LevelId,YearPartId,ExamTypeId,ExamScheduleName,StartDateAd,EndDateAd,StartDateBs,EndDateBs,PublishedDate,StartTime,EndTime,Remarks,IsActive,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate,ExamScheduleParentId,NegativeMarks,ProgramIds,RegularBatchIds,PartialBatchIds,ExtendedDate,ExtendedDateCharge,CollegeApprovalDate,AdmissionCardReleaseDate,ExamScheduleCode")] ExamSchedule examSchedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(examSchedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AcademicYearId"] = new SelectList(_context.AcademicYears, "AcademicYearId", "AcademicYearName", examSchedule.AcademicYearId);
            ViewData["ExamScheduleParentId"] = new SelectList(_context.ExamScheduleParents, "ExamScheduleParentId", "ExamScheduleParentName", examSchedule.ExamScheduleParentId);
            ViewData["ExamTypeId"] = new SelectList(_context.ExamTypes, "ExamTypeId", "ExamTypeName", examSchedule.ExamTypeId);
            ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName", examSchedule.LevelId);
            ViewData["YearPartId"] = new SelectList(_context.YearParts, "YearPartId", "YearPartName", examSchedule.YearPartId);
            return View(examSchedule);
        }

        // GET: ExamSchedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examSchedule = await _context.ExamSchedules.FindAsync(id);
            if (examSchedule == null)
            {
                return NotFound();
            }
            ViewData["AcademicYearId"] = new SelectList(_context.AcademicYears, "AcademicYearId", "AcademicYearName", examSchedule.AcademicYearId);
            ViewData["ExamScheduleParentId"] = new SelectList(_context.ExamScheduleParents, "ExamScheduleParentId", "ExamScheduleParentName", examSchedule.ExamScheduleParentId);
            ViewData["ExamTypeId"] = new SelectList(_context.ExamTypes, "ExamTypeId", "ExamTypeName", examSchedule.ExamTypeId);
            ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName", examSchedule.LevelId);
            ViewData["YearPartId"] = new SelectList(_context.YearParts, "YearPartId", "YearPartName", examSchedule.YearPartId);
            return View(examSchedule);
        }

        // POST: ExamSchedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ExamScheduleId,AcademicYearId,LevelId,YearPartId,ExamTypeId,ExamScheduleName,StartDateAd,EndDateAd,StartDateBs,EndDateBs,PublishedDate,StartTime,EndTime,Remarks,IsActive,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate,ExamScheduleParentId,NegativeMarks,ProgramIds,RegularBatchIds,PartialBatchIds,ExtendedDate,ExtendedDateCharge,CollegeApprovalDate,AdmissionCardReleaseDate,ExamScheduleCode")] ExamSchedule examSchedule)
        {
            if (id != examSchedule.ExamScheduleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(examSchedule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExamScheduleExists(examSchedule.ExamScheduleId))
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
            ViewData["AcademicYearId"] = new SelectList(_context.AcademicYears, "AcademicYearId", "AcademicYearName", examSchedule.AcademicYearId);
            ViewData["ExamScheduleParentId"] = new SelectList(_context.ExamScheduleParents, "ExamScheduleParentId", "ExamScheduleParentName", examSchedule.ExamScheduleParentId);
            ViewData["ExamTypeId"] = new SelectList(_context.ExamTypes, "ExamTypeId", "ExamTypeName", examSchedule.ExamTypeId);
            ViewData["LevelId"] = new SelectList(_context.Levels, "LevelId", "LevelName", examSchedule.LevelId);
            ViewData["YearPartId"] = new SelectList(_context.YearParts, "YearPartId", "YearPartName", examSchedule.YearPartId);
            return View(examSchedule);
        }

        // GET: ExamSchedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var examSchedule = await _context.ExamSchedules
                .Include(e => e.AcademicYear)
                .Include(e => e.ExamScheduleParent)
                .Include(e => e.ExamType)
                .Include(e => e.Level)
                .Include(e => e.YearPart)
                .FirstOrDefaultAsync(m => m.ExamScheduleId == id);
            if (examSchedule == null)
            {
                return NotFound();
            }

            return View(examSchedule);
        }

        // POST: ExamSchedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var examSchedule = await _context.ExamSchedules.FindAsync(id);
            if (examSchedule != null)
            {
                _context.ExamSchedules.Remove(examSchedule);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExamScheduleExists(int id)
        {
            return _context.ExamSchedules.Any(e => e.ExamScheduleId == id);
        }
    }
}
