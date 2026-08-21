using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
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

        var programIds = subjectKeys.Select(k => k.ProgramId).Distinct().ToList();
        var semesterIds = subjectKeys.Select(k => k.SemesterId).Distinct().ToList();

        var offerings = subjectKeys.Count > 0
            ? await context.SubjectOfferings
                .AsNoTracking()
                .Include(so => so.SubjectCatalog)
                .Where(so => programIds.Contains(so.ProgramId) && semesterIds.Contains(so.SemesterId))
                .OrderBy(so => so.DisplayOrder)
                .ToListAsync()
            : [];

        var offeringLookup = offerings
            .GroupBy(so => (so.ProgramId, so.SemesterId))
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
                selectedSubjectIds = string.IsNullOrWhiteSpace(matchedLog.SelectedSubjectIds)
                    ? new HashSet<int>()
                    : matchedLog.SelectedSubjectIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(id => int.TryParse(id, out var value) ? value : (int?)null)
                        .Where(value => value.HasValue)
                        .Select(value => value!.Value)
                        .ToHashSet();
            }

            var schedule = er.ExamSchedule;
            var subjects = new List<ExamFormSubjectDto>();
            if (schedule != null && offeringLookup.TryGetValue((schedule.ProgramId, schedule.SemesterInstance!.SemesterId), out var scheduleOfferings))
            {
                var eligible = scheduleOfferings.Where(so => so.SubjectCatalog != null);
                if (selectedSubjectIds is { Count: > 0 })
                    eligible = eligible.Where(so => selectedSubjectIds.Contains(so.Id));
                else
                    eligible = [];

                subjects = eligible
                    .Select(so => new ExamFormSubjectDto
                    {
                        Code = so.SubjectCatalog!.SubjectCode,
                        Name = so.SubjectCatalog.SubjectName,
                        Theory = so.HasTheory,
                        Practical = so.HasPractical
                    })
                    .ToList();
            }

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
                AcademicYearName = er.AcademicYear?.AcademicYearName ?? schedule?.SemesterInstance?.AcademicYear?.AcademicYearName,
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
                CanAdminApprove = er.Status == RegistrationStatus.CollegeVerified
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
