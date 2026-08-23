using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Helpers;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Extensions;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ExamRegistrationService(AppDbContext context, IUserContext userContext) : IExamRegistrationService
{
    public async Task<(List<ExamRegistration> Items, int TotalCount)> GetExamRegistrationsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? examScheduleId = null)
    {
        var query = BuildQuery(search, sort, sortDir, examScheduleId);
        query = query.ApplyScope(userContext);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExamRegistration
            {
                Id = e.Id,
                ExamScheduleId = e.ExamScheduleId,
                CollegeId = e.CollegeId,
                AcademicYearId = e.AcademicYearId,
                ExamCenterId = e.ExamCenterId,
                ProgramsId = e.ProgramsId,
                ExamRollNumber = e.ExamRollNumber,
                FeeEnclosed = e.FeeEnclosed,
                AttendancePercentage = e.AttendancePercentage,
                RegistrationDate = e.RegistrationDate,
                Status = e.Status,
                Sgpa = e.Sgpa,
                Remarks = e.Remarks,
                IsActive = e.IsActive,
                RollNumberIndex = e.RollNumberIndex,
                IsAppliedByStudent = e.IsAppliedByStudent,
                ExamSchedule = e.ExamSchedule,
                College = e.College,
                ExamCenter = e.ExamCenter,
                AcademicYear = e.AcademicYear,
                Program = e.Program
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<ExamRegistration>> GetFilteredItemsAsync(string? search)
    {
        var query = BuildQuery(search, "Id", "asc", null);
        query = query.ApplyScope(userContext);
        return await query
            .Select(e => new ExamRegistration
            {
                Id = e.Id,
                ExamScheduleId = e.ExamScheduleId,
                CollegeId = e.CollegeId,
                AcademicYearId = e.AcademicYearId,
                ExamCenterId = e.ExamCenterId,
                ProgramsId = e.ProgramsId,
                ExamRollNumber = e.ExamRollNumber,
                FeeEnclosed = e.FeeEnclosed,
                RegistrationDate = e.RegistrationDate,
                Status = e.Status,
                Sgpa = e.Sgpa,
                Remarks = e.Remarks,
                IsActive = e.IsActive,
                ExamSchedule = e.ExamSchedule,
                College = e.College,
                AcademicYear = e.AcademicYear,
                Program = e.Program
            })
            .ToListAsync();
    }

    public async Task<ExamRegistration?> GetExamRegistrationByIdAsync(int id)
    {
        return await context.ExamRegistrations
            .AsNoTracking()
            .Include(e => e.ExamSchedule)
            .Include(e => e.College)
            .Include(e => e.ExamCenter)
            .Include(e => e.AcademicYear)
            .Include(e => e.Program)
            .Include(e => e.ApplicationVoucher)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task CreateExamRegistrationAsync(ExamRegistration examRegistration)
    {
        context.ExamRegistrations.Add(examRegistration);
        await context.SaveChangesAsync();
    }

    public async Task UpdateExamRegistrationAsync(ExamRegistration examRegistration)
    {
        var existing = await context.ExamRegistrations.FindAsync(examRegistration.Id);
        if (existing != null)
        {
            examRegistration.TenantId = existing.TenantId;
            context.Entry(existing).CurrentValues.SetValues(examRegistration);
            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteExamRegistrationAsync(int id)
    {
        var examRegistration = await context.ExamRegistrations.FindAsync(id);
        if (examRegistration != null)
        {
            examRegistration.IsActive = false;
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExamRegistrationExistsAsync(int id)
    {
        return await context.ExamRegistrations.AnyAsync(e => e.Id == id);
    }

    public async Task VerifyExamRegistrationAsync(int id)
    {
        var examRegistration = await context.ExamRegistrations.FindAsync(id);
        if (examRegistration != null && examRegistration.Status == RegistrationStatus.Pending)
        {
            examRegistration.Status = RegistrationStatus.CollegeVerified;
            examRegistration.VerifiedByUsername = await ResolveUsernameAsync();
            examRegistration.VerifiedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public async Task ApproveExamRegistrationAsync(int id)
    {
        var examRegistration = await context.ExamRegistrations.FindAsync(id);
        if (examRegistration != null && examRegistration.Status == RegistrationStatus.CollegeVerified)
        {
            examRegistration.Status = RegistrationStatus.AdminVerified;
            examRegistration.AdminVerifiedByUsername = await ResolveUsernameAsync();
            examRegistration.AdminVerifiedDate = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public async Task<(bool Success, string Message)> RejectExamRegistrationAsync(int id, string? reason)
    {
        reason = reason?.Trim();
        if (string.IsNullOrEmpty(reason))
            return (false, "A rejection reason is required.");

        var examRegistration = await context.ExamRegistrations
            .Where(er => er.Id == id && er.IsAppliedByStudent == true && er.IsActive)
            .ApplyScope(userContext)
            .FirstOrDefaultAsync();

        if (examRegistration == null)
            return (false, "Exam form not found.");

        if (examRegistration.Status != RegistrationStatus.Pending && examRegistration.Status != RegistrationStatus.CollegeVerified)
            return (false, "Only pending or college-verified forms can be rejected.");

        var hasAdmitCard = await context.AdmitCards!
            .AsNoTracking()
            .AnyAsync(ac => ac.ExamRegistrationId == id && ac.IsActive);
        if (hasAdmitCard)
            return (false, "This form cannot be rejected because an admit card has already been generated.");

        var username = await ResolveUsernameAsync() ?? "unknown";
        reason = reason.Length > 150 ? reason[..150] : reason;
        examRegistration.Remarks = $"[Rejected by {username} on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC] {reason}";
        examRegistration.Status = RegistrationStatus.Rejected;

        await context.SaveChangesAsync();
        return (true, "Exam form rejected.");
    }

    private async Task<string?> ResolveUsernameAsync()
    {
        if (string.IsNullOrEmpty(userContext.UserId)) return null;
        var user = await context.Users.FindAsync(userContext.UserId);
        return user?.UserName;
    }

    public async Task<ExamRegistrationSelectListsDto> GetSelectListDataAsync(ExamRegistration? examRegistration = null)
    {
        var examSchedulesQuery = context.ExamSchedules.AsNoTracking().ApplyScope(userContext);
        var examSchedules = await examSchedulesQuery.ToListAsync();

        var collegesQuery = context.Colleges.AsNoTracking().ApplyScope(userContext);
        var colleges = await collegesQuery.ToListAsync();

        var academicYears = await context.AcademicYears.AsNoTracking().ToListAsync();

        var programsQuery = context.Programs.AsNoTracking().ApplyScope(userContext);
        var programs = await programsQuery.ToListAsync();

        var examCentersQuery = context.ExamCenters.AsNoTracking();
        var examCenters = await examCentersQuery.ToListAsync();

        return new ExamRegistrationSelectListsDto
        {
            ExamSchedules = examSchedules.Select(es => new SelectOption { Id = es.Id, Name = es.ExamScheduleName }).ToList(),
            Colleges = colleges.Select(c => new SelectOption { Id = c.Id, Name = c.Name }).ToList(),
            AcademicYears = academicYears.Select(ay => new SelectOption { Id = ay.Id, Name = ay.AcademicYearName }).ToList(),
            Programs = programs.Select(p => new SelectOption { Id = p.Id, Name = p.ProgramName }).ToList(),
            ExamCenters = examCenters.Select(ec => new SelectOption { Id = ec.Id, Name = $"Center {ec.Code}" }).ToList()
        };
    }

    private IQueryable<ExamRegistration> BuildQuery(string? search, string sort, string sortDir, int? examScheduleId = null)
    {
        var query = context.ExamRegistrations.AsNoTracking();

        if (examScheduleId.HasValue)
            query = query.Where(e => e.ExamScheduleId == examScheduleId.Value);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(e =>
                (e.ExamRollNumber != null && e.ExamRollNumber.Contains(search)) ||
                (e.Remarks != null && e.Remarks.Contains(search)) ||
                (e.Sgpa != null && e.Sgpa.Contains(search)) ||
                (e.ExamSchedule != null && e.ExamSchedule.ExamScheduleName != null && e.ExamSchedule.ExamScheduleName.Contains(search)) ||
                (e.College != null && e.College.Name != null && e.College.Name.Contains(search)));
        }

        var descending = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sort.ToLower() switch
        {
            "rollnumber" => descending ? query.OrderByDescending(e => e.ExamRollNumber) : query.OrderBy(e => e.ExamRollNumber),
            "schedule" => descending
                ? query.OrderByDescending(e => e.ExamSchedule != null ? e.ExamSchedule.ExamScheduleName : string.Empty)
                : query.OrderBy(e => e.ExamSchedule != null ? e.ExamSchedule.ExamScheduleName : string.Empty),
            "college" => descending
                ? query.OrderByDescending(e => e.College != null ? e.College.Name : string.Empty)
                : query.OrderBy(e => e.College != null ? e.College.Name : string.Empty),
            "status" => descending ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
            _ => descending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id)
        };
    }

    public async Task<ExamFormsAdminResult> GetStudentExamFormsAsync(int? academicYearId, int? levelId, int? examScheduleId, string? search, int page, int pageSize)
    {
        var scopedQuery = context.ExamRegistrations
            .AsNoTracking()
            .Where(er => er.IsAppliedByStudent == true && er.IsActive)
            .ApplyScope(userContext);

        var query = scopedQuery;

        if (academicYearId.HasValue)
            query = query.Where(er => er.AcademicYearId == academicYearId.Value);

        if (levelId.HasValue)
            query = query.Where(er => er.ExamSchedule != null && er.ExamSchedule.Program != null && er.ExamSchedule.Program.LevelId == levelId.Value);

        if (examScheduleId.HasValue)
            query = query.Where(er => er.ExamScheduleId == examScheduleId.Value);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(er =>
                (er.ExamRollNumber != null && er.ExamRollNumber.Contains(search)) ||
                (er.Remarks != null && er.Remarks.Contains(search)));
        }

        var pendingBySchedule = await scopedQuery
            .Where(er => er.Status == RegistrationStatus.Pending)
            .GroupBy(er => er.ExamSchedule!.ExamScheduleName)
            .Select(g => new SchedulePendingCountDto { ScheduleName = g.Key, PendingCount = g.Count() })
            .ToListAsync();

        var totalCount = await query.CountAsync();

        var items = await query
            .Include(er => er.ExamSchedule)
                .ThenInclude(es => es!.Program)
                    .ThenInclude(p => p!.Level)
            .Include(er => er.ExamSchedule)
                .ThenInclude(es => es!.SemesterInstance)
                    .ThenInclude(si => si!.Semester)
            .Include(er => er.ExamSchedule)
                .ThenInclude(es => es!.ExamType)
            .Include(er => er.ExamSchedule)
                .ThenInclude(es => es!.SemesterInstance)
                    .ThenInclude(si => si!.AcademicYear)
            .Include(er => er.College)
            .Include(er => er.Program)
            .Include(er => er.AcademicYear)
            .OrderByDescending(er => er.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (items.Count == 0)
        {
            return new ExamFormsAdminResult
            {
                Forms = [],
                TotalCount = totalCount,
                PaymentConfirmedCount = 0,
                AdmitCardGeneratedCount = 0,
                PendingAdmitCardCount = 0,
                PendingApprovalCount = pendingBySchedule.Sum(g => g.PendingCount),
                PendingBySchedule = pendingBySchedule
            };
        }

        var stats = await ComputeStatsAsync(scopedQuery);
        var forms = await BuildFormsAsync(items);

        return new ExamFormsAdminResult
        {
            Forms = forms,
            TotalCount = totalCount,
            PaymentConfirmedCount = stats.PaymentConfirmedCount,
            AdmitCardGeneratedCount = stats.AdmitCardGeneratedCount,
            PendingAdmitCardCount = Math.Max(0, totalCount - stats.AdmitCardGeneratedCount),
            PendingApprovalCount = pendingBySchedule.Sum(g => g.PendingCount),
            PendingBySchedule = pendingBySchedule
        };
    }

    public async Task<ExamFormAdminDto?> GetStudentExamFormDetailAsync(int id)
    {
        var er = await context.ExamRegistrations
            .AsNoTracking()
            .Where(e => e.Id == id && e.IsAppliedByStudent == true && e.IsActive)
            .ApplyScope(userContext)
            .Include(er => er.ExamSchedule)
                .ThenInclude(es => es!.Program)
                    .ThenInclude(p => p!.Level)
            .Include(er => er.ExamSchedule)
                .ThenInclude(es => es!.SemesterInstance)
                    .ThenInclude(si => si!.Semester)
            .Include(er => er.ExamSchedule)
                .ThenInclude(es => es!.ExamType)
            .Include(er => er.ExamSchedule)
                .ThenInclude(es => es!.SemesterInstance)
                    .ThenInclude(si => si!.AcademicYear)
            .Include(er => er.College)
            .Include(er => er.Program)
            .Include(er => er.AcademicYear)
            .FirstOrDefaultAsync();

        if (er == null) return null;

        var forms = await BuildFormsAsync([er]);
        return forms.FirstOrDefault();
    }

    public async Task<ExamFormEditableSubjectsDto?> GetEditableSubjectsAsync(int examRegistrationId)
    {
        var er = await context.ExamRegistrations
            .AsNoTracking()
            .Where(e => e.Id == examRegistrationId && e.IsAppliedByStudent == true && e.IsActive)
            .ApplyScope(userContext)
            .Include(er => er.ExamSchedule)
                .ThenInclude(es => es!.SemesterInstance)
                    .ThenInclude(si => si!.Semester)
            .Include(er => er.ExamSchedule)
                .ThenInclude(es => es!.SemesterInstance)
                    .ThenInclude(si => si!.AcademicYear)
            .Include(er => er.ExamSchedule)
                .ThenInclude(es => es!.ExamType)
            .FirstOrDefaultAsync();

        if (er?.ExamSchedule?.SemesterInstance == null) return null;

        var dto = new ExamFormEditableSubjectsDto
        {
            ExamRegistrationId = er.Id,
            ExamScheduleName = er.ExamSchedule!.ExamScheduleName,
            ExamTypeName = er.ExamSchedule.ExamType?.Name
        };

        var matchedLog = await FindConfirmedPaymentLogAsync(er);
        var hasAdmitCard = await context.AdmitCards!
            .AsNoTracking()
            .AnyAsync(ac => ac.ExamRegistrationId == er.Id && ac.IsActive);

        var editableStatus = er.Status == RegistrationStatus.Pending || er.Status == RegistrationStatus.CollegeVerified;
        dto.CanEdit = matchedLog != null && !hasAdmitCard && editableStatus;
        if (!dto.CanEdit)
        {
            dto.NotEditableReason = !editableStatus
                ? "Subjects can only be changed before final approval."
                : hasAdmitCard
                    ? "An admit card has already been generated for this form."
                    : "Payment has not been confirmed for this form yet.";
        }

        var selection = ReExamSubjectSelection.Parse(matchedLog?.SelectedSubjectIds);
        var selectedIds = selection.Keys.ToHashSet();

        var schedule = er.ExamSchedule!;
        var semesterNumber = schedule.SemesterInstance!.Semester?.Number ?? 0;
        var resolvedVersion = await CurriculumVersionResolver.ResolveAsync(
            context, schedule.ProgramId, schedule.SemesterInstance.AcademicYearId);
        var isReExam = IsReExamForm(er);

        int? batchAcademicYearId = null;
        if (isReExam && er.ApplicationVoucherId.HasValue)
        {
            batchAcademicYearId = await context.StudentRegistrations!
                .AsNoTracking()
                .Where(sr => context.ApplicationVouchers!
                    .Any(v => v.Id == er.ApplicationVoucherId.Value && v.StudentRegistrationId == sr.Id))
                .Select(sr => (int?)sr.AcademicYearId)
                .FirstOrDefaultAsync();
            if (batchAcademicYearId is null or <= 0)
            {
                batchAcademicYearId = null;
            }
        }

        if (isReExam)
        {
            dto.AcademicYearName = batchAcademicYearId.HasValue
                ? await context.AcademicYears
                    .AsNoTracking()
                    .Where(ay => ay.Id == batchAcademicYearId.Value)
                    .Select(ay => ay.AcademicYearName)
                    .FirstOrDefaultAsync()
                : schedule.SemesterInstance.AcademicYear?.AcademicYearName;
        }
        else
        {
            dto.AcademicYearName = schedule.SemesterInstance.AcademicYear?.AcademicYearName;
        }

        int? batchVersionId = batchAcademicYearId.HasValue
            ? await CurriculumVersionResolver.ResolveAsync(context, schedule.ProgramId, batchAcademicYearId.Value)
            : null;

        List<SubjectOffering> offerings;
        if (semesterNumber > 0 && isReExam)
        {
            // Re-exam forms belong to the student's own cohort, so the editable
            // list resolves from their batch curriculum version first; unversioned
            // and then the schedule-instance resolution act as fallbacks.
            var candidateLegs = new List<int?>();
            if (batchVersionId.HasValue) candidateLegs.Add(batchVersionId);
            candidateLegs.Add(null);
            if (resolvedVersion.HasValue && !candidateLegs.Contains(resolvedVersion.Value))
            {
                candidateLegs.Add(resolvedVersion);
            }

            var versionedLegs = candidateLegs.Where(l => l.HasValue).Select(l => l!.Value).ToList();

            var pool = await context.SubjectOfferings
                .AsNoTracking()
                .Include(so => so.SubjectCatalog)
                .Where(so => so.ProgramId == schedule.ProgramId
                          && so.Semester != null && so.Semester.Number == semesterNumber
                          && (so.CurriculumVersionId == null
                              || (so.CurriculumVersionId != null && versionedLegs.Contains(so.CurriculumVersionId.Value))))
                .OrderBy(so => so.DisplayOrder)
                .ToListAsync();

            offerings = [];
            foreach (var leg in candidateLegs)
            {
                var legRows = pool.Where(so => so.CurriculumVersionId == leg).ToList();
                if (legRows.Count == 0) continue;
                if (selectedIds.Count == 0 || legRows.Any(so => selectedIds.Contains(so.Id)))
                {
                    offerings = legRows;
                    break;
                }
                if (offerings.Count == 0)
                {
                    offerings = legRows;
                }
            }
        }
        else if (semesterNumber > 0)
        {
            offerings = await context.SubjectOfferings
                .AsNoTracking()
                .Include(so => so.SubjectCatalog)
                .Where(so => so.ProgramId == schedule.ProgramId
                          && so.Semester != null && so.Semester.Number == semesterNumber
                          && (resolvedVersion == null || so.CurriculumVersionId == resolvedVersion.Value || so.CurriculumVersionId == null))
                .OrderBy(so => so.DisplayOrder)
                .ToListAsync();
        }
        else
        {
            offerings = [];
        }

        dto.AvailableSubjects = offerings
            .Where(so => so.SubjectCatalog != null)
            .Select(so =>
            {
                var legs = selection.GetValueOrDefault(so.Id);
                return new ExamFormSelectableSubjectDto
                {
                    SubjectOfferingId = so.Id,
                    Code = so.SubjectCatalog!.SubjectCode,
                    Name = so.SubjectCatalog.SubjectName,
                    Theory = so.HasTheory,
                    Practical = so.HasPractical,
                    IsSelected = selectedIds.Contains(so.Id),
                    SelectedTheory = legs.HasFlag(ReExamLegs.Theory) || (legs == ReExamLegs.None && selectedIds.Contains(so.Id) && so.HasTheory),
                    SelectedPractical = legs.HasFlag(ReExamLegs.Practical) || (legs == ReExamLegs.None && selectedIds.Contains(so.Id) && so.HasPractical)
                };
            })
            .ToList();

        return dto;
    }

    public async Task<(bool Success, string Message)> UpdateRegistrationSubjectsAsync(int examRegistrationId, List<int> subjectOfferingIds)
    {
        var requestedIds = (subjectOfferingIds ?? [])
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (requestedIds.Count == 0)
            return (false, "At least one subject must remain selected.");

        var er = await context.ExamRegistrations
            .Where(e => e.Id == examRegistrationId && e.IsAppliedByStudent == true && e.IsActive)
            .ApplyScope(userContext)
            .Include(er => er.ExamSchedule)
                .ThenInclude(es => es!.SemesterInstance)
                    .ThenInclude(si => si!.Semester)
            .Include(er => er.ExamSchedule)
                .ThenInclude(es => es!.SemesterInstance)
                    .ThenInclude(si => si!.AcademicYear)
            .Include(er => er.ExamSchedule)
                .ThenInclude(es => es!.ExamType)
            .FirstOrDefaultAsync();

        if (er?.ExamSchedule?.SemesterInstance == null)
            return (false, "Exam form not found.");

        if (er.Status != RegistrationStatus.Pending && er.Status != RegistrationStatus.CollegeVerified)
            return (false, "Subjects can only be changed before final approval.");

        var hasAdmitCard = await context.AdmitCards!
            .AsNoTracking()
            .AnyAsync(ac => ac.ExamRegistrationId == er.Id && ac.IsActive);
        if (hasAdmitCard)
            return (false, "Subjects cannot be changed because an admit card has already been generated.");

        var schedule = er.ExamSchedule!;
        var semesterNumber = schedule.SemesterInstance!.Semester?.Number ?? 0;
        var resolvedVersion = await CurriculumVersionResolver.ResolveAsync(
            context, schedule.ProgramId, schedule.SemesterInstance.AcademicYearId);
        var isReExam = IsReExamForm(er);

        var validOfferings = await context.SubjectOfferings
            .AsNoTracking()
            .Where(so => requestedIds.Contains(so.Id)
                      && so.ProgramId == schedule.ProgramId
                      && so.Semester != null && so.Semester.Number == semesterNumber
                      && (isReExam
                          || resolvedVersion == null
                          || so.CurriculumVersionId == resolvedVersion.Value
                          || so.CurriculumVersionId == null))
            .ToDictionaryAsync(so => so.Id);

        if (validOfferings.Count != requestedIds.Count)
            return (false, "One or more selected subjects are not offered for this exam schedule.");

        var matchedLog = await FindConfirmedPaymentLogAsync(er, asNoTracking: false);
        if (matchedLog == null)
            return (false, "Payment has not been confirmed for this form yet.");

        var existingSelection = ReExamSubjectSelection.Parse(matchedLog.SelectedSubjectIds);

        var existingResults = await context.ExamSubjectResults!
            .Where(esr => esr.ExamRegistrationId == er.Id)
            .ToListAsync();

        var activeByOffering = existingResults
            .Where(esr => esr.IsActive)
            .GroupBy(esr => esr.SubjectOfferingId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(esr => esr.Id).First());

        var finalIds = validOfferings.Keys.ToHashSet();

        foreach (var result in activeByOffering.Values.Where(r => !finalIds.Contains(r.SubjectOfferingId)))
        {
            result.IsActive = false;
        }

        HashSet<int>? previousScheduleIds = null;
        if (isReExam)
        {
            previousScheduleIds = await context.ExamSchedules!
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(es => es.IsActive
                          && es.ProgramId == schedule.ProgramId
                          && es.SemesterInstance!.SemesterId == schedule.SemesterInstance.SemesterId
                          && es.Id != er.ExamScheduleId
                          && es.ExamType != null
                          && es.ExamType.Name != "Entrance")
                .Select(es => es.Id)
                .ToHashSetAsync();
        }

        foreach (var offeringId in finalIds.Where(id => !activeByOffering.ContainsKey(id)))
        {
            var offering = validOfferings[offeringId];

            // Chosen legs for this subject; fall back to the offering's own
            // papers when the payment log carries no explicit leg entry.
            var chosenLegs = existingSelection.TryGetValue(offeringId, out var legs) && legs != ReExamLegs.None
                ? legs
                : (offering.HasTheory ? ReExamLegs.Theory : ReExamLegs.None)
                | (offering.HasPractical ? ReExamLegs.Practical : ReExamLegs.None);
            var theorySelected = chosenLegs.HasFlag(ReExamLegs.Theory);
            var practicalSelected = chosenLegs.HasFlag(ReExamLegs.Practical);

            float? carriedPractical = null;
            float? carriedPracticalInternal = null;
            float? carriedTheoryInternal = null;

            if (isReExam && previousScheduleIds is { Count: > 0 })
            {
                var previousResult = await context.ExamSubjectResults!
                    .AsNoTracking()
                    .Where(esr => esr.SubjectOfferingId == offeringId
                               && previousScheduleIds.Contains(esr.ExamScheduleId ?? 0)
                               && esr.ExamRegistration != null
                               && esr.ExamRegistration.IsActive
                               && esr.IsActive)
                    .OrderByDescending(esr => esr.Id)
                    .FirstOrDefaultAsync();

                if (previousResult != null)
                {
                    // Marks carry forward; only the external marks of a re-sat
                    // leg are cleared for fresh entry.
                    carriedPractical = practicalSelected ? null : previousResult.ObtainedMarksPractical;
                    carriedPracticalInternal = previousResult.ObtainedMarksPracticalInternal;
                    carriedTheoryInternal = previousResult.ObtainedMarksTheoryInternal;
                }
            }

            context.ExamSubjectResults!.Add(new ExamSubjectResult
            {
                TenantId = er.TenantId,
                ExamRegistrationId = er.Id,
                SubjectOfferingId = offeringId,
                ExamScheduleId = er.ExamScheduleId,
                ExamTypeId = schedule.ExamTypeId,
                IsTheoryRegistered = theorySelected,
                IsPracticalRegistered = practicalSelected,
                IsActive = true,
                IsSubmitted = false,
                IsSupplementary = er.IsSupplementary,
                ObtainedMarksPractical = carriedPractical,
                ObtainedMarksPracticalInternal = carriedPracticalInternal,
                ObtainedMarksTheoryInternal = carriedTheoryInternal
            });

            existingSelection[offeringId] = chosenLegs;
        }

        matchedLog.SelectedSubjectIds = ReExamSubjectSelection.Format(
            existingSelection.Where(kvp => finalIds.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

        await context.SaveChangesAsync();
        return (true, "Subjects updated successfully.");
    }

    private async Task<PaymentRequestLog?> FindConfirmedPaymentLogAsync(ExamRegistration er, bool asNoTracking = true)
    {
        var logs = asNoTracking ? context.PaymentRequestLogs!.AsNoTracking() : context.PaymentRequestLogs!;

        if (er.ApplicationVoucherId.HasValue)
        {
            var srId = await context.ApplicationVouchers!
                .AsNoTracking()
                .Where(v => v.Id == er.ApplicationVoucherId.Value)
                .Select(v => (int?)v.StudentRegistrationId)
                .FirstOrDefaultAsync();

            if (srId.HasValue)
            {
                var log = await logs
                    .Where(prl => prl.ExamScheduleId == er.ExamScheduleId
                               && prl.StudentRegistrationId == srId.Value
                               && prl.PaymentRequestLogStatus == 1)
                    .OrderByDescending(pl => pl.Id)
                    .FirstOrDefaultAsync();

                if (log != null) return log;
            }
        }

        if (er.CollegeId > 0)
        {
            return await logs
                .Where(prl => prl.ExamScheduleId == er.ExamScheduleId
                           && prl.CollegeId == er.CollegeId
                           && prl.PaymentRequestLogStatus == 1)
                .OrderByDescending(pl => pl.Id)
                .FirstOrDefaultAsync();
        }

        return null;
    }

    private static HashSet<int> ParseSelectedSubjectIds(string? selectedSubjectIds)
    {
        return ReExamSubjectSelection.Parse(selectedSubjectIds).Keys.ToHashSet();
    }

    // Rows created before the IsSupplementary stamping shipped (e.g. by a stale
    // deployment) still describe re-exams, so the schedule's exam type is the
    // source of truth and the stored flag is only a fast path.
    private static bool IsReExamForm(ExamRegistration er) =>
        er.IsSupplementary || StudentDashboardService.IsReExamTypeStatic(er.ExamSchedule?.ExamType?.Name);

    public async Task<List<SelectOption>> GetFilterAcademicYearsAsync()
    {
        var scopedQuery = context.ExamRegistrations
            .AsNoTracking()
            .Where(er => er.IsAppliedByStudent == true && er.IsActive)
            .ApplyScope(userContext);

        var yearIds = await scopedQuery
            .Select(er => er.AcademicYearId)
            .Distinct()
            .ToListAsync();

        return await context.AcademicYears
            .AsNoTracking()
            .Where(ay => yearIds.Contains(ay.Id))
            .OrderByDescending(ay => ay.IsRunning)
            .ThenByDescending(ay => ay.Id)
            .Select(ay => new SelectOption { Id = ay.Id, Name = ay.AcademicYearName })
            .ToListAsync();
    }

    public async Task<List<SelectOption>> GetFilterLevelsAsync(int academicYearId)
    {
        var scopedQuery = context.ExamRegistrations
            .AsNoTracking()
            .Where(er => er.IsAppliedByStudent == true && er.IsActive)
            .ApplyScope(userContext);

        var levelIds = await scopedQuery
            .Where(er => er.AcademicYearId == academicYearId && er.ExamSchedule != null && er.ExamSchedule.Program != null)
            .Select(er => er.ExamSchedule!.Program!.LevelId)
            .Distinct()
            .ToListAsync();

        return await context.Levels
            .AsNoTracking()
            .Where(l => levelIds.Contains(l.Id) && l.IsActive)
            .OrderBy(l => l.LevelDisplayOrder)
            .ThenBy(l => l.LevelName)
            .Select(l => new SelectOption { Id = l.Id, Name = l.LevelName })
            .ToListAsync();
    }

    public async Task<List<SelectOption>> GetFilterExamSchedulesAsync(int academicYearId, int levelId)
    {
        var scopedQuery = context.ExamRegistrations
            .AsNoTracking()
            .Where(er => er.IsAppliedByStudent == true && er.IsActive)
            .ApplyScope(userContext);

        return await scopedQuery
            .Where(er => er.AcademicYearId == academicYearId
                && er.ExamSchedule != null
                && er.ExamSchedule.Program != null
                && er.ExamSchedule.Program.LevelId == levelId)
            .Select(er => new SelectOption { Id = er.ExamScheduleId, Name = er.ExamSchedule!.ExamScheduleName })
            .Distinct()
            .ToListAsync();
    }

    private async Task<List<ExamFormAdminDto>> BuildFormsAsync(List<ExamRegistration> items)
    {
        var registrationIds = items.Select(i => i.Id).ToList();
        var scheduleIds = items.Select(i => i.ExamScheduleId).Distinct().ToList();
        var voucherIds = items.Where(i => i.ApplicationVoucherId.HasValue).Select(i => i.ApplicationVoucherId!.Value).Distinct().ToList();

        var vouchers = voucherIds.Count > 0
            ? await context.ApplicationVouchers!
                .AsNoTracking()
                .Where(v => voucherIds.Contains(v.Id))
                .ToListAsync()
            : [];

        var erIdToSrId = vouchers
            .Where(v => v.StudentRegistrationId.HasValue)
            .ToDictionary(v => v.Id, v => v.StudentRegistrationId!.Value);

        var srIds = erIdToSrId.Values.Distinct().ToList();

        var studentRegistrations = srIds.Count > 0
            ? await context.StudentRegistrations!
                .AsNoTracking()
                .Where(sr => srIds.Contains(sr.Id))
                .ToListAsync()
            : [];

        var srLookup = studentRegistrations.ToDictionary(sr => sr.Id);

        var batchAyByRegistrationId = new Dictionary<int, int>();
        foreach (var item in items)
        {
            if (!IsReExamForm(item) || !item.ApplicationVoucherId.HasValue) continue;
            if (!erIdToSrId.TryGetValue(item.ApplicationVoucherId.Value, out var batchSrId)) continue;
            if (!srLookup.TryGetValue(batchSrId, out var batchSr) || batchSr.AcademicYearId <= 0) continue;
            batchAyByRegistrationId[item.Id] = batchSr.AcademicYearId;
        }

        var batchAcademicYearIds = batchAyByRegistrationId.Values.Distinct().ToList();
        var batchYearNameLookup = batchAcademicYearIds.Count > 0
            ? await context.AcademicYears
                .AsNoTracking()
                .Where(ay => batchAcademicYearIds.Contains(ay.Id))
                .ToDictionaryAsync(ay => ay.Id, ay => ay.AcademicYearName)
            : [];

        var paymentLogs = await context.PaymentRequestLogs!
            .AsNoTracking()
            .Where(prl => scheduleIds.Contains(prl.ExamScheduleId)
                       && prl.StudentRegistrationId != null
                       && srIds.Contains(prl.StudentRegistrationId.Value))
            .ToListAsync();

        var paymentLogLookup = paymentLogs
            .Where(pl => pl.PaymentRequestLogStatus == 1)
            .GroupBy(pl => (pl.ExamScheduleId, pl.StudentRegistrationId!.Value))
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(pl => pl.Id).First());

        var fallbackPaymentLogs = await context.PaymentRequestLogs!
            .AsNoTracking()
            .Where(prl => scheduleIds.Contains(prl.ExamScheduleId)
                       && prl.CollegeId != null)
            .ToListAsync();

        var admitCards = await context.AdmitCards!
            .AsNoTracking()
            .Where(ac => registrationIds.Contains(ac.ExamRegistrationId) && ac.IsActive)
            .ToListAsync();

        var regNumbers = studentRegistrations
            .Where(sr => !string.IsNullOrEmpty(sr.RegistrationNumber))
            .Select(sr => sr.RegistrationNumber!)
            .Distinct()
            .ToList();

        // Students have UserName = RegistrationNumber; legacy rows stored the
        // registration number in the Email column instead.
        var users = regNumbers.Count > 0
            ? await context.Users
                .AsNoTracking()
                .Where(u => (u.UserName != null && regNumbers.Contains(u.UserName))
                    || (u.Email != null && regNumbers.Contains(u.Email)))
                .Select(u => new { u.UserName, u.Email, u.ProfilePath, u.SignaturePath })
                .ToListAsync()
            : [];
        var usersByUserName = users
            .Where(u => u.UserName != null)
            .GroupBy(u => u.UserName!)
            .ToDictionary(g => g.Key, g => g.First());
        var usersByEmail = users
            .Where(u => u.Email != null)
            .GroupBy(u => u.Email!)
            .ToDictionary(g => g.Key, g => g.First());

        var subjectKeys = items
            .Where(i => i.ExamSchedule != null)
            .Select(i => (i.ExamSchedule!.ProgramId, i.ExamSchedule.SemesterInstance!.SemesterId))
            .Distinct()
            .ToList();

        var scheduleKeys = items
            .Where(i => i.ExamSchedule?.SemesterInstance != null)
            .Select(i => (i.ExamSchedule!.ProgramId, i.ExamSchedule.SemesterInstance!.AcademicYearId))
            .Distinct()
            .ToList();

        var curriculumVersionMap = new Dictionary<(int ProgramId, int AcademicYearId), int?>();
        foreach (var key in scheduleKeys)
        {
            curriculumVersionMap[key] = await CurriculumVersionResolver.ResolveAsync(
                context, key.ProgramId, key.AcademicYearId);
        }

        var batchVersionMap = new Dictionary<(int ProgramId, int AcademicYearId), int?>();
        foreach (var key in items
            .Where(i => i.ExamSchedule != null && batchAyByRegistrationId.ContainsKey(i.Id))
            .Select(i => (ProgramId: i.ExamSchedule!.ProgramId, AcademicYearId: batchAyByRegistrationId[i.Id]))
            .Distinct())
        {
            batchVersionMap[key] = await CurriculumVersionResolver.ResolveAsync(context, key.ProgramId, key.AcademicYearId);
        }

        var programIds = subjectKeys.Select(k => k.ProgramId).Distinct().ToList();
        var semesterIds = subjectKeys.Select(k => k.SemesterId).Distinct().ToList();

        var semesterNumberMap = await context.Semesters
            .AsNoTracking()
            .Where(s => semesterIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Number })
            .ToListAsync();

        var semesterIdToNumber = semesterNumberMap.ToDictionary(s => s.Id, s => s.Number);
        var semesterNumbers = semesterNumberMap.Select(s => s.Number).Distinct().ToList();

        var activeVersionIds = curriculumVersionMap.Values.Where(v => v.HasValue).Select(v => v!.Value).Distinct().ToHashSet();
        var batchVersionIds = batchVersionMap.Values.Where(v => v.HasValue).Select(v => v!.Value).Distinct().ToHashSet();

        var offerings = subjectKeys.Count > 0
            ? await context.SubjectOfferings
                .AsNoTracking()
                .Include(so => so.SubjectCatalog)
                .Include(so => so.Semester)
                .Where(so => programIds.Contains(so.ProgramId)
                          && so.Semester != null && semesterNumbers.Contains(so.Semester.Number)
                          && (activeVersionIds.Count == 0
                              || (so.CurriculumVersionId != null && activeVersionIds.Contains(so.CurriculumVersionId.Value))
                              || so.CurriculumVersionId == null
                              || (so.CurriculumVersionId != null && batchVersionIds.Contains(so.CurriculumVersionId.Value))))
                .OrderBy(so => so.DisplayOrder)
                .ToListAsync()
            : [];

        var offeringLookup = offerings
            .GroupBy(so => (so.ProgramId, semesterIdToNumber.GetValueOrDefault(so.SemesterId)))
            .ToDictionary(g => g.Key, g => g.ToList());

        var forms = items.Select(er =>
        {
            string? studentName = null;
            string? registrationNumber = null;
            string? contactNumber = null;
            string? dateOfBirthAD = null;
            string? photoPath = null;
            string? signaturePath = null;
            decimal? paidAmount = null;
            bool paymentConfirmed = false;
            string? invoiceNumber = null;
            HashSet<int>? selectedSubjectIds = null;
            Dictionary<int, ReExamLegs>? selectedLegsByOffering = null;

            if (er.ApplicationVoucherId.HasValue
                && erIdToSrId.TryGetValue(er.ApplicationVoucherId.Value, out var srId)
                && srLookup.TryGetValue(srId, out var sr))
            {
                studentName = sr.FirstName.GetFullName(sr.MiddleName, sr.LastName);
                registrationNumber = sr.RegistrationNumber;
                contactNumber = sr.ContactNumber;
                dateOfBirthAD = sr.DateOfBirthAD;

                if (!string.IsNullOrEmpty(sr.RegistrationNumber)
                    && (usersByUserName.TryGetValue(sr.RegistrationNumber, out var studentUser)
                        || usersByEmail.TryGetValue(sr.RegistrationNumber, out studentUser)))
                {
                    photoPath = studentUser.ProfilePath;
                    signaturePath = studentUser.SignaturePath;
                }
            }

            PaymentRequestLog? matchedLog = null;
            if (er.ApplicationVoucherId.HasValue
                && erIdToSrId.TryGetValue(er.ApplicationVoucherId.Value, out var logSrId)
                && paymentLogLookup.TryGetValue((er.ExamScheduleId, logSrId), out var voucherLog))
            {
                matchedLog = voucherLog;
            }
            else if (er.CollegeId > 0)
            {
                matchedLog = fallbackPaymentLogs
                    .FirstOrDefault(fpl => fpl.ExamScheduleId == er.ExamScheduleId
                                        && fpl.CollegeId == er.CollegeId
                                        && fpl.PaymentRequestLogStatus == 1);
            }

            if (matchedLog != null)
            {
                paymentConfirmed = true;
                invoiceNumber = matchedLog.InvoiceNumber;
                paidAmount = matchedLog.Amount;
                selectedSubjectIds = ReExamSubjectSelection.Parse(matchedLog.SelectedSubjectIds).Keys.ToHashSet();
                selectedLegsByOffering = ReExamSubjectSelection.Parse(matchedLog.SelectedSubjectIds);
            }

            var schedule = er.ExamSchedule;
            var subjects = new List<ExamFormSubjectDto>();
            if (schedule?.SemesterInstance != null
                && semesterIdToNumber.TryGetValue(schedule.SemesterInstance.SemesterId, out var scheduleSemNumber)
                && offeringLookup.TryGetValue((schedule.ProgramId, scheduleSemNumber), out var scheduleOfferings))
            {
                // Re-exam forms belong to the student's own cohort: resolve
                // eligible offerings strictly from one curriculum leg, preferring
                // the student's batch version, then unversioned rows, and only
                // then the schedule-instance resolution.
                List<SubjectOffering> eligible;
                if (IsReExamForm(er) && batchVersionMap.Count > 0)
                {
                    var candidateLegs = new List<int?>();
                    var batchVersion = batchAyByRegistrationId.TryGetValue(er.Id, out var batchAy)
                        ? batchVersionMap.GetValueOrDefault((schedule.ProgramId, batchAy))
                        : null;
                    if (batchVersion.HasValue) candidateLegs.Add(batchVersion);
                    candidateLegs.Add(null);
                    var scheduleResolved = curriculumVersionMap.GetValueOrDefault(
                        (schedule.ProgramId, schedule.SemesterInstance.AcademicYearId));
                    if (scheduleResolved.HasValue && !candidateLegs.Contains(scheduleResolved.Value))
                        candidateLegs.Add(scheduleResolved);

                    var pool = scheduleOfferings.Where(so => so.SubjectCatalog != null);
                    List<SubjectOffering>? legRows = null;
                    foreach (var leg in candidateLegs)
                    {
                        var current = pool.Where(so => so.CurriculumVersionId == leg).ToList();
                        if (current.Count == 0) continue;
                        if (selectedSubjectIds is not { Count: > 0 }
                            || current.Any(so => selectedSubjectIds.Contains(so.Id)))
                        {
                            legRows = current;
                            break;
                        }
                        if (legRows == null)
                        {
                            legRows = current;
                        }
                    }

                    if (legRows is { Count: > 0 } && selectedSubjectIds is { Count: > 0 })
                        eligible = legRows.Where(so => selectedSubjectIds.Contains(so.Id)).ToList();
                    else
                        eligible = [];
                }
                else
                {
                    var resolvedVersion = curriculumVersionMap.GetValueOrDefault((schedule.ProgramId, schedule.SemesterInstance.AcademicYearId));
                    var baseEligible = scheduleOfferings.Where(so => so.SubjectCatalog != null
                        && (resolvedVersion == null || so.CurriculumVersionId == resolvedVersion.Value || so.CurriculumVersionId == null));
                    if (selectedSubjectIds is { Count: > 0 })
                        baseEligible = baseEligible.Where(so => selectedSubjectIds.Contains(so.Id));
                    else
                        baseEligible = [];

                    eligible = baseEligible.ToList();
                }

                subjects = eligible
                    .Select(so =>
                    {
                        // Leg-aware registration may cover fewer papers than the
                        // offering provides; reflect what was actually paid for.
                        var legs = selectedLegsByOffering?.GetValueOrDefault(so.Id) ?? ReExamLegs.None;
                        return new ExamFormSubjectDto
                        {
                            SubjectOfferingId = so.Id,
                            Code = so.SubjectCatalog!.SubjectCode,
                            Name = so.SubjectCatalog.SubjectName,
                            Theory = so.HasTheory,
                            Practical = so.HasPractical,
                            RegisteredTheory = legs == ReExamLegs.None ? so.HasTheory : legs.HasFlag(ReExamLegs.Theory),
                            RegisteredPractical = legs == ReExamLegs.None ? so.HasPractical : legs.HasFlag(ReExamLegs.Practical)
                        };
                    })
                    .ToList();
            }

            var canEditSubjects = paymentConfirmed
                && !admitCards.Any(ac => ac.ExamRegistrationId == er.Id)
                && (er.Status == RegistrationStatus.Pending || er.Status == RegistrationStatus.CollegeVerified);

            return new ExamFormAdminDto
            {
                ExamRegistrationId = er.Id,
                StudentName = studentName,
                RegistrationNumber = registrationNumber,
                ContactNumber = contactNumber,
                DateOfBirthAD = dateOfBirthAD,
                CollegeName = er.College?.Name,
                ExamScheduleId = er.ExamScheduleId,
                ExamScheduleName = schedule?.ExamScheduleName,
                ProgramName = er.Program?.ProgramName ?? schedule?.Program?.ProgramName,
                LevelName = schedule?.Program?.Level?.LevelName ?? er.Program?.Level?.LevelName,
                SemesterName = schedule?.SemesterInstance?.Semester?.Name,
                ExamTypeName = schedule?.ExamType?.Name,
                // Re-exam forms present the student's own cohort year; the
                // schedule instance year only describes when the exam runs.
                AcademicYearName = batchAyByRegistrationId.TryGetValue(er.Id, out var displayBatchAy)
                                && batchYearNameLookup.TryGetValue(displayBatchAy, out var batchYearName)
                    ? batchYearName
                    : er.AcademicYear?.AcademicYearName ?? schedule?.SemesterInstance?.AcademicYear?.AcademicYearName,
                FeeEnclosed = er.FeeEnclosed,
                PaidAmount = paidAmount ?? er.FeeEnclosed,
                PhotoPath = photoPath,
                SignaturePath = signaturePath,
                Subjects = subjects,
                Status = er.Status,
                PaymentConfirmed = paymentConfirmed,
                InvoiceNumber = invoiceNumber,
                HasAdmitCard = admitCards.Any(ac => ac.ExamRegistrationId == er.Id),
                RegistrationDate = er.RegistrationDate,
                VerifiedByUsername = er.VerifiedByUsername,
                VerifiedDate = er.VerifiedDate,
                CanApprove = er.Status == RegistrationStatus.Pending,
                CanAdminApprove = er.Status == RegistrationStatus.CollegeVerified,
                CanEditSubjects = canEditSubjects
            };
        }).ToList();

        return forms;
    }

    private async Task<(int PaymentConfirmedCount, int AdmitCardGeneratedCount)> ComputeStatsAsync(IQueryable<ExamRegistration> scopedQuery)
    {
        var all = await scopedQuery
            .Select(er => new { er.Id, er.ApplicationVoucherId, er.ExamScheduleId })
            .ToListAsync();

        var allIds = all.Select(x => x.Id).ToList();
        var scheduleIds = all.Select(x => x.ExamScheduleId).Distinct().ToList();

        var voucherIds = all.Where(x => x.ApplicationVoucherId.HasValue).Select(x => x.ApplicationVoucherId!.Value).Distinct().ToList();
        var vouchers = voucherIds.Count > 0
            ? await context.ApplicationVouchers!
                .AsNoTracking()
                .Where(v => voucherIds.Contains(v.Id))
                .Select(v => new { v.Id, v.StudentRegistrationId })
                .ToListAsync()
            : [];

        var erIdToSrId = vouchers
            .Where(v => v.StudentRegistrationId.HasValue)
            .ToDictionary(v => v.Id, v => v.StudentRegistrationId!.Value);

        var srIds = erIdToSrId.Values.Distinct().ToList();

        var confirmedPairs = new HashSet<(int ExamScheduleId, int StudentRegistrationId)>();
        if (scheduleIds.Count > 0 && srIds.Count > 0)
        {
            var logs = await context.PaymentRequestLogs!
                .AsNoTracking()
                .Where(prl => scheduleIds.Contains(prl.ExamScheduleId)
                           && prl.StudentRegistrationId != null
                           && srIds.Contains(prl.StudentRegistrationId.Value)
                           && prl.PaymentRequestLogStatus == 1)
                .Select(prl => new { prl.ExamScheduleId, prl.StudentRegistrationId })
                .ToListAsync();

            foreach (var log in logs)
                confirmedPairs.Add((log.ExamScheduleId, log.StudentRegistrationId!.Value));
        }

        var paymentConfirmedCount = all.Count(x =>
            x.ApplicationVoucherId.HasValue
            && erIdToSrId.TryGetValue(x.ApplicationVoucherId.Value, out var srId)
            && confirmedPairs.Contains((x.ExamScheduleId, srId)));

        var admitCardGeneratedCount = allIds.Count > 0
            ? await context.AdmitCards!
                .AsNoTracking()
                .Where(ac => ac.IsActive && allIds.Contains(ac.ExamRegistrationId))
                .Select(ac => ac.ExamRegistrationId)
                .Distinct()
                .CountAsync()
            : 0;

        return (paymentConfirmedCount, admitCardGeneratedCount);
    }
}
