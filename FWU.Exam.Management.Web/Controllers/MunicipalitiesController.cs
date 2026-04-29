using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using fwu_examination_management_system.Data;
using fwu_examination_management_system.Data.Models;

namespace fwu_examination_management_system.Controllers
{
    public class MunicipalitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MunicipalitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Municipalities
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Municipalities.Include(m => m.District);
           
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Municipalities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var municipality = await _context.Municipalities
                .Include(m => m.District)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (municipality == null)
            {
                return NotFound();
            }

            return View(municipality);
        }

        // GET: Municipalities/Create
        public IActionResult Create()
        {
            ViewData["DistrictId"] = new SelectList(_context.Districts, "Id", "DistrictName");
            ViewData["MunicipalityType"] = new SelectList(Enum.GetValues(typeof(MunicipalityType)).Cast<MunicipalityType>().Select(e => new { Id = e, Name = e.ToString() }), "Id", "Name");
            return View();
        }

        // POST: Municipalities/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DistrictId,MunicipalityName,MunicipalityType,IsActive")] Municipality municipality)
        {
            if (ModelState.IsValid)
            {
                _context.Add(municipality);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DistrictId"] = new SelectList(_context.Districts, "Id", "DistrictName", municipality.DistrictId);
            return View(municipality);
        }

        // GET: Municipalities/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var municipality = await _context.Municipalities.FindAsync(id);
            if (municipality == null)
            {
                return NotFound();
            }
            ViewData["DistrictId"] = new SelectList(_context.Districts, "Id", "DistrictName", municipality.DistrictId);
            return View(municipality);
        }

        // POST: Municipalities/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DistrictId,MunicipalityName,MunicipalityType,IsActive")] Municipality municipality)
        {
            if (id != municipality.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(municipality);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MunicipalityExists(municipality.Id))
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
            ViewData["DistrictId"] = new SelectList(_context.Districts, "Id", "DistrictName", municipality.DistrictId);
            return View(municipality);
        }

        // GET: Municipalities/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var municipality = await _context.Municipalities
                .Include(m => m.District)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (municipality == null)
            {
                return NotFound();
            }

            return View(municipality);
        }

        // POST: Municipalities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var municipality = await _context.Municipalities.FindAsync(id);
            if (municipality != null)
            {
                _context.Municipalities.Remove(municipality);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MunicipalityExists(int id)
        {
            return _context.Municipalities.Any(e => e.Id == id);
        }
    }
}
