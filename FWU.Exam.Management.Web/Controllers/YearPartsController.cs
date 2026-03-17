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
    public class YearPartsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public YearPartsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: YearParts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.YearParts.Include(y => y.ProgramPeriodType);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: YearParts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var yearPart = await _context.YearParts
                .Include(y => y.ProgramPeriodType)
                .FirstOrDefaultAsync(m => m.YearPartId == id);
            if (yearPart == null)
            {
                return NotFound();
            }

            return View(yearPart);
        }

        // GET: YearParts/Create
        public IActionResult Create()
        {
            ViewData["ProgramPeriodTypeId"] = new SelectList(_context.ProgramPeriodTypes, "ProgramPeriodTypeId", "ProgramPeriodTypeName");
            return View();
        }

        // POST: YearParts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("YearPartId,ProgramPeriodTypeId,Year,Part,YearPartName,Remark,IsActive,IsEditable,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate,Code")] YearPart yearPart)
        {
            if (ModelState.IsValid)
            {
                _context.Add(yearPart);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProgramPeriodTypeId"] = new SelectList(_context.ProgramPeriodTypes, "ProgramPeriodTypeId", "ProgramPeriodTypeName", yearPart.ProgramPeriodTypeId);
            return View(yearPart);
        }

        // GET: YearParts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var yearPart = await _context.YearParts.FindAsync(id);
            if (yearPart == null)
            {
                return NotFound();
            }
            ViewData["ProgramPeriodTypeId"] = new SelectList(_context.ProgramPeriodTypes, "ProgramPeriodTypeId", "ProgramPeriodTypeName", yearPart.ProgramPeriodTypeId);
            return View(yearPart);
        }

        // POST: YearParts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("YearPartId,ProgramPeriodTypeId,Year,Part,YearPartName,Remark,IsActive,IsEditable,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate,Code")] YearPart yearPart)
        {
            if (id != yearPart.YearPartId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(yearPart);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!YearPartExists(yearPart.YearPartId))
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
            ViewData["ProgramPeriodTypeId"] = new SelectList(_context.ProgramPeriodTypes, "ProgramPeriodTypeId", "ProgramPeriodTypeName", yearPart.ProgramPeriodTypeId);
            return View(yearPart);
        }

        // GET: YearParts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var yearPart = await _context.YearParts
                .Include(y => y.ProgramPeriodType)
                .FirstOrDefaultAsync(m => m.YearPartId == id);
            if (yearPart == null)
            {
                return NotFound();
            }

            return View(yearPart);
        }

        // POST: YearParts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var yearPart = await _context.YearParts.FindAsync(id);
            if (yearPart != null)
            {
                _context.YearParts.Remove(yearPart);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool YearPartExists(int id)
        {
            return _context.YearParts.Any(e => e.YearPartId == id);
        }
    }
}
