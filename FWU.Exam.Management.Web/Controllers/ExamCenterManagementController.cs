using fwu_examination_management_system.Data;
using fwu_examination_management_system.Models;
using fwu_examination_management_system.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace fwu_examination_management_system.Controllers
{
    [Authorize]
    public class ExamCenterManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExamCenterManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpGet]
        public async Task<IActionResult> Index(int? examScheduleId, string statusFilter = "All")
        {
            var model = await BuildIndexModelAsync(examScheduleId, statusFilter);
            return View(model);
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await BuildCreateModelAsync(new CreateExamCenterViewModel());
            return View(model);
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateExamCenterViewModel model)
        {
            model = await BuildCreateModelAsync(model);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!model.ExamScheduleId.HasValue || !model.CollegeId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Exam Schedule and Exam Center College are required.");
                return View(model);
            }

            var exists = await _context.ExamCenters.AnyAsync(x => x.ExamScheduleId == model.ExamScheduleId.Value && x.CollegeId == model.CollegeId.Value);
            if (exists)
            {
                ModelState.AddModelError(string.Empty, "Exam center mapping already exists for selected schedule and college.");
                return View(model);
            }

            var nextCode = (await _context.ExamCenters.MaxAsync(x => (int?)x.Code) ?? 0) + 1;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var createdBy = int.TryParse(userIdClaim, out var userId) ? userId : 0;

            var item = new ExamCenter
            {
                ExamScheduleId = model.ExamScheduleId.Value,
                CollegeId = model.CollegeId.Value,
                Remark = model.Remarks ?? string.Empty,
                IsActive = model.IsActive,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                Code = nextCode
            };

            _context.ExamCenters.Add(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Exam center added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpGet]
        public async Task<IActionResult> DetailsList()
        {
            var model = new ExamCenterDetailsListViewModel
            {
                Items = await _context.ExamCenterDetails
                    .AsNoTracking()
                    .Include(x => x.ExamCenter)
                        .ThenInclude(x => x.ExamSchedule)
                    .Include(x => x.ExamCenter)
                        .ThenInclude(x => x.College)
                    .Include(x => x.College)
                    .Include(x => x.Program)
                    .OrderBy(x => x.ExamCenter.ExamSchedule.ExamScheduleName)
                    .ThenBy(x => x.College.CollegeName)
                    .Select(x => new ExamCenterDetailItemViewModel
                    {
                        ExamScheduleName = x.ExamCenter.ExamSchedule.ExamScheduleName,
                        ExamCenterCollege = x.ExamCenter.College.CollegeName,
                        College = x.College.CollegeName,
                        Program = x.Program != null ? x.Program.ProgramName : "All",
                        RollNumberFrom = x.RollNumberFrom,
                        RollNumberTo = x.RollNumberTo
                    })
                    .ToListAsync()
            };

            return View(model);
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExamCenter()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var createdBy = int.TryParse(userIdClaim, out var userId) ? userId : 0;

            var missingMappings = await (from reg in _context.ExamRegistrations.AsNoTracking()
                                         join center in _context.ExamCenters.AsNoTracking()
                                             on new { reg.ExamScheduleId, reg.CollegeId }
                                             equals new { center.ExamScheduleId, center.CollegeId } into centerJoin
                                         from center in centerJoin.DefaultIfEmpty()
                                         where center == null
                                         select new { reg.ExamScheduleId, reg.CollegeId })
                .Distinct()
                .ToListAsync();

            var nextCode = (await _context.ExamCenters.MaxAsync(x => (int?)x.Code) ?? 0) + 1;
            foreach (var map in missingMappings)
            {
                _context.ExamCenters.Add(new ExamCenter
                {
                    ExamScheduleId = map.ExamScheduleId,
                    CollegeId = map.CollegeId,
                    Remark = string.Empty,
                    IsActive = true,
                    CreatedBy = createdBy,
                    CreatedDate = DateTime.UtcNow,
                    Code = nextCode++
                });
            }

            if (missingMappings.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            var centerMap = await _context.ExamCenters
                .AsNoTracking()
                .Select(x => new { x.ExamScheduleId, x.CollegeId, x.ExamCenterId })
                .ToListAsync();

            var centerLookup = centerMap.ToDictionary(x => (x.ExamScheduleId, x.CollegeId), x => x.ExamCenterId);
            var pendingRegistrations = await _context.ExamRegistrations
                .Where(x => !x.ExamCenterId.HasValue)
                .ToListAsync();

            var assignedCount = 0;
            foreach (var registration in pendingRegistrations)
            {
                if (!centerLookup.TryGetValue((registration.ExamScheduleId, registration.CollegeId), out var examCenterId))
                {
                    continue;
                }

                registration.ExamCenterId = examCenterId;
                assignedCount++;
            }

            if (assignedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"Import complete. New centers: {missingMappings.Count}, registrations updated: {assignedCount}.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<ExamCenterManagementViewModel> BuildIndexModelAsync(int? examScheduleId, string statusFilter)
        {
            var query = _context.ExamCenters
                .AsNoTracking()
                .Include(x => x.ExamSchedule)
                .Include(x => x.College)
                .Include(x => x.ExamCenterDetails)
                .AsQueryable();

            if (examScheduleId.HasValue)
            {
                query = query.Where(x => x.ExamScheduleId == examScheduleId.Value);
            }

            if (string.Equals(statusFilter, "Remaining", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => !x.ExamCenterDetails.Any());
            }
            else if (string.Equals(statusFilter, "Applied", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.ExamCenterDetails.Any());
            }

            var model = new ExamCenterManagementViewModel
            {
                ExamScheduleId = examScheduleId,
                StatusFilter = string.IsNullOrWhiteSpace(statusFilter) ? "All" : statusFilter,
                Items = await query
                    .OrderBy(x => x.ExamSchedule.ExamScheduleName)
                    .ThenBy(x => x.College.CollegeName)
                    .Select(x => new ExamCenterManagementItemViewModel
                    {
                        ExamCenterId = x.ExamCenterId,
                        ExamScheduleName = x.ExamSchedule.ExamScheduleName,
                        ExamCenterCollege = x.College.CollegeName + " (" + x.College.CollegeCode + ")",
                        Remarks = x.Remark,
                        IsActive = x.IsActive
                    })
                    .ToListAsync()
            };

            model.ExamSchedules = [new SelectListItem("All Exam Schedules", "")];
            model.ExamSchedules.AddRange(await _context.ExamSchedules
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new SelectListItem(x.ExamScheduleName, x.ExamScheduleId.ToString()))
                .ToListAsync());

            model.StatusOptions =
            [
                new SelectListItem("All", "All"),
                new SelectListItem("Remaining", "Remaining"),
                new SelectListItem("Applied", "Applied")
            ];

            return model;
        }

        private async Task<CreateExamCenterViewModel> BuildCreateModelAsync(CreateExamCenterViewModel model)
        {
            model.ExamSchedules = [new SelectListItem("Select Exam Schedule", "")];
            model.ExamSchedules.AddRange(await _context.ExamSchedules
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new SelectListItem(x.ExamScheduleName, x.ExamScheduleId.ToString()))
                .ToListAsync());

            model.Colleges = [new SelectListItem("Select College", "")];
            model.Colleges.AddRange(await _context.Colleges
                .AsNoTracking()
                .OrderBy(x => x.CollegeName)
                .Select(x => new SelectListItem(x.CollegeName + " (" + x.CollegeCode + ")", x.CollegeId.ToString()))
                .ToListAsync());

            return model;
        }
    }
}
