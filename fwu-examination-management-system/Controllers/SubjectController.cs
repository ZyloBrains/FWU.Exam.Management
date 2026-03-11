using fwu_examination_management_system.Data;
using fwu_examination_management_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fwu_examination_management_system.Controllers
{
    [Authorize]
    public class SubjectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SubjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.SubjectDetails.OrderBy(s => s.SubjectName).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.SubjectDetails.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin,Admin")]
        public async Task<IActionResult> Create(SubjectDetail item)
        {
            if (ModelState.IsValid)
            {
                item.CreatedDate = DateTime.UtcNow;
                _context.SubjectDetails.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.SubjectDetails.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin,Admin")]
        public async Task<IActionResult> Edit(int id, SubjectDetail item)
        {
            if (id != item.SubjectDetailID) return NotFound();
            if (ModelState.IsValid)
            {
                item.ModifiedDate = DateTime.UtcNow;
                _context.Update(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.SubjectDetails.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.SubjectDetails.FindAsync(id);
            if (item != null)
            {
                _context.SubjectDetails.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
