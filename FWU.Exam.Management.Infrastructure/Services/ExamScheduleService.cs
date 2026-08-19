using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Helpers;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ExamScheduleService(AppDbContext context, IUserContext userContext) : IExamScheduleService
{
    public async Task<(List<ExamSchedule> Items, int TotalCount)> GetExamSchedulesAsync(int page, int pageSize, string? search, string sort, string sortDir, string? examTypeName = null)
    {
        var query = BuildQuery(search, sort, sortDir, examTypeName).ApplyScope(userContext);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExamSchedule
            {
                Id = e.Id,
                ProgramId = e.ProgramId,
                ExamTypeId = e.ExamTypeId,
                ExamScheduleName = e.ExamScheduleName,
                StartDateBs = e.StartDateBs,
                EndDateBs = e.EndDateBs,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                PublishedDate = e.PublishedDate,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Remarks = e.Remarks,
                IsActive = e.IsActive,
                ExtendedDate = e.ExtendedDate,
                ExtendedDateCharge = e.ExtendedDateCharge,
                ExamFee = e.ExamFee,
                PracticalSubjectFee = e.PracticalSubjectFee,
                AdmissionCardReleaseDate = e.AdmissionCardReleaseDate,
                ExamScheduleCode = e.ExamScheduleCode,
                SemesterInstanceId = e.SemesterInstanceId,
                SemesterInstance = e.SemesterInstance,
                Program = e.Program,
                ExamType = e.ExamType
            })
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task DeactivateExpiredSchedulesAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var expired = await context.ExamSchedules
            .Where(e => e.IsActive
                        && e.EndDate != null
                        && ((e.ExtendedDate != null && DateOnly.FromDateTime(e.ExtendedDate.Value) < today)
                            || (e.ExtendedDate == null && e.EndDate < today)))
            .ToListAsync();

        foreach (var schedule in expired)
        {
            schedule.IsActive = false;
        }

        if (expired.Count > 0)
            await context.SaveChangesAsync();
    }

    public async Task<List<ExamSchedule>> GetFilteredItemsAsync(string? search, string? examTypeName = null)
    {
        var query = BuildQuery(search, "Id", "asc", examTypeName).ApplyScope(userContext);
        return await query
            .Select(e => new ExamSchedule
            {
                Id = e.Id,
                ProgramId = e.ProgramId,
                ExamTypeId = e.ExamTypeId,
                ExamScheduleName = e.ExamScheduleName,
                StartDateBs = e.StartDateBs,
                EndDateBs = e.EndDateBs,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                PublishedDate = e.PublishedDate,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Remarks = e.Remarks,
                IsActive = e.IsActive,
                ExtendedDate = e.ExtendedDate,
                ExtendedDateCharge = e.ExtendedDateCharge,
                ExamFee = e.ExamFee,
                PracticalSubjectFee = e.PracticalSubjectFee,
                AdmissionCardReleaseDate = e.AdmissionCardReleaseDate,
                ExamScheduleCode = e.ExamScheduleCode,
                SemesterInstanceId = e.SemesterInstanceId,
                SemesterInstance = e.SemesterInstance,
                Program = e.Program,
                ExamType = e.ExamType
            })
            .ToListAsync();
    }

    public async Task<ExamSchedule?> GetExamScheduleByIdAsync(int id)
    {
        return await context.ExamSchedules
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new ExamSchedule
            {
                Id = e.Id,
                TenantId = e.TenantId,
                ProgramId = e.ProgramId,
                SemesterInstanceId = e.SemesterInstanceId,
                ExamTypeId = e.ExamTypeId,
                ExamScheduleName = e.ExamScheduleName,
                StartDateBs = e.StartDateBs,
                EndDateBs = e.EndDateBs,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                PublishedDate = e.PublishedDate,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Remarks = e.Remarks,
                IsActive = e.IsActive,
                ExtendedDate = e.ExtendedDate,
                ExtendedDateCharge = e.ExtendedDateCharge,
                ExamFee = e.ExamFee,
                PracticalSubjectFee = e.PracticalSubjectFee,
                AdmissionCardReleaseDate = e.AdmissionCardReleaseDate,
                ExamScheduleCode = e.ExamScheduleCode,
                SemesterInstance = e.SemesterInstance,
                Program = e.Program,
                ExamType = e.ExamType
            })
            .FirstOrDefaultAsync();
    }

    public async Task CreateExamScheduleAsync(ExamSchedule examSchedule)
    {
        ValidateScheduleDates(examSchedule, existing: null);
        EnsureBsDates(examSchedule);
        context.ExamSchedules.Add(examSchedule);
        await context.SaveChangesAsync();
    }

    public async Task UpdateExamScheduleAsync(ExamSchedule examSchedule)
    {
        var existing = await context.ExamSchedules.FindAsync(examSchedule.Id);
        if (existing == null)
            throw new InvalidOperationException("Exam schedule not found.");

        ValidateScheduleDates(examSchedule, existing);
        EnsureBsDates(examSchedule);

        if (examSchedule.ExtendedDate.HasValue && examSchedule.EndDate.HasValue)
        {
            var extendedChanged = existing.ExtendedDate != examSchedule.ExtendedDate;

            if (extendedChanged)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var threshold = examSchedule.EndDate.Value.AddDays(-7);
                if (today < threshold && today < examSchedule.EndDate.Value)
                {
                    throw new InvalidOperationException("Extension is only allowed when the exam end date is near or has passed.");
                }

                if (DateOnly.FromDateTime(examSchedule.ExtendedDate.Value) <= examSchedule.EndDate.Value)
                {
                    throw new InvalidOperationException("Extended date must be after the original end date.");
                }
            }
        }

        context.Entry(existing).CurrentValues.SetValues(examSchedule);
        await context.SaveChangesAsync();
    }

    private static void EnsureBsDates(ExamSchedule schedule)
    {
        if (schedule.StartDate.HasValue
            && !NepaliDateConverter.TryParseBs(schedule.StartDateBs, out _))
        {
            schedule.StartDateBs = NepaliDateConverter.AdToBsString(schedule.StartDate.Value);
        }

        if (schedule.EndDate.HasValue
            && !NepaliDateConverter.TryParseBs(schedule.EndDateBs, out _))
        {
            schedule.EndDateBs = NepaliDateConverter.AdToBsString(schedule.EndDate.Value);
        }
    }

    private static void ValidateScheduleDates(ExamSchedule schedule, ExamSchedule? existing)
    {
        if (schedule.StartDate.HasValue && schedule.EndDate.HasValue
            && schedule.StartDate.Value > schedule.EndDate.Value)
        {
            throw new InvalidOperationException("Start date cannot be after end date.");
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        var startChanged = existing == null || schedule.StartDate != existing.StartDate;
        var endChanged = existing == null || schedule.EndDate != existing.EndDate;

        if (startChanged && schedule.StartDate.HasValue && schedule.StartDate.Value < today)
        {
            throw new InvalidOperationException("Start date cannot be in the past.");
        }

        if (endChanged && schedule.EndDate.HasValue && schedule.EndDate.Value < today)
        {
            throw new InvalidOperationException("End date cannot be in the past.");
        }
    }

    public async Task DeleteExamScheduleAsync(int id)
    {
        if (!await context.ExamSchedules.AnyAsync(e => e.Id == id))
            return;

        if (await context.ExamRegistrations.AnyAsync(r => r.ExamScheduleId == id))
            throw new InvalidOperationException(
                "Cannot delete this exam schedule because students are already registered for it. Delete the registrations first.");

        if (await context.ExamSubjectResults.AnyAsync(r => r.ExamScheduleId == id)
            || await context.ResultRecords.AnyAsync(r => r.ExamScheduleId == id)
            || await context.ApplicationVouchers.AnyAsync(v => v.ExamScheduleId == id))
            throw new InvalidOperationException(
                "Cannot delete this exam schedule because exam results or vouchers already exist for it.");

        await using var transaction = await context.Database.BeginTransactionAsync();

        await context.ExamSlots.Where(s => s.ExamScheduleId == id).ExecuteDeleteAsync();
        await context.ExamRollNumberSetup.Where(r => r.ExamScheduleId == id).ExecuteDeleteAsync();
        await context.ExamFees.Where(f => f.ExamScheduleId == id).ExecuteDeleteAsync();
        await context.CollegeAdminSubjectAssignments.Where(a => a.ExamScheduleId == id).ExecuteDeleteAsync();
        await context.Set<BillTitle>().Where(b => b.ExamScheduleId == id).ExecuteDeleteAsync();

        var examCenterIds = context.ExamCenters.Where(c => c.ExamScheduleId == id).Select(c => c.Id);
        await context.ExamCenterSymbolRanges.Where(r => examCenterIds.Contains(r.ExamCenterId)).ExecuteDeleteAsync();
        await context.ExamCenterVenues.Where(v => examCenterIds.Contains(v.ExamCenterId)).ExecuteDeleteAsync();
        await context.ExamCenterColleges.Where(c => examCenterIds.Contains(c.ExamCenterId)).ExecuteDeleteAsync();
        await context.ExamCenters.Where(c => c.ExamScheduleId == id).ExecuteDeleteAsync();

        await context.AdmitCards.Where(a => a.ExamScheduleId == id).ExecuteDeleteAsync();

        var paymentRequestLogIds = context.PaymentRequestLogs.Where(p => p.ExamScheduleId == id).Select(p => p.Id);
        await context.PaymentPracticalSubjects.Where(p => paymentRequestLogIds.Contains(p.PaymentRequestLogId)).ExecuteDeleteAsync();
        await context.PaymentResponseLogs.Where(p => paymentRequestLogIds.Contains(p.PaymentRequestLogId)).ExecuteDeleteAsync();
        await context.PaymentRequestLogs.Where(p => p.ExamScheduleId == id).ExecuteDeleteAsync();

        await context.ExamSchedules.Where(e => e.Id == id).ExecuteDeleteAsync();
        await transaction.CommitAsync();
    }

    public async Task<bool> ExamScheduleExistsAsync(int id)
    {
        return await context.ExamSchedules.AnyAsync(e => e.Id == id);
    }

    public async Task<DeletePreviewDto> GetDeletePreviewAsync(int id)
    {
        var schedule = await context.ExamSchedules.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        if (schedule == null)
            throw new InvalidOperationException("Exam schedule not found.");

        var items = new List<DeletePreviewItemDto>();

        var submissions = await CountRowsAsync($"SELECT COUNT(*) FROM ExamRegistrations WHERE ExamScheduleId = {id}");
        var subjectResults = await CountRowsAsync($"SELECT COUNT(*) FROM ExamSubjectResults WHERE ExamScheduleId = {id}");
        var hallTickets = await CountRowsAsync($"SELECT COUNT(*) FROM HallTickets WHERE ExamScheduleId = {id}");
        var rollNumbers = await CountRowsAsync($"SELECT COUNT(*) FROM ExamRollNumberSetup WHERE ExamScheduleId = {id}");

        var payments = await CountRowsAsync($"SELECT COUNT(*) FROM ApplicationVouchers WHERE ExamScheduleId = {id}");
        payments += await CountRowsAsync($"SELECT COUNT(*) FROM PaymentRequestLogs WHERE ExamScheduleId = {id}");
        payments += await CountRowsAsync($"SELECT COUNT(*) FROM PaymentResponseLogs WHERE PaymentRequestLogId IN (SELECT Id FROM PaymentRequestLogs WHERE ExamScheduleId = {id})");
        payments += await CountRowsAsync($"SELECT COUNT(*) FROM PaymentPracticalSubjects WHERE PaymentRequestLogId IN (SELECT Id FROM PaymentRequestLogs WHERE ExamScheduleId = {id})");
        payments += await CountRowsAsync($"SELECT COUNT(*) FROM BankVoucher WHERE BillTitleId IN (SELECT Id FROM BillTitle WHERE ExamScheduleId = {id})");
        payments += await CountRowsAsync($"SELECT COUNT(*) FROM ExamFees WHERE ExamScheduleId = {id}");

        var centers = await CountRowsAsync($"SELECT COUNT(*) FROM ExamCenters WHERE ExamScheduleId = {id}");
        centers += await CountRowsAsync($"SELECT COUNT(*) FROM ExamCenterColleges WHERE ExamCenterId IN (SELECT Id FROM ExamCenters WHERE ExamScheduleId = {id})");
        centers += await CountRowsAsync($"SELECT COUNT(*) FROM ExamCenterVenues WHERE ExamCenterId IN (SELECT Id FROM ExamCenters WHERE ExamScheduleId = {id})");
        centers += await CountRowsAsync($"SELECT COUNT(*) FROM ExamCenterSymbolRanges WHERE ExamScheduleId = {id}");

        var slots = await CountRowsAsync($"SELECT COUNT(*) FROM ExamSlots WHERE ExamScheduleId = {id}");
        var retotals = await CountRowsAsync($"SELECT COUNT(*) FROM RetotalRequests WHERE ExamRegistrationId IN (SELECT Id FROM ExamRegistrations WHERE ExamScheduleId = {id}) OR ExamSubjectResultId IN (SELECT Id FROM ExamSubjectResults WHERE ExamScheduleId = {id})");
        var resultRecords = await CountRowsAsync($"SELECT COUNT(*) FROM ResultRecords WHERE ExamScheduleId = {id}");
        var entranceApplications = await CountRowsAsync($"SELECT COUNT(*) FROM EntranceExamApplications WHERE ApplicationVoucherId IN (SELECT Id FROM ApplicationVouchers WHERE ExamScheduleId = {id})");
        var billTitles = await CountRowsAsync($"SELECT COUNT(*) FROM BillTitle WHERE ExamScheduleId = {id}");
        var adminAssignments = await CountRowsAsync($"SELECT COUNT(*) FROM CollegeAdminSubjectAssignments WHERE ExamScheduleId = {id}");

        void Add(string label, long count)
        {
            if (count > 0)
                items.Add(new DeletePreviewItemDto { Label = label, Count = (int)count });
        }

        Add("Student exam submissions", submissions);
        Add("Subject results", subjectResults);
        Add("Hall tickets", hallTickets);
        Add("Roll numbers", rollNumbers);
        Add("Payments", payments);
        Add("Exam centers", centers);
        Add("Exam slots", slots);
        Add("Retotal requests", retotals);
        Add("Result records", resultRecords);
        Add("Entrance applications", entranceApplications);
        Add("Bill titles", billTitles);
        Add("College admin subject assignments", adminAssignments);

        return new DeletePreviewDto { ScheduleName = schedule.ExamScheduleName, Items = items };
    }

    private async Task<long> CountRowsAsync(FormattableString sql)
    {
        var rows = await context.Database.SqlQuery<long>(sql).ToListAsync();
        return rows.Count > 0 ? rows[0] : 0;
    }

    public async Task<ExamScheduleSelectListsDto> GetSelectListDataAsync(ExamSchedule? examSchedule = null)
    {
        var academicYears = await context.AcademicYears.AsNoTracking().ToListAsync();
        var examTypes = await context.ExamTypes.AsNoTracking().ToListAsync();
        var programsQuery = context.Programs.AsNoTracking().ApplyScope(userContext);
        var programs = await programsQuery.ToListAsync();

        List<SelectOption> semesters;
        if (examSchedule != null && examSchedule.ProgramId > 0)
        {
            semesters = await context.SemesterInstances
                .AsNoTracking()
                .Where(si => si.ProgramId == examSchedule.ProgramId)
                .Include(si => si.Semester)
                .Include(si => si.AcademicYear)
                .OrderBy(si => si.Semester!.Number)
                .Select(si => new SelectOption
                {
                    Id = si.Id,
                    Name = si.Semester!.Name + " (" + si.Semester!.Code + ") - " + si.AcademicYear!.AcademicYearName
                })
                .ToListAsync();
        }
        else
        {
            semesters = [];
        }

        return new ExamScheduleSelectListsDto
        {
            AcademicYears = [.. academicYears.Select(ay => new SelectOption { Id = ay.Id, Name = ay.AcademicYearName })],
            ExamTypes = [.. examTypes.Select(et => new SelectOption { Id = et.Id, Name = et.Name })],
            Programs = [.. programs.Select(p => new SelectOption { Id = p.Id, Name = p.ProgramName })],
            Semesters = semesters
        };
    }

    public async Task<List<SelectOption>> GetSemestersByAcademicYearAsync(int academicYearId, int programId)
    {
        return await context.SemesterInstances.AsNoTracking()
            .Where(si => si.AcademicYearId == academicYearId
                         && si.ProgramId == programId
                         && si.Semester != null
                         && context.ProgramSemesters.Any(ps =>
                             ps.ProgramId == programId
                             && ps.SemesterId == si.SemesterId
                             && ps.IsActive))
            .Include(si => si.Semester)
            .OrderBy(si => si.Semester!.Number)
            .Select(si => new SelectOption
            {
                Id = si.Id,
                Name = si.Semester!.Name + " (" + si.Semester!.Code + ")"
            })
            .ToListAsync();
    }

    public async Task<List<SelectOption>> GetProgramsByAcademicYearAsync(int academicYearId)
    {
        return await context.SemesterInstances
            .AsNoTracking()
            .Where(si => si.AcademicYearId == academicYearId)
            .Select(si => new { si.ProgramId, si.Program!.ProgramName })
            .Distinct()
            .OrderBy(p => p.ProgramName)
            .Select(p => new SelectOption { Id = p.ProgramId, Name = p.ProgramName })
            .ToListAsync();
    }

    public async Task<ExamScheduleDetailsDto?> GetExamScheduleDetailsAsync(int id)
    {
        var schedule = await GetExamScheduleByIdAsync(id);
        if (schedule == null) return null;

        var registrations = await context.ExamRegistrations
            .Where(r => r.ExamScheduleId == id)
            .ToListAsync();

        var examSlots = await context.ExamSlots
            .Where(es => es.ExamScheduleId == id)
            .Include(es => es.SubjectOffering)
                .ThenInclude(so => so!.SubjectCatalog)
            .Include(es => es.ExamCenter)
            .ToListAsync();

        var offeringQuery = context.SubjectOfferings
            .Where(so => so.ProgramId == schedule.ProgramId && so.SemesterId == schedule.SemesterInstance!.SemesterId);
        var subjectOfferings = await offeringQuery
            .Include(so => so.SubjectCatalog)
            .OrderBy(so => so.DisplayOrder)
            .ThenBy(so => so.Id)
            .ToListAsync();

        var examCenters = await context.ExamCenters
            .Where(ec => ec.IsActive && ec.ExamScheduleId == id)
            .OrderBy(ec => ec.Code)
            .Select(ec => new SelectOption { Id = ec.Id, Name = ec.Code ?? $"Center {ec.Id}" })
            .ToListAsync();

        var batches = await context.Batches
            .Where(b => b.AcademicYearId == schedule.SemesterInstance!.AcademicYearId && b.IsActive)
            .OrderBy(b => b.BatchName)
            .Select(b => new SelectOption { Id = b.Id, Name = b.BatchName })
            .ToListAsync();

        return new ExamScheduleDetailsDto
        {
            Schedule = schedule,
            TotalRegistrations = registrations.Count,
            PaidCount = registrations.Count(r => r.FeeEnclosed.HasValue && r.FeeEnclosed > 0),
            PendingCount = registrations.Count(r => !r.FeeEnclosed.HasValue || r.FeeEnclosed == 0),
            RegisteredCount = registrations.Count(r => r.Status >= Domain.Enums.RegistrationStatus.CollegeVerified),
            PendingVerificationCount = registrations.Count(r => r.Status == Domain.Enums.RegistrationStatus.Pending),
            ExamSlots = examSlots,
            SubjectOfferings = subjectOfferings,
            ExamCenters = examCenters,
            Batches = batches,
            ExistingSlotsByOfferingId = examSlots.ToDictionary(
                es => es.SubjectOfferingId,
                es => es)
        };
    }

    public async Task<ExamSlotSaveResultDto> SaveExamSlotsAsync(int examScheduleId, int batchId, int[] subjectOfferingId, int[] examCenterId, string[]? examDate, string[]? startTime, string[]? endTime, string[]? remarks)
    {
        var schedule = await context.ExamSchedules.FindAsync(examScheduleId);
        if (schedule == null)
            return new ExamSlotSaveResultDto { Errors = ["Exam schedule not found."] };

        var validOfferingQuery = context.SubjectOfferings
            .Where(so => so.ProgramId == schedule.ProgramId && so.SemesterId == schedule.SemesterInstance!.SemesterId);
        var validOfferingIds = await validOfferingQuery.Select(so => so.Id).ToHashSetAsync();

        var existingSlots = await context.ExamSlots
            .Where(es => es.ExamScheduleId == examScheduleId)
            .ToDictionaryAsync(es => es.SubjectOfferingId);

        var result = new ExamSlotSaveResultDto();
        for (var i = 0; i < subjectOfferingId.Length; i++)
        {
            var offeringId = subjectOfferingId[i];
            if (!validOfferingIds.Contains(offeringId)) continue;

            var date = (i < (examDate?.Length ?? 0) ? examDate![i] : null)?.Trim();
            if (string.IsNullOrEmpty(date)) continue;

            if (!DateOnly.TryParse(date, out var parsedDate))
            {
                result.Errors.Add($"Subject '{offeringId}' has an invalid exam date '{date}'. Use YYYY-MM-DD.");
                continue;
            }

            if (schedule.StartDate.HasValue && parsedDate < schedule.StartDate.Value)
                result.Errors.Add($"Subject '{offeringId}' exam date {date} is before the schedule start date {schedule.StartDate.Value:yyyy-MM-dd}.");
            if (schedule.EndDate.HasValue && parsedDate > schedule.EndDate.Value)
                result.Errors.Add($"Subject '{offeringId}' exam date {date} is after the schedule end date {schedule.EndDate.Value:yyyy-MM-dd}.");
        }

        if (result.Errors.Count > 0)
            return result;

        for (var i = 0; i < subjectOfferingId.Length; i++)
        {
            var offeringId = subjectOfferingId[i];
            if (!validOfferingIds.Contains(offeringId)) continue;

            var centerId = i < examCenterId.Length ? examCenterId[i] : 0;
            var date = (i < (examDate?.Length ?? 0) ? examDate![i] : null)?.Trim();
            var startText = i < (startTime?.Length ?? 0) ? startTime![i] : null;
            var endText = i < (endTime?.Length ?? 0) ? endTime![i] : null;
            var remark = (i < (remarks?.Length ?? 0) ? remarks![i] : null)?.Trim();

            var start = TimeOnly.TryParse(startText, out var startParsed) ? startParsed : schedule.StartTime;
            var end = TimeOnly.TryParse(endText, out var endParsed) ? endParsed : schedule.EndTime;

            if (existingSlots.TryGetValue(offeringId, out var slot))
            {
                if (centerId > 0) slot.ExamCenterId = centerId;
                if (batchId > 0) slot.BatchId = batchId;
                slot.ExamDate = string.IsNullOrEmpty(date) ? null : date;
                slot.StartTime = start;
                slot.EndTime = end;
                slot.Remarks = string.IsNullOrEmpty(remark) ? null : remark;
                result.Updated++;
            }
            else if (centerId > 0 && batchId > 0)
            {
                context.ExamSlots.Add(new ExamSlot
                {
                    ExamScheduleId = examScheduleId,
                    SubjectOfferingId = offeringId,
                    ExamCenterId = centerId,
                    BatchId = batchId,
                    ExamDate = string.IsNullOrEmpty(date) ? null : date,
                    StartTime = start,
                    EndTime = end,
                    Remarks = string.IsNullOrEmpty(remark) ? null : remark,
                    TenantId = schedule.TenantId
                });
                result.Added++;
            }
        }

        if (result.Added > 0 || result.Updated > 0)
            await context.SaveChangesAsync();

        return result;
    }

    public async Task DeleteExamSlotAsync(int slotId)
    {
        var slot = await context.ExamSlots.FindAsync(slotId);
        if (slot != null)
        {
            context.ExamSlots.Remove(slot);
            await context.SaveChangesAsync();
        }
    }

    public async Task<Dictionary<int, int>> GetRegistrationCountsAsync(List<int> scheduleIds)
    {
        return await context.ExamRegistrations
            .Where(r => scheduleIds.Contains(r.ExamScheduleId))
            .GroupBy(r => r.ExamScheduleId)
            .Select(g => new { ScheduleId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ScheduleId, x => x.Count);
    }

    public async Task<int> GetRegistrationCountAsync(int scheduleId)
    {
        return await context.ExamRegistrations.CountAsync(r => r.ExamScheduleId == scheduleId);
    }

    private IQueryable<ExamSchedule> BuildQuery(string? search, string sort, string sortDir, string? examTypeName = null)
    {
        var query = context.ExamSchedules.AsNoTracking();

        if (!string.IsNullOrEmpty(examTypeName))
            query = query.Where(s => s.ExamType != null && s.ExamType.Name == examTypeName);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                (s.ExamScheduleName != null && s.ExamScheduleName.Contains(search)) ||
                (s.ExamScheduleCode != null && s.ExamScheduleCode.Contains(search)) ||
                (s.Remarks != null && s.Remarks.Contains(search)) ||
                (s.SemesterInstance != null && s.SemesterInstance.AcademicYear != null && s.SemesterInstance.AcademicYear.AcademicYearName != null && s.SemesterInstance.AcademicYear.AcademicYearName.Contains(search)) ||
                (s.Program != null && s.Program.ProgramName != null && s.Program.ProgramName.Contains(search)) ||
                (s.ExamType != null && s.ExamType.Name != null && s.ExamType.Name.Contains(search)));
        }

        var descending = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sort.ToLower() switch
        {
            "name" => descending ? query.OrderByDescending(e => e.ExamScheduleName) : query.OrderBy(e => e.ExamScheduleName),
            "code" => descending ? query.OrderByDescending(e => e.ExamScheduleCode) : query.OrderBy(e => e.ExamScheduleCode),
            "academicyear" => descending
                ? query.OrderByDescending(e => e.SemesterInstance != null && e.SemesterInstance.AcademicYear != null ? e.SemesterInstance.AcademicYear.AcademicYearName : string.Empty)
                : query.OrderBy(e => e.SemesterInstance != null && e.SemesterInstance.AcademicYear != null ? e.SemesterInstance.AcademicYear.AcademicYearName : string.Empty),
            "level" => descending
                ? query.OrderByDescending(e => e.Program != null ? e.Program.ProgramName : string.Empty)
                : query.OrderBy(e => e.Program != null ? e.Program.ProgramName : string.Empty),
            "examtype" => descending
                ? query.OrderByDescending(e => e.ExamType != null ? e.ExamType.Name : string.Empty)
                : query.OrderBy(e => e.ExamType != null ? e.ExamType.Name : string.Empty),
            "startdate" => descending ? query.OrderByDescending(e => e.StartDate) : query.OrderBy(e => e.StartDate),
            _ => descending ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id)
        };
    }
}
