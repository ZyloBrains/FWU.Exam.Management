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
    public class RollNoGenerationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RollNoGenerationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await BuildModelAsync(new RollNoGenerationSetupViewModel());
            return View(model);
        }

        [Authorize(Roles = "SystemAdmin,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RollNoGenerationSetupViewModel model, string command)
        {
            model = await BuildModelAsync(model);

            if (!model.ExamScheduleParentId.HasValue)
            {
                ModelState.AddModelError(nameof(model.ExamScheduleParentId), "Please select parent exam schedule.");
                return View(model);
            }

            switch (command)
            {
                case "save":
                    await SaveSetupAsync(model.ExamScheduleParentId.Value);
                    model.StatusMessage = "Setup generated and saved successfully.";
                    break;
                case "roll":
                    model.StatusMessage = model.Setups.Count > 0
                        ? "Roll number generation requested from saved setup."
                        : "No saved setup found for selected parent exam schedule.";
                    break;
                default:
                    model.StatusMessage = model.Setups.Count > 0
                        ? "Setup preview loaded from saved records."
                        : "No saved setup found for selected parent exam schedule.";
                    break;
            }

            model = await BuildModelAsync(model);
            return View(model);
        }

        private async Task<RollNoGenerationSetupViewModel> BuildModelAsync(RollNoGenerationSetupViewModel model)
        {
            model.ExamScheduleParents = [new SelectListItem("Select Parent Exam Schedule", "")];
            model.ExamScheduleParents.AddRange(await _context.ExamScheduleParents
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.ExamScheduleParentName)
                .Select(x => new SelectListItem(x.ExamScheduleParentName, x.ExamScheduleParentId.ToString()))
                .ToListAsync());

            if (model.ExamScheduleParentId.HasValue)
            {
                model.Setups = await _context.ExamRollNumberSetups
                    .AsNoTracking()
                    .Where(x => x.ExamScheduleParentId == model.ExamScheduleParentId.Value)
                    .OrderByDescending(x => x.ExamRollNumberSetupId)
                    .Select(x => new RollNoSetupItemViewModel
                    {
                        ExamRollNumberSetupId = x.ExamRollNumberSetupId,
                        FirstExamRollNumber = x.FirstExamRollNumber,
                        Prefix = x.Prefix ?? string.Empty,
                        Suffix = x.Suffix ?? string.Empty,
                        MinimumRollNumberLength = x.MinimumRollNumberLength,
                        Round = x.Round,
                        MinimumGap = x.MinimumGap,
                        IsActive = x.IsActive
                    })
                    .ToListAsync();
            }
            else
            {
                model.Setups = [];
            }

            return model;
        }

        private async Task SaveSetupAsync(int examScheduleParentId)
        {
            var existing = await _context.ExamRollNumberSetups
                .AnyAsync(x => x.ExamScheduleParentId == examScheduleParentId && x.IsActive);

            if (existing)
            {
                return;
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var createdBy = int.TryParse(userIdClaim, out var id) ? id : 0;

            var setup = new ExamRollNumberSetup
            {
                ExamScheduleParentId = examScheduleParentId,
                FirstExamRollNumber = 1,
                Prefix = string.Empty,
                Suffix = string.Empty,
                MinimumRollNumberLength = 6,
                Round = 1,
                MinimumGap = 0,
                IsActive = true,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow
            };

            _context.ExamRollNumberSetups.Add(setup);
            await _context.SaveChangesAsync();
        }
    }
}
