using fwu_examination_management_system.Data;
using fwu_examination_management_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fwu_examination_management_system.Controllers
{
    [Authorize]
    public class CollegeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CollegeController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Colleges.OrderBy(c => c.CollegeName).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var college = await _context.Colleges.FindAsync(id);
            if (college == null) return NotFound();
            return View(college);
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin,Admin")]
        public async Task<IActionResult> Create(College college)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    college.CreatedBy = int.TryParse(user.Id, out var userId) ? userId : 0;
                }
                college.CreatedDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                if (college.EstablishedDate.HasValue)
                    college.EstablishedDate = DateTime.SpecifyKind(college.EstablishedDate.Value, DateTimeKind.Utc);
                if (college.ClosedDate.HasValue)
                    college.ClosedDate = DateTime.SpecifyKind(college.ClosedDate.Value, DateTimeKind.Utc);

                _context.Colleges.Add(college);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(college);
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var college = await _context.Colleges.FindAsync(id);
            if (college == null) return NotFound();
            return View(college);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin,Admin")]
        public async Task<IActionResult> Edit(int id, College college)
        {
            if (id != college.CollegeId) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        college.ModifiedBy = int.TryParse(user.Id, out var userId) ? userId : 0;
                    }
                    college.ModifiedDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                    if (college.EstablishedDate.HasValue)
                        college.EstablishedDate = DateTime.SpecifyKind(college.EstablishedDate.Value, DateTimeKind.Utc);
                    if (college.ClosedDate.HasValue)
                        college.ClosedDate = DateTime.SpecifyKind(college.ClosedDate.Value, DateTimeKind.Utc);

                    _context.Update(college);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CollegeExists(college.CollegeId))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return View(college);
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var college = await _context.Colleges.FindAsync(id);
            if (college == null) return NotFound();
            return View(college);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var college = await _context.Colleges.FindAsync(id);
            if (college != null)
            {
                _context.Colleges.Remove(college);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CollegeExists(int id)
        {
            return _context.Colleges.Any(e => e.CollegeId == id);
        }
    }
}
