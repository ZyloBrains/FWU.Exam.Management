using fwu_examination_management_system.Data;
using fwu_examination_management_system.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fwu_examination_management_system.Controllers
{
    [Authorize]
    public class ExamScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ExamScheduleController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.ExamSchedules.OrderByDescending(e => e.CreatedDate).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.ExamSchedules.FindAsync(id);
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
        public async Task<IActionResult> Create(ExamSchedule item)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    item.CreatedBy = int.TryParse(user.Id, out var userId) ? userId : 0;
                }

                // Convert all DateTime fields to UTC for PostgreSQL compatibility
                item.CreatedDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
                if (item.StartDateAd.HasValue)
                    item.StartDateAd = DateTime.SpecifyKind(item.StartDateAd.Value, DateTimeKind.Utc);
                if (item.EndDateAd.HasValue)
                    item.EndDateAd = DateTime.SpecifyKind(item.EndDateAd.Value, DateTimeKind.Utc);
                if (item.PublishedDate.HasValue)
                    item.PublishedDate = DateTime.SpecifyKind(item.PublishedDate.Value, DateTimeKind.Utc);
                if (item.ExtendedDate.HasValue)
                    item.ExtendedDate = DateTime.SpecifyKind(item.ExtendedDate.Value, DateTimeKind.Utc);
                if (item.CollegeApprovalDate.HasValue)
                    item.CollegeApprovalDate = DateTime.SpecifyKind(item.CollegeApprovalDate.Value, DateTimeKind.Utc);
                if (item.AdmissionCardReleaseDate.HasValue)
                    item.AdmissionCardReleaseDate = DateTime.SpecifyKind(item.AdmissionCardReleaseDate.Value, DateTimeKind.Utc);

                _context.ExamSchedules.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.ExamSchedules.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin,Admin")]
        public async Task<IActionResult> Edit(int id, ExamSchedule item)
        {
            if (id != item.ExamScheduleId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingItem = await _context.ExamSchedules.FindAsync(id);
                    if (existingItem == null) return NotFound();

                    existingItem.ExamScheduleName = item.ExamScheduleName;
                    existingItem.ExamScheduleCode = item.ExamScheduleCode;
                    existingItem.AcademicYearId = item.AcademicYearId;
                    existingItem.LevelId = item.LevelId;
                    existingItem.YearPartId = item.YearPartId;
                    existingItem.ExamTypeId = item.ExamTypeId;
                    existingItem.StartDateBs = item.StartDateBs;
                    existingItem.EndDateBs = item.EndDateBs;
                    existingItem.StartTime = item.StartTime;
                    existingItem.EndTime = item.EndTime;
                    existingItem.Remarks = item.Remarks;
                    existingItem.IsActive = item.IsActive;

                    existingItem.StartDateAd = item.StartDateAd.HasValue
                        ? DateTime.SpecifyKind(item.StartDateAd.Value, DateTimeKind.Utc)
                        : null;
                    existingItem.EndDateAd = item.EndDateAd.HasValue
                        ? DateTime.SpecifyKind(item.EndDateAd.Value, DateTimeKind.Utc)
                        : null;
                    existingItem.PublishedDate = item.PublishedDate.HasValue
                        ? DateTime.SpecifyKind(item.PublishedDate.Value, DateTimeKind.Utc)
                        : null;
                    existingItem.ExtendedDate = item.ExtendedDate.HasValue
                        ? DateTime.SpecifyKind(item.ExtendedDate.Value, DateTimeKind.Utc)
                        : null;
                    existingItem.CollegeApprovalDate = item.CollegeApprovalDate.HasValue
                        ? DateTime.SpecifyKind(item.CollegeApprovalDate.Value, DateTimeKind.Utc)
                        : null;
                    existingItem.AdmissionCardReleaseDate = item.AdmissionCardReleaseDate.HasValue
                        ? DateTime.SpecifyKind(item.AdmissionCardReleaseDate.Value, DateTimeKind.Utc)
                        : null;

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
                    if (!ExamScheduleExists(item.ExamScheduleId))
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
            var item = await _context.ExamSchedules.FindAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SystemAdmin,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.ExamSchedules.FindAsync(id);
            if (item != null)
            {
                _context.ExamSchedules.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ExamScheduleExists(int id)
        {
            return _context.ExamSchedules.Any(e => e.ExamScheduleId == id);
        }
    }
}
