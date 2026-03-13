using fwu_examination_management_system.Data;
using fwu_examination_management_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fwu_examination_management_system.Controllers
{
    [Authorize]
    public class SubjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public SubjectController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
                try
                {
                    // Set default values for required fields if not provided
                    if (item.ProgramID == 0) item.ProgramID = 1;
                    if (item.YearPartID == 0) item.YearPartID = 1;
                    if (item.SubjectTypeId == 0) item.SubjectTypeId = 1;

                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        item.CreatedBy = int.TryParse(user.Id, out var userId) ? userId : 0;
                    }
                    else
                    {
                        item.CreatedBy = 0;
                    }

                    // ✅ Always convert ALL DateTime fields to UTC for PostgreSQL compatibility
                    item.CreatedDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

                    // Clear ModifiedDate for new records
                    item.ModifiedDate = null;

                    item.IsActive = true; // Default to active

                    _context.SubjectDetails.Add(item);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Database error: {ex.InnerException?.Message ?? ex.Message}");
                    return View(item);
                }
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
                try
                {
                    var existingItem = await _context.SubjectDetails.FindAsync(id);
                    if (existingItem == null) return NotFound();

                    if (item.ProgramID == 0) item.ProgramID = 1;
                    if (item.YearPartID == 0) item.YearPartID = 1;
                    if (item.SubjectTypeId == 0) item.SubjectTypeId = 1;

                    existingItem.SubjectGroupID = item.SubjectGroupID;
                    existingItem.ProgramID = item.ProgramID;
                    existingItem.YearPartID = item.YearPartID;
                    existingItem.SubjectCode = item.SubjectCode;
                    existingItem.SubjectName = item.SubjectName;
                    existingItem.TheoryFullMark = item.TheoryFullMark;
                    existingItem.TheoryPassMark = item.TheoryPassMark;
                    existingItem.PracticalFullMark = item.PracticalFullMark;
                    existingItem.PracticalPassMark = item.PracticalPassMark;
                    existingItem.InternalTheoryFullMark = item.InternalTheoryFullMark;
                    existingItem.InternalTheoryPassMark = item.InternalTheoryPassMark;
                    existingItem.InternalPracticalFullMark = item.InternalPracticalFullMark;
                    existingItem.InternalPracticalPassMark = item.InternalPracticalPassMark;
                    existingItem.CreditHour = item.CreditHour;
                    existingItem.HasPractical = item.HasPractical;
                    existingItem.HasInternal = item.HasInternal;
                    existingItem.DisplayOrder = item.DisplayOrder;
                    existingItem.Remarks = item.Remarks;
                    existingItem.IsActive = item.IsActive;
                    existingItem.IsCompulsory = item.IsCompulsory;
                    existingItem.ShortName = item.ShortName;
                    existingItem.ConSubjectCode = item.ConSubjectCode;
                    existingItem.SubjectTypeId = item.SubjectTypeId;
                    existingItem.HasTheory = item.HasTheory;
                    existingItem.Year = item.Year;
                    existingItem.Part = item.Part;

                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        existingItem.ModifiedBy = int.TryParse(user.Id, out var userId) ? userId : 0;
                    }

                    existingItem.ModifiedDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubjectExists(item.SubjectDetailID))
                    {
                        return NotFound();
                    }
                    throw;
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", $"Database error: {ex.InnerException?.Message ?? ex.Message}");
                    return View(item);
                }
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

        private bool SubjectExists(int id)
        {
            return _context.SubjectDetails.Any(e => e.SubjectDetailID == id);
        }
    }
}
