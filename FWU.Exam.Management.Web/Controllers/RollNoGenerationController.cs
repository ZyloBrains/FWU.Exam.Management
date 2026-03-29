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
                    var (created, detailCount) = await SaveSetupAsync(model.ExamScheduleParentId.Value);
                    model.StatusMessage = created
                        ? $"Setup generated and saved successfully. Detail rows: {detailCount}."
                        : $"Active setup already exists for selected parent exam schedule. Detail rows: {detailCount}.";
                    break;
                case "roll":
                    var generatedCount = await GenerateRollNumbersFromSavedSetupAsync(model.ExamScheduleParentId.Value);
                    model.StatusMessage = generatedCount > 0
                        ? $"Roll numbers generated successfully. Updated records: {generatedCount}."
                        : "No pending registrations found or no saved setup detail available for selected parent exam schedule.";
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

        private async Task<(bool Created, int DetailCount)> SaveSetupAsync(int examScheduleParentId)
        {
            var existingSetup = await _context.ExamRollNumberSetups
                .AsNoTracking()
                .Where(x => x.ExamScheduleParentId == examScheduleParentId && x.IsActive)
                .OrderByDescending(x => x.ExamRollNumberSetupId)
                .FirstOrDefaultAsync();

            if (existingSetup != null)
            {
                var existingDetailCount = await _context.ExamRollNumberSetupDetails
                    .AsNoTracking()
                    .CountAsync(x => x.ExamRollNumberSetupId == existingSetup.ExamRollNumberSetupId);

                return (false, existingDetailCount);
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

            var groupedRows = await (from reg in _context.ExamRegistrations.AsNoTracking()
                                     join sp in _context.StudentProgramYearParts.AsNoTracking() on reg.StudentProgramYearPartId equals sp.StudentProgramYearPartId
                                     join sa in _context.StudentAdmissions.AsNoTracking() on sp.StudentAdmissionId equals sa.StudentAdmissionId
                                     join sch in _context.ExamSchedules.AsNoTracking() on reg.ExamScheduleId equals sch.ExamScheduleId
                                     where sch.ExamScheduleParentId == examScheduleParentId
                                           && (reg.ExamRollNumber == null || reg.ExamRollNumber == string.Empty)
                                     group new { reg, sa } by new
                                     {
                                         reg.ExamScheduleId,
                                         ProgramId = reg.ProgramsId ?? sa.ProgramsId,
                                         reg.CollegeId,
                                         ExamTypeId = reg.TypeId ?? 0
                                     }
                into g
                                     orderby g.Key.ProgramId, g.Key.ExamScheduleId, g.Key.CollegeId, g.Key.ExamTypeId
                                     select new
                                     {
                                         g.Key.ExamScheduleId,
                                         g.Key.ProgramId,
                                         g.Key.CollegeId,
                                         g.Key.ExamTypeId,
                                         Count = g.Count()
                                     }).ToListAsync();

            var currentRoll = setup.FirstExamRollNumber;
            foreach (var row in groupedRows)
            {
                var start = currentRoll;
                var end = start + row.Count - 1;

                _context.ExamRollNumberSetupDetails.Add(new ExamRollNumberSetupDetail
                {
                    ExamRollNumberSetupId = setup.ExamRollNumberSetupId,
                    ExamScheduleId = row.ExamScheduleId,
                    ProgramId = row.ProgramId,
                    ExamTypeId = row.ExamTypeId,
                    CollegeId = row.CollegeId,
                    StartRollNumber = start,
                    EndRollNumber = end,
                    Count = row.Count,
                    Prefix = setup.Prefix,
                    Suffix = setup.Suffix
                });

                currentRoll = end + 1 + Math.Max(setup.MinimumGap, 0);
            }

            if (groupedRows.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return (true, groupedRows.Count);
        }

        private async Task<int> GenerateRollNumbersFromSavedSetupAsync(int examScheduleParentId)
        {
            var setup = await _context.ExamRollNumberSetups
                .AsNoTracking()
                .Where(x => x.ExamScheduleParentId == examScheduleParentId && x.IsActive)
                .OrderByDescending(x => x.ExamRollNumberSetupId)
                .FirstOrDefaultAsync();

            if (setup == null)
            {
                return 0;
            }

            var details = await _context.ExamRollNumberSetupDetails
                .AsNoTracking()
                .Where(x => x.ExamRollNumberSetupId == setup.ExamRollNumberSetupId)
                .OrderBy(x => x.ProgramId)
                .ThenBy(x => x.ExamScheduleId)
                .ThenBy(x => x.CollegeId)
                .ThenBy(x => x.ExamTypeId)
                .ThenBy(x => x.StartRollNumber)
                .ToListAsync();

            if (details.Count == 0)
            {
                return 0;
            }

            var pending = await (from reg in _context.ExamRegistrations.AsNoTracking()
                                 join sch in _context.ExamSchedules.AsNoTracking() on reg.ExamScheduleId equals sch.ExamScheduleId
                                 join sp in _context.StudentProgramYearParts.AsNoTracking() on reg.StudentProgramYearPartId equals sp.StudentProgramYearPartId
                                 join sa in _context.StudentAdmissions.AsNoTracking() on sp.StudentAdmissionId equals sa.StudentAdmissionId
                                 join sr in _context.StudentRegistrations.AsNoTracking() on sa.StudentRegistrationId equals sr.StudentRegistrationId
                                 where sch.ExamScheduleParentId == examScheduleParentId
                                       && (reg.ExamRollNumber == null || reg.ExamRollNumber == string.Empty)
                                 select new PendingRollRow
                                 {
                                     ExamRegistrationId = reg.ExamRegistrationId,
                                     ExamScheduleId = reg.ExamScheduleId,
                                     ProgramId = reg.ProgramsId ?? sa.ProgramsId,
                                     CollegeId = reg.CollegeId,
                                     ExamTypeId = reg.TypeId ?? 0,
                                     RegistrationNo = sr.RegistrationNumber ?? string.Empty,
                                     FullName = ((sr.FirstName ?? string.Empty) + " " + (sr.MiddleName ?? string.Empty) + " " + (sr.LastName ?? string.Empty)).Trim()
                                 }).ToListAsync();

            if (pending.Count == 0)
            {
                return 0;
            }

            var pendingIds = pending.Select(x => x.ExamRegistrationId).Distinct().ToList();
            var registrationEntities = await _context.ExamRegistrations
                .Where(x => pendingIds.Contains(x.ExamRegistrationId))
                .ToDictionaryAsync(x => x.ExamRegistrationId);

            var updated = 0;
            foreach (var detail in details)
            {
                var rows = pending
                    .Where(x => x.ExamScheduleId == detail.ExamScheduleId
                                && x.ProgramId == detail.ProgramId
                                && x.CollegeId == detail.CollegeId
                                && x.ExamTypeId == detail.ExamTypeId)
                    .OrderBy(x => x.FullName)
                    .ThenBy(x => x.RegistrationNo)
                    .ThenBy(x => x.ExamRegistrationId)
                    .ToList();

                var current = detail.StartRollNumber;
                foreach (var row in rows)
                {
                    if (!registrationEntities.TryGetValue(row.ExamRegistrationId, out var registration))
                    {
                        continue;
                    }

                    var rollNo = BuildRollNumber(setup, detail, current);
                    registration.ExamRollNumber = rollNo;

                    if (!registration.ExamRollNumberCoding.HasValue && long.TryParse(rollNo, out var coding))
                    {
                        registration.ExamRollNumberCoding = coding;
                    }

                    current++;
                    updated++;
                }
            }

            if (updated > 0)
            {
                await _context.SaveChangesAsync();
            }

            return updated;
        }

        private static string BuildRollNumber(ExamRollNumberSetup setup, ExamRollNumberSetupDetail detail, int number)
        {
            var prefix = !string.IsNullOrWhiteSpace(detail.Prefix) ? detail.Prefix : setup.Prefix;
            var suffix = !string.IsNullOrWhiteSpace(detail.Suffix) ? detail.Suffix : setup.Suffix;
            var minLength = Math.Max(setup.MinimumRollNumberLength, number.ToString().Length);
            var numericPart = number.ToString().PadLeft(minLength, '0');

            return $"{prefix}{numericPart}{suffix}";
        }

        private sealed class PendingRollRow
        {
            public int ExamRegistrationId { get; init; }
            public int ExamScheduleId { get; init; }
            public int ProgramId { get; init; }
            public int CollegeId { get; init; }
            public int ExamTypeId { get; init; }
            public string RegistrationNo { get; init; } = string.Empty;
            public string FullName { get; init; } = string.Empty;
        }
    }
}
