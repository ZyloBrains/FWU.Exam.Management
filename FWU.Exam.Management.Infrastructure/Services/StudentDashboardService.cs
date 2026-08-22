using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Extensions;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;

public class StudentDashboardService(AppDbContext context, IUserContext userContext, ILogger<StudentDashboardService> logger) : IStudentDashboardService
{
    public async Task<StudentRegistration?> GetStudentRegistrationByEmailAsync(string email)
    {
        return await RegistrationQuery()
            .FirstOrDefaultAsync(s => s.Email != null && s.Email == email);
    }

    public async Task<StudentRegistration?> GetStudentRegistrationByUserIdAsync(string userId)
    {
        var admission = await context.StudentAdmissions!
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.AppUserId == userId);
        if (admission != null)
        {
            var registration = await RegistrationQuery()
                .FirstOrDefaultAsync(s => s.StudentAdmissionId == admission.Id);
            if (registration != null) return registration;
        }

        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return null;

        // Students authenticate with their registration number (UserName == RegistrationNumber),
        // so fall back to that link before trying email.
        if (!string.IsNullOrWhiteSpace(user.UserName))
        {
            var byRegistrationNumber = await RegistrationQuery()
                .FirstOrDefaultAsync(s => s.RegistrationNumber != null && s.RegistrationNumber == user.UserName);
            if (byRegistrationNumber != null) return byRegistrationNumber;
        }

        return user.Email == null ? null : await GetStudentRegistrationByEmailAsync(user.Email);
    }

    private IQueryable<StudentRegistration> RegistrationQuery() =>
        context.StudentRegistrations!
            .AsNoTracking()
            .Include(s => s.AcademicYear)
            .Include(s => s.Level)
            .Include(s => s.College)
            .Include(s => s.Gender)
            .Include(s => s.StudentCategory)
            .Include(s => s.Ethnicity)
            .Include(s => s.PermanentAddress).ThenInclude(a => a!.LocalLevel).ThenInclude(l => l!.District).ThenInclude(d => d!.Province)
            .Include(s => s.CurrentAddress).ThenInclude(a => a!.LocalLevel).ThenInclude(l => l!.District).ThenInclude(d => d!.Province);

    public async Task<List<ExamSchedule>> GetExamSchedulesForStudentAsync(StudentRegistration student, string userId)
    {
        var enrolledSemesterInstanceIds = await context.SemesterEnrollments!
            .AsNoTracking()
            .Where(se => se.StudentAdmissionId == student.StudentAdmissionId)
            .Select(se => (int?)se.SemesterInstanceId)
            .Distinct()
            .ToListAsync();

        if (enrolledSemesterInstanceIds.Count == 0) return [];

        var query = context.ExamSchedules!
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(es => es.ExamType)
            .Include(es => es.Program)
            .Include(es => es.Level)
            .Include(es => es.SemesterInstance).ThenInclude(si => si!.Semester)
            .Include(es => es.SemesterInstance).ThenInclude(si => si!.AcademicYear)
            .Where(es => es.IsActive
                      && es.ProgramId == student.ProgramId
                      && enrolledSemesterInstanceIds.Contains(es.SemesterInstanceId)
                      && es.ExamType!.Name != "Entrance");

        if (student.LevelId != 0)
        {
            query = query.Where(es => es.LevelId == null || es.LevelId == student.LevelId);
        }

        var allSchedules = await query.ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);

        var filtered = new List<ExamSchedule>();
        foreach (var schedule in allSchedules)
        {
            // Date window visibility: a schedule is only shown once its start date has
            // arrived and until it ends (ExtendedDate overrides EndDate when set).
            if (schedule.StartDate.HasValue && schedule.StartDate.Value > today)
                continue;

            DateOnly? effectiveEnd = schedule.ExtendedDate.HasValue
                ? DateOnly.FromDateTime(schedule.ExtendedDate.Value)
                : schedule.EndDate;
            if (effectiveEnd.HasValue && effectiveEnd.Value < today)
                continue;

            var isSupplementary = IsReExamType(schedule.ExamType?.Name);

            if (isSupplementary)
            {
                var hasFailed = await HasFailedSubjectsInSemesterAsync(userId, schedule.SemesterInstance!.SemesterId, student.ProgramId ?? 0);
                if (!hasFailed)
                    continue;
            }
            filtered.Add(schedule);
        }

        return filtered;
    }

    public async Task<List<SubjectOffering>> GetSubjectOfferingsForScheduleAsync(int examScheduleId)
    {
        var schedule = await context.ExamSchedules!
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(es => es.Id == examScheduleId);

        if (schedule == null) return new List<SubjectOffering>();

        var instance = await context.SemesterInstances!
            .AsNoTracking()
            .Where(si => si.Id == schedule.SemesterInstanceId)
            .Select(si => new { si.SemesterId, si.AcademicYearId })
            .FirstOrDefaultAsync();

        int? semesterNumber = null;
        if (instance != null)
        {
            semesterNumber = await context.Semesters
                .AsNoTracking()
                .Where(s => s.Id == instance.SemesterId)
                .Select(s => (int?)s.Number)
                .FirstOrDefaultAsync();
        }

        var curriculumVersionId = instance != null
            ? await CurriculumVersionResolver.ResolveAsync(context, schedule.ProgramId, instance.AcademicYearId)
            : null;

        var query = context.SubjectOfferings!
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .ThenInclude(sc => sc!.SubjectType)
            .Where(so => so.ProgramId == schedule.ProgramId
                      && so.IsActive
                      && so.Semester != null && so.Semester.Number == semesterNumber);

        if (curriculumVersionId.HasValue)
        {
            var versioned = await query.Where(so => so.CurriculumVersionId == curriculumVersionId.Value)
                .OrderBy(so => so.DisplayOrder).ToListAsync();
            if (versioned.Count > 0) return versioned;
        }

        return await query.Where(so => so.CurriculumVersionId == null)
            .OrderBy(so => so.DisplayOrder).ToListAsync();
    }

    public async Task<List<SubjectOffering>> GetSubjectOfferingsForStudentAsync(string userId, int programId)
    {
        var admission = await ResolveStudentAdmissionAsync(userId);

        var query = context.SubjectOfferings!
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .Include(so => so.Semester)
            .Where(so => so.ProgramId == programId && so.IsActive);

        var academicYearId = admission?.AcademicYearId;

        if (admission != null)
        {
            var enrollment = await context.Set<SemesterEnrollment>()
                .AsNoTracking()
                .Where(se => se.StudentAdmissionId == admission.Id
                          && se.EnrollmentStatus == StudentEnrollmentStatus.Active
                          && se.DropDate == null)
                .OrderByDescending(se => se.EnrolledDate)
                .Select(se => new { se.SemesterInstance!.SemesterId, se.SemesterInstance.AcademicYearId })
                .FirstOrDefaultAsync();

            if (enrollment?.SemesterId != null)
            {
                var enrolledVersionId = enrollment.AcademicYearId != 0
                    ? await CurriculumVersionResolver.ResolveAsync(context, programId, enrollment.AcademicYearId)
                    : null;

                var enrolledSemesterNumber = await context.Semesters
                    .AsNoTracking()
                    .Where(s => s.Id == enrollment.SemesterId)
                    .Select(s => (int?)s.Number)
                    .FirstOrDefaultAsync();

                var enrolledQuery = query
                    .Where(so => so.Semester != null && so.Semester.Number == enrolledSemesterNumber);

                if (enrolledVersionId.HasValue)
                {
                    var versioned = await enrolledQuery.Where(so => so.CurriculumVersionId == enrolledVersionId.Value)
                        .OrderBy(so => so.DisplayOrder).ToListAsync();
                    if (versioned.Count > 0) return versioned;
                }

                return await enrolledQuery.Where(so => so.CurriculumVersionId == null)
                    .OrderBy(so => so.DisplayOrder).ToListAsync();
            }
        }
        else
        {
            var user = await context.Users.FindAsync(userId);
            StudentRegistration? registration = null;
            if (user?.UserName != null)
            {
                registration = await context.StudentRegistrations!
                    .AsNoTracking()
                    .FirstOrDefaultAsync(sr => sr.RegistrationNumber == user.UserName);
            }
            if (registration == null && user?.Email != null)
            {
                registration = await context.StudentRegistrations!
                    .AsNoTracking()
                    .FirstOrDefaultAsync(sr => (sr.Email != null && sr.Email == user.Email) || sr.RegistrationNumber == user.Email);
            }
            if (registration != null && registration.AcademicYearId != 0)
            {
                academicYearId = registration.AcademicYearId;
            }
        }

        if (!academicYearId.HasValue) return new List<SubjectOffering>();

        var curriculumVersionId = await CurriculumVersionResolver.ResolveAsync(context, programId, academicYearId.Value);

        var programSemesterIds = await context.ProgramSemesters
            .AsNoTracking()
            .Where(ps => ps.ProgramId == programId && ps.IsActive)
            .Select(ps => ps.SemesterId)
            .ToListAsync();

        var fallbackQuery = query.Where(so => so.Semester != null && programSemesterIds.Contains(so.SemesterId));

        if (curriculumVersionId.HasValue)
        {
            var versionedFallback = fallbackQuery.Where(so => so.CurriculumVersionId == curriculumVersionId.Value);
            var firstSemVer = await versionedFallback
                .Select(so => (int?)so.Semester!.Number).OrderBy(n => n).FirstOrDefaultAsync();
            if (firstSemVer.HasValue)
            {
                return await versionedFallback
                    .Where(so => so.Semester!.Number == firstSemVer.Value)
                    .OrderBy(so => so.DisplayOrder).ToListAsync();
            }
        }

        var firstSemNull = await fallbackQuery
            .Where(so => so.CurriculumVersionId == null)
            .Select(so => (int?)so.Semester!.Number).OrderBy(n => n).FirstOrDefaultAsync();

        if (!firstSemNull.HasValue) return new List<SubjectOffering>();

        return await fallbackQuery
            .Where(so => so.CurriculumVersionId == null && so.Semester!.Number == firstSemNull.Value)
            .OrderBy(so => so.DisplayOrder).ToListAsync();
    }

    public async Task<decimal> GetExamFeeForScheduleAsync(int examScheduleId)
    {
        var schedule = await context.ExamSchedules!
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(es => es.Id == examScheduleId);

        return schedule?.ExamFee ?? 0;
    }

    public async Task<decimal> GetPracticalSubjectFeeForScheduleAsync(int examScheduleId)
    {
        var schedule = await context.ExamSchedules!
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(es => es.Id == examScheduleId);

        return schedule?.PracticalSubjectFee ?? 0;
    }

    public async Task<bool> HasExistingPaymentAsync(int examScheduleId, int studentRegistrationId)
    {
        return await context.Set<PaymentRequestLog>()
            .AnyAsync(prl => prl.ExamScheduleId == examScheduleId
                          && prl.StudentRegistrationId == studentRegistrationId
                          && prl.PaymentRequestLogStatus == 1);
    }

    public async Task<bool> IsRejectedOnlyForScheduleAsync(int examScheduleId, string userId)
    {
        var studentErIds = await GetStudentExamRegistrationIdsAsync(userId);
        if (studentErIds.Count == 0) return false;

        var statuses = await context.ExamRegistrations!
            .AsNoTracking()
            .Where(er => er.ExamScheduleId == examScheduleId
                      && studentErIds.Contains(er.Id)
                      && er.IsAppliedByStudent == true
                      && er.IsActive)
            .Select(er => er.Status)
            .ToListAsync();

        return statuses.Count > 0 && statuses.All(s => s == RegistrationStatus.Rejected);
    }

    public async Task<string?> GetLatestRejectionReasonAsync(int examScheduleId, string userId)
    {
        var studentErIds = await GetStudentExamRegistrationIdsAsync(userId);
        if (studentErIds.Count == 0) return null;

        var remarks = await context.ExamRegistrations!
            .AsNoTracking()
            .Where(er => er.ExamScheduleId == examScheduleId
                      && studentErIds.Contains(er.Id)
                      && er.IsAppliedByStudent == true
                      && er.IsActive
                      && er.Status == RegistrationStatus.Rejected)
            .OrderByDescending(er => er.Id)
            .Select(er => er.Remarks)
            .FirstOrDefaultAsync();

        if (string.IsNullOrWhiteSpace(remarks)) return null;

        var lastEntry = remarks
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .LastOrDefault(line => line.Contains("[Rejected by", StringComparison.OrdinalIgnoreCase));

        if (lastEntry == null) return remarks.Trim();

        var idx = lastEntry.IndexOf(']');
        return idx >= 0 && idx + 1 < lastEntry.Length
            ? lastEntry[(idx + 1)..].Trim()
            : lastEntry;
    }

    public async Task<(bool Success, string Message)> ReapplyExamRegistrationAsync(
        int examScheduleId, string userId, int studentRegistrationId, List<int> subjectOfferingIds)
    {
        var requestedIds = (subjectOfferingIds ?? [])
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (requestedIds.Count == 0)
            return (false, "At least one subject must be selected.");

        var studentErIds = await GetStudentExamRegistrationIdsAsync(userId);
        if (studentErIds.Count == 0)
            return (false, "No exam application found for this schedule.");

        var registrations = await context.ExamRegistrations!
            .Where(er => er.ExamScheduleId == examScheduleId
                      && studentErIds.Contains(er.Id)
                      && er.IsAppliedByStudent == true
                      && er.IsActive)
            .ToListAsync();

        if (registrations.Any(er => er.Status != RegistrationStatus.Rejected))
            return (false, "This exam form is not rejected.");

        var target = registrations
            .OrderByDescending(er => er.Id)
            .First();

        var hasAdmitCard = await context.AdmitCards!
            .AsNoTracking()
            .AnyAsync(ac => ac.ExamRegistrationId == target.Id && ac.IsActive);
        if (hasAdmitCard)
            return (false, "This form cannot be re-applied because an admit card already exists.");

        var schedule = await context.ExamSchedules!
            .Include(es => es.SemesterInstance!)
                .ThenInclude(si => si.Semester)
            .Include(es => es.ExamType)
            .FirstOrDefaultAsync(es => es.Id == examScheduleId);

        if (schedule?.SemesterInstance == null)
            return (false, "Exam schedule not found.");

        var semesterNumber = schedule.SemesterInstance.Semester?.Number ?? 0;
        var resolvedVersion = await CurriculumVersionResolver.ResolveAsync(
            context, schedule.ProgramId, schedule.SemesterInstance.AcademicYearId);

        var validOfferings = await context.SubjectOfferings!
            .AsNoTracking()
            .Where(so => requestedIds.Contains(so.Id)
                      && so.ProgramId == schedule.ProgramId
                      && so.Semester != null && so.Semester.Number == semesterNumber
                      && (resolvedVersion == null || so.CurriculumVersionId == resolvedVersion.Value || so.CurriculumVersionId == null))
            .ToDictionaryAsync(so => so.Id);

        if (validOfferings.Count != requestedIds.Count)
            return (false, "One or more selected subjects are not offered for this exam schedule.");

        var paymentLog = await context.PaymentRequestLogs!
            .Where(prl => prl.ExamScheduleId == examScheduleId
                       && prl.StudentRegistrationId == studentRegistrationId
                       && prl.PaymentRequestLogStatus == 1)
            .OrderByDescending(pl => pl.Id)
            .FirstOrDefaultAsync();

        if (paymentLog == null)
            return (false, "Payment has not been confirmed for this form yet.");

        var existingResults = await context.ExamSubjectResults!
            .Where(esr => esr.ExamRegistrationId == target.Id)
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
        if (target.IsSupplementary)
        {
            previousScheduleIds = await context.ExamSchedules!
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(es => es.IsActive
                          && es.ProgramId == schedule.ProgramId
                          && es.SemesterInstance!.SemesterId == schedule.SemesterInstance.SemesterId
                          && es.Id != examScheduleId
                          && es.ExamType != null
                          && es.ExamType.Name != "Entrance")
                .Select(es => es.Id)
                .ToHashSetAsync();
        }

        foreach (var offeringId in finalIds.Where(id => !activeByOffering.ContainsKey(id)))
        {
            var offering = validOfferings[offeringId];

            float? carriedPractical = null;
            float? carriedPracticalInternal = null;
            float? carriedTheoryInternal = null;

            if (target.IsSupplementary && previousScheduleIds is { Count: > 0 })
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
                    carriedPractical = previousResult.ObtainedMarksPractical;
                    carriedPracticalInternal = previousResult.ObtainedMarksPracticalInternal;
                    carriedTheoryInternal = previousResult.ObtainedMarksTheoryInternal;
                }
            }

            context.ExamSubjectResults!.Add(new ExamSubjectResult
            {
                TenantId = target.TenantId,
                ExamRegistrationId = target.Id,
                SubjectOfferingId = offeringId,
                ExamScheduleId = examScheduleId,
                ExamTypeId = schedule.ExamTypeId,
                IsTheoryRegistered = offering.HasTheory,
                IsPracticalRegistered = offering.HasPractical,
                IsActive = true,
                IsSubmitted = false,
                IsSupplementary = target.IsSupplementary,
                ObtainedMarksPractical = carriedPractical,
                ObtainedMarksPracticalInternal = carriedPracticalInternal,
                ObtainedMarksTheoryInternal = carriedTheoryInternal
            });
        }

        var username = (await context.Users.FindAsync(userId))?.UserName ?? "student";
        target.Remarks = $"[Re-applied by {username} on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC]";
        target.Status = RegistrationStatus.Pending;
        target.RegistrationDate = DateTime.UtcNow;

        paymentLog.SelectedSubjectIds = string.Join(",", finalIds.OrderBy(id => id));

        await context.SaveChangesAsync();
        logger.LogInformation("ReapplyExamRegistrationAsync: ExamRegistration {RegId} re-applied for scheduleId={ScheduleId}, userId={UserId}, subjects={SubjectCount}",
            target.Id, examScheduleId, userId, finalIds.Count);

        return (true, "Your exam form has been re-applied successfully.");
    }

    public async Task<bool> HasExistingExamRegistrationAsync(int examScheduleId, string userId)
    {
        var studentErIds = await GetStudentExamRegistrationIdsAsync(userId);

        return await context.ExamRegistrations!
            .AsNoTracking()
            .AnyAsync(er => er.ExamScheduleId == examScheduleId
                         && studentErIds.Contains(er.Id)
                         && er.IsAppliedByStudent == true
                         && er.Status != RegistrationStatus.Rejected);
    }

    public async Task<List<PaymentType>> GetActivePaymentTypesAsync()
    {
        return await context.Set<PaymentType>()
            .AsNoTracking()
            .Where(pt => pt.IsActive)
            .ToListAsync();
    }

    public async Task<List<ResultRecord>> GetResultRecordsAsync(string registrationNumber)
    {
        var resultRecords = await context.ResultRecords!
            .AsNoTracking()
            .Include(rr => rr.AcademicYear)
            .Include(rr => rr.Program)
                .ThenInclude(p => p!.Faculty)
            .Include(rr => rr.ExamType)
            .Include(rr => rr.College)
            .Where(rr => rr.RegistrationNumber != null && rr.RegistrationNumber == registrationNumber)
            .ToListAsync();

        var schedules = await LoadExamSchedulesByIdsAsync(resultRecords.Select(rr => rr.ExamScheduleId).OfType<int>());
        foreach (var rr in resultRecords)
        {
            if (rr.ExamScheduleId.HasValue)
                rr.ExamSchedule = schedules.FirstOrDefault(s => s.Id == rr.ExamScheduleId.Value);
        }

        return resultRecords;
    }

    public async Task<List<ExamSubjectResult>> GetExamSubjectResultsAsync(int examRegistrationId)
    {
        return await context.ExamSubjectResults!
            .AsNoTracking()
            .Include(esr => esr.SubjectOffering).ThenInclude(so => so!.SubjectCatalog)
            .Where(esr => esr.ExamRegistrationId == examRegistrationId)
            .ToListAsync();
    }

    public async Task<StudentAdmission?> GetStudentAdmissionByUserIdAsync(string userId)
    {
        return await ResolveStudentAdmissionAsync(userId);
    }

    public async Task<ExamSchedule?> GetExamScheduleByIdAsync(int examScheduleId)
    {
        return await context.ExamSchedules!
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(es => es.Program)
            .Include(es => es.SemesterInstance).ThenInclude(si => si!.Semester)
            .Include(es => es.Level)
            .Include(es => es.SemesterInstance).ThenInclude(si => si!.AcademicYear)
            .FirstOrDefaultAsync(es => es.Id == examScheduleId);
    }

    public async Task<List<ExamSchedule>> GetExamSchedulesByIdsAsync(IEnumerable<int> ids)
    {
        return await LoadExamSchedulesByIdsAsync(ids);
    }

    public async Task<List<ExamRegistration>> GetStudentExamRegistrationsAsync(string userId)
    {
        var studentErIds = await GetStudentExamRegistrationIdsAsync(userId);
        if (studentErIds.Count == 0) return new();

        var registrations = await context.ExamRegistrations!
            .AsNoTracking()
            .Include(er => er.ExamSubjectResults!)
                .ThenInclude(esr => esr.SubjectOffering)
                    .ThenInclude(so => so!.SubjectCatalog)
            .Where(er => studentErIds.Contains(er.Id) && er.IsActive)
            .OrderByDescending(er => er.RegistrationDate)
            .ToListAsync();

        var schedules = await LoadExamSchedulesByIdsAsync(registrations.Select(er => er.ExamScheduleId));
        foreach (var er in registrations)
        {
            er.ExamSchedule = schedules.FirstOrDefault(s => s.Id == er.ExamScheduleId);
        }

        return registrations;
    }

    public async Task<List<ExamSubjectResult>> GetExamSubjectResultsForStudentAsync(string userId, int examScheduleId)
    {
        var studentErIds = await GetStudentExamRegistrationIdsAsync(userId);
        if (studentErIds.Count == 0) return new();

        var registrations = await context.ExamRegistrations!
            .AsNoTracking()
            .Include(er => er.ExamSubjectResults!)
                .ThenInclude(esr => esr.SubjectOffering)
                    .ThenInclude(so => so!.SubjectCatalog)
            .Where(er => studentErIds.Contains(er.Id)
                      && er.ExamScheduleId == examScheduleId
                      && er.IsActive)
            .ToListAsync();

        return registrations
            .SelectMany(er => er.ExamSubjectResults ?? Enumerable.Empty<ExamSubjectResult>())
            .Where(esr => esr.IsActive)
            .GroupBy(esr => esr.SubjectOfferingId)
            .Select(g => g.OrderByDescending(esr => esr.Id).First())
            .ToList();
    }

    public async Task UpdatePaymentRequestLogAsync(int logId, string transactionId, bool isSuccess, string responseData, string? responseMessage = null)
    {
        var log = await context.Set<PaymentRequestLog>().FirstOrDefaultAsync(prl => prl.Id == logId);
        if (log == null) return;

        log.TransactionId = transactionId;
        log.PaymentRequestLogStatus = isSuccess ? 1 : 0;

        context.Set<PaymentResponseLog>().Add(new PaymentResponseLog
        {
            PaymentRequestLogId = logId,
            ResponseTimestamp = DateTime.UtcNow,
            IsSuccess = isSuccess,
            ResponseMessage = responseMessage ?? (isSuccess ? "Payment verified via callback" : "Payment failed via callback"),
            FullResponse = responseData
        });

        await context.SaveChangesAsync();
    }

    public async Task<PaymentRequestLog?> GetPaymentLogByIdAsync(int logId)
    {
        return await context.Set<PaymentRequestLog>()
            .AsNoTracking()
            .FirstOrDefaultAsync(prl => prl.Id == logId);
    }

    public async Task CreateExamRegistrationAsync(int examScheduleId, string userId, decimal amount, List<int> subjectOfferingIds, int studentRegistrationId)
    {
        var schedule = await context.ExamSchedules!
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(es => es.SemesterInstance).ThenInclude(si => si!.Semester)
            .FirstOrDefaultAsync(es => es.Id == examScheduleId);

        if (schedule == null)
        {
            logger.LogWarning("CreateExamRegistrationAsync: ExamSchedule not found for scheduleId={ScheduleId}", examScheduleId);
            return;
        }

        var studentReg = await context.StudentRegistrations!
            .AsNoTracking()
            .FirstOrDefaultAsync(sr => sr.Id == studentRegistrationId);

        var admission = await context.StudentAdmissions!
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.AppUserId == userId);

        if (admission == null && studentReg != null)
        {
            admission = await context.StudentAdmissions!
                .AsNoTracking()
                .FirstOrDefaultAsync(sa => sa.CollegeId == studentReg.CollegeId
                                        && sa.ProgramsId == studentReg.ProgramId
                                        && sa.IsActive);

            if (admission == null && schedule.CollegeId > 0)
            {
                admission = await context.StudentAdmissions!
                    .AsNoTracking()
                    .FirstOrDefaultAsync(sa => sa.CollegeId == schedule.CollegeId
                                            && sa.ProgramsId == schedule.ProgramId
                                            && sa.IsActive);
            }
        }

        if (admission != null && string.IsNullOrEmpty(admission.AppUserId))
        {
            var trackedAdmission = await context.StudentAdmissions!
                .FirstOrDefaultAsync(sa => sa.Id == admission.Id);

            if (trackedAdmission != null)
            {
                trackedAdmission.AppUserId = userId;
                await context.SaveChangesAsync();
                logger.LogInformation("CreateExamRegistrationAsync: Linked AppUserId={UserId} to admissionId={AdmissionId}", userId, trackedAdmission.Id);
            }
        }

        int collegeId;
        int programsId;

        if (admission != null)
        {
            collegeId = admission.CollegeId;
            programsId = admission.ProgramsId;
        }
        else if (studentReg != null)
        {
            collegeId = studentReg.CollegeId;
            programsId = schedule.ProgramId;
            logger.LogInformation("CreateExamRegistrationAsync: No admission found, using StudentRegistration CollegeId={CollegeId} and ExamSchedule ProgramId={ProgramId} for userId={UserId}", collegeId, programsId, userId);
        }
        else
        {
            logger.LogWarning("CreateExamRegistrationAsync: Neither StudentAdmission nor StudentRegistration found for userId={UserId}, studentRegistrationId={SrId}", userId, studentRegistrationId);
            return;
        }

        var studentName = studentReg != null
            ? studentReg.FirstName.GetFullName(studentReg.MiddleName, studentReg.LastName)
            : "";

        var voucherNumber = $"VCH-{DateTime.UtcNow:yyyyMMdd}-{examScheduleId}-{studentRegistrationId}";
        var voucher = new ApplicationVoucher
        {
            TenantId = schedule.TenantId,
            VoucherNumber = voucherNumber,
            StudentName = studentName,
            StudentRegistrationId = studentRegistrationId,
            ContactNumber = studentReg?.ContactNumber ?? "",
            Amount = amount,
            VoucherDate = DateTime.UtcNow,
            Timestamp = DateTime.UtcNow,
            ExamScheduleId = examScheduleId
        };
        context.ApplicationVouchers!.Add(voucher);
        await context.SaveChangesAsync();

        var academicYearId = schedule.SemesterInstance!.AcademicYearId;

        int? semesterEnrollmentId = null;
        if (admission != null)
        {
            semesterEnrollmentId = await context.SemesterEnrollments!
                .AsNoTracking()
                .Where(se => se.StudentAdmissionId == admission.Id
                          && se.SemesterInstanceId == schedule.SemesterInstanceId
                          && se.EnrollmentStatus == StudentEnrollmentStatus.Active)
                .Select(se => (int?)se.Id)
                .FirstOrDefaultAsync();
        }

        var registration = new ExamRegistration
        {
            ExamScheduleId = examScheduleId,
            CollegeId = collegeId,
            AcademicYearId = academicYearId,
            ProgramsId = programsId,
            FeeEnclosed = amount,
            RegistrationDate = DateTime.UtcNow,
            Status = RegistrationStatus.Pending,
            IsActive = true,
            IsAppliedByStudent = true,
            ApplicationVoucherId = voucher.Id,
            IsSupplementary = IsReExamType(schedule.ExamType?.Name)
        };

        context.ExamRegistrations!.Add(registration);
        if (semesterEnrollmentId.HasValue)
        {
            registration.SemesterEnrollmentId = semesterEnrollmentId.Value;
        }
        await context.SaveChangesAsync();
        logger.LogInformation("CreateExamRegistrationAsync: ExamRegistration created. RegId={RegId}, ScheduleId={ScheduleId}, UserId={UserId}, VoucherId={VoucherId}", registration.Id, examScheduleId, userId, voucher.Id);

        var subjectOfferings = await context.SubjectOfferings!
            .AsNoTracking()
            .Where(so => subjectOfferingIds.Contains(so.Id))
            .ToListAsync();
        var subjectOfferingDict = subjectOfferings.ToDictionary(so => so.Id);

        HashSet<int>? previousScheduleIds = null;
        if (registration.IsSupplementary)
        {
            previousScheduleIds = await context.ExamSchedules!
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(es => es.IsActive
                          && es.ProgramId == programsId
                          && es.SemesterInstance!.SemesterId == schedule.SemesterInstance!.SemesterId
                          && es.Id != examScheduleId
                          && es.ExamType != null
                          && es.ExamType.Name != "Entrance")
                .Select(es => es.Id)
                .ToHashSetAsync();
        }

        foreach (var subjectOfferingId in subjectOfferingIds)
        {
            if (!subjectOfferingDict.TryGetValue(subjectOfferingId, out var subjectOffering))
                continue;

            float? carriedPractical = null;
            float? carriedPracticalInternal = null;
            float? carriedTheoryInternal = null;

            if (registration.IsSupplementary && previousScheduleIds != null && previousScheduleIds.Count > 0)
            {
                var previousResult = await context.ExamSubjectResults!
                    .AsNoTracking()
                    .Where(esr => esr.SubjectOfferingId == subjectOfferingId
                               && previousScheduleIds.Contains(esr.ExamScheduleId ?? 0)
                               && esr.ExamRegistration != null
                               && esr.ExamRegistration.IsActive
                               && esr.IsActive)
                    .OrderByDescending(esr => esr.Id)
                    .FirstOrDefaultAsync();

                if (previousResult != null)
                {
                    carriedPractical = previousResult.ObtainedMarksPractical;
                    carriedPracticalInternal = previousResult.ObtainedMarksPracticalInternal;
                    carriedTheoryInternal = previousResult.ObtainedMarksTheoryInternal;
                }
            }

            context.ExamSubjectResults!.Add(new ExamSubjectResult
            {
                ExamRegistrationId = registration.Id,
                SubjectOfferingId = subjectOfferingId,
                ExamScheduleId = examScheduleId,
                ExamTypeId = schedule.ExamTypeId,
                IsTheoryRegistered = subjectOffering.HasTheory,
                IsPracticalRegistered = subjectOffering.HasPractical,
                IsActive = true,
                IsSubmitted = false,
                IsSupplementary = registration.IsSupplementary,
                ObtainedMarksPractical = carriedPractical,
                ObtainedMarksPracticalInternal = carriedPracticalInternal,
                ObtainedMarksTheoryInternal = carriedTheoryInternal
            });
        }

        await context.SaveChangesAsync();
    }

    public async Task<int?> GetCurrentSemesterIdForStudentAsync(string userId)
    {
        var admission = await ResolveStudentAdmissionAsync(userId);
        if (admission == null) return null;

        return await context.SemesterEnrollments!
            .AsNoTracking()
            .Where(se => se.StudentAdmissionId == admission.Id
                      && se.EnrollmentStatus == StudentEnrollmentStatus.Active)
            .OrderByDescending(se => se.SemesterInstance!.AcademicYearId)
            .ThenByDescending(se => se.SemesterInstance!.Semester!.Number)
            .ThenByDescending(se => se.EnrolledDate)
            .Select(se => (int?)se.SemesterInstance!.SemesterId)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasFailedSubjectsInSemesterAsync(string userId, int semesterId, int programId)
    {

        var failedIds = await GetFailedSubjectOfferingIdsForSemesterAsync(userId, semesterId, programId);
        return failedIds.Count > 0;
    }

    public async Task<List<int>> GetFailedSubjectOfferingIdsForSemesterAsync(string userId, int semesterId, int programId)
    {
        var scheduleIds = await context.ExamSchedules!
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(es => es.IsActive
                      && es.ProgramId == programId
                      && es.SemesterInstance!.SemesterId == semesterId)
            .Select(es => es.Id)
            .ToListAsync();

        if (scheduleIds.Count == 0) return [];

        var studentErIds = await GetStudentExamRegistrationIdsAsync(userId);

        var results = await context.ExamRegistrations!
            .AsNoTracking()
            .Where(er => scheduleIds.Contains(er.ExamScheduleId) && er.IsActive
                      && (studentErIds.Count == 0 || studentErIds.Contains(er.Id)))
            .SelectMany(er => er.ExamSubjectResults!)
            .Where(esr => esr.IsActive)
            .ToListAsync();

        var latestPerSubject = results
            .GroupBy(esr => esr.SubjectOfferingId)
            .Select(g => g.OrderByDescending(esr => esr.Id).First())
            .ToList();

        return latestPerSubject
            .Where(esr => IsFailedGrade(esr.GradeLetter))
            .Select(esr => esr.SubjectOfferingId)
            .ToList();
    }

    private async Task<StudentAdmission?> ResolveStudentAdmissionAsync(string userId)
    {
        var admission = await context.StudentAdmissions!
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.AppUserId == userId);
        if (admission != null) return admission;

        var user = await context.Users.FindAsync(userId);
        if (user?.UserName == null) return null;

        var sr = await context.StudentRegistrations!
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.RegistrationNumber == user.UserName);
        if (sr == null) return null;

        if (sr.StudentAdmissionId.HasValue)
        {
            return await context.StudentAdmissions!
                .AsNoTracking()
                .FirstOrDefaultAsync(sa => sa.Id == sr.StudentAdmissionId.Value);
        }

        if (!sr.ProgramId.HasValue) return null;

        return await context.StudentAdmissions!
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.CollegeId == sr.CollegeId
                                    && sa.ProgramsId == sr.ProgramId
                                    && sa.IsActive);
    }

    private async Task<List<int>> GetStudentExamRegistrationIdsAsync(string userId)
    {
        var user = await context.Users.FindAsync(userId);
        if (user?.Email == null) return [];

        var sr = await context.StudentRegistrations!
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Email == user.Email && s.IsActive);
        if (sr == null) return [];

        var voucherIds = await context.ApplicationVouchers!
            .AsNoTracking()
            .Where(av => av.StudentRegistrationId == sr.Id)
            .Select(av => av.Id)
            .ToListAsync();

        if (voucherIds.Count == 0) return [];

        return await context.ExamRegistrations!
            .AsNoTracking()
            .Where(er => er.ApplicationVoucherId != null
                      && voucherIds.Contains(er.ApplicationVoucherId!.Value)
                      && er.IsActive)
            .Select(er => er.Id)
            .ToListAsync();
    }

    private async Task<List<ExamSchedule>> LoadExamSchedulesByIdsAsync(IEnumerable<int> scheduleIds)
    {
        var ids = scheduleIds.Distinct().ToList();
        if (ids.Count == 0) return new();

        return await context.ExamSchedules!
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(es => es.SemesterInstance).ThenInclude(si => si!.Semester)
            .Include(es => es.Level)
            .Include(es => es.ExamType)
            .Include(es => es.SemesterInstance).ThenInclude(si => si!.AcademicYear)
            .Include(es => es.Program)
                .ThenInclude(p => p!.Faculty)
            .Where(es => ids.Contains(es.Id))
            .ToListAsync();
    }

    private static bool IsFailedGrade(string? gradeLetter)
    {
        if (string.IsNullOrEmpty(gradeLetter)) return false;
        var upper = gradeLetter.Trim().ToUpperInvariant();
        return upper is "F" or "NG";
    }

    private static readonly HashSet<string> ReExamTypeNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Supplementary", "Partial", "Chance", "Special Chance"
    };

    public static bool IsReExamType(string? examTypeName) =>
        !string.IsNullOrEmpty(examTypeName) && ReExamTypeNames.Contains(examTypeName);

    public async Task<int> CreatePaymentRequestLogWithSubjectsAsync(int examScheduleId, int studentRegistrationId, decimal amount, string paymentMethod, string invoiceNumber, List<int> subjectOfferingIds, string? fullName = null, string? email = null, string? mobileNumber = null, string? dateOfBirthAd = null, string? transactionUuid = null)
    {
        var paymentTypes = await context.Set<PaymentType>()
            .AsNoTracking()
            .Where(pt => pt.IsActive && pt.PaymentTypeName != null)
            .ToListAsync();
        var paymentType = paymentTypes.FirstOrDefault(pt =>
            paymentMethod.Contains(pt.PaymentTypeName, StringComparison.OrdinalIgnoreCase));

        DateTime? dob = null;
        if (!string.IsNullOrEmpty(dateOfBirthAd) && DateTime.TryParse(dateOfBirthAd, out var parsedDob))
            dob = parsedDob;

        var requestContent = $"{{\"method\":\"{paymentMethod}\",\"amount\":{amount},\"subjects\":[{string.Join(",", subjectOfferingIds)}]}}";
        if (!string.IsNullOrEmpty(transactionUuid))
            requestContent = requestContent[..^1] + $",\"transaction_uuid\":\"{transactionUuid}\"}}";

        var log = new PaymentRequestLog
        {
            ExamScheduleId = examScheduleId,
            StudentRegistrationId = studentRegistrationId,
            Amount = amount,
            InvoiceNumber = invoiceNumber,
            FullName = fullName ?? "",
            Email = email,
            MobileNumber = mobileNumber,
            CollegeId = userContext.CollegeId,
            DateOfBirthAd = dob,
            FullRequestContent = requestContent,
            PaymentTypeId = paymentType?.Id ?? 0,
            ForwardedTimestamp = DateTime.UtcNow,
            StudentCount = subjectOfferingIds.Count,
            SelectedSubjectIds = string.Join(",", subjectOfferingIds),
            TransactionId = string.IsNullOrEmpty(transactionUuid) ? null : transactionUuid
        };

        context.Set<PaymentRequestLog>().Add(log);
        await context.SaveChangesAsync();

        if (subjectOfferingIds.Count > 0)
        {
            context.Set<PaymentPracticalSubjects>().Add(new PaymentPracticalSubjects
            {
                PaymentRequestLogId = log.Id,
                PracticalSubjectsCount = subjectOfferingIds.Count,
                TotalAmount = amount
            });
        }
        await context.SaveChangesAsync();

        return log.Id;
    }

    public async Task<int> CreatePaymentRequestLogAsync(int examScheduleId, int studentRegistrationId, decimal amount, string paymentMethod, string invoiceNumber, string? fullName = null, string? email = null, string? mobileNumber = null, string? dateOfBirthAd = null, string? transactionUuid = null)
    {
        var paymentTypes = await context.Set<PaymentType>()
            .AsNoTracking()
            .Where(pt => pt.IsActive && pt.PaymentTypeName != null)
            .ToListAsync();
        var paymentType = paymentTypes.FirstOrDefault(pt =>
            paymentMethod.Contains(pt.PaymentTypeName, StringComparison.OrdinalIgnoreCase));

        DateTime? dob = null;
        if (!string.IsNullOrEmpty(dateOfBirthAd) && DateTime.TryParse(dateOfBirthAd, out var parsedDob))
            dob = parsedDob;

        var requestContent = $"{{\"method\":\"{paymentMethod}\",\"amount\":{amount}}}";
        if (!string.IsNullOrEmpty(transactionUuid))
            requestContent = requestContent[..^1] + $",\"transaction_uuid\":\"{transactionUuid}\"}}";

        var log = new PaymentRequestLog
        {
            ExamScheduleId = examScheduleId,
            StudentRegistrationId = studentRegistrationId,
            Amount = amount,
            InvoiceNumber = invoiceNumber,
            FullName = fullName ?? "",
            Email = email,
            MobileNumber = mobileNumber,
            CollegeId = userContext.CollegeId,
            DateOfBirthAd = dob,
            FullRequestContent = requestContent,
            PaymentTypeId = paymentType?.Id ?? 0,
            ForwardedTimestamp = DateTime.UtcNow,
            StudentCount = 1,
            TransactionId = string.IsNullOrEmpty(transactionUuid) ? null : transactionUuid
        };

        context.Set<PaymentRequestLog>().Add(log);
        await context.SaveChangesAsync();
        return log.Id;
    }

    public async Task<List<AdmitCard>> GetAdmitCardsForStudentAsync(string userId, int studentRegistrationId)
    {
        var studentErIds = await GetStudentExamRegistrationIdsAsync(userId);

        var admitCards = await context.Set<AdmitCard>()
            .AsNoTracking()
            .Where(ac => ac.IsActive
                      && (studentErIds.Contains(ac.ExamRegistrationId)
                          || ac.StudentRegistrationId == studentRegistrationId))
            .OrderByDescending(ac => ac.GeneratedDate)
            .ToListAsync();

        var schedules = await LoadExamSchedulesByIdsAsync(admitCards.Select(ac => ac.ExamScheduleId));
        foreach (var ac in admitCards)
        {
            ac.ExamSchedule = schedules.FirstOrDefault(s => s.Id == ac.ExamScheduleId);
        }

        return admitCards;
    }

    public async Task<bool> HasAdmitCardForScheduleAsync(int examScheduleId, string userId, int studentRegistrationId)
    {
        var studentErIds = await GetStudentExamRegistrationIdsAsync(userId);

        return await context.Set<AdmitCard>()
            .AsNoTracking()
            .AnyAsync(ac => ac.ExamScheduleId == examScheduleId
                         && ac.IsActive
                         && (studentErIds.Contains(ac.ExamRegistrationId)
                             || ac.StudentRegistrationId == studentRegistrationId));
    }

    public async Task<int?> GetAdmitCardIdForScheduleAsync(int examScheduleId, string userId, int studentRegistrationId)
    {
        var studentErIds = await GetStudentExamRegistrationIdsAsync(userId);

        return await context.Set<AdmitCard>()
            .AsNoTracking()
            .Where(ac => ac.ExamScheduleId == examScheduleId
                      && ac.IsActive
                      && (studentErIds.Contains(ac.ExamRegistrationId)
                          || ac.StudentRegistrationId == studentRegistrationId))
            .Select(ac => (int?)ac.Id)
            .FirstOrDefaultAsync();
    }


    public async Task<List<PaymentRequestLog>> GetPaymentHistoryForStudentAsync(int studentRegistrationId)
    {
        var payments = await context.Set<PaymentRequestLog>()
            .AsNoTracking()
            .Include(prl => prl.PaymentType)
            .Where(prl => prl.StudentRegistrationId == studentRegistrationId
                       && prl.PaymentRequestLogStatus == 1)
            .OrderByDescending(prl => prl.ForwardedTimestamp)
            .ToListAsync();

        var schedules = await LoadExamSchedulesByIdsAsync(payments.Select(prl => prl.ExamScheduleId));
        foreach (var payment in payments)
        {
            payment.ExamSchedule = schedules.FirstOrDefault(s => s.Id == payment.ExamScheduleId);
        }

        logger.LogInformation("GetPaymentHistoryForStudentAsync: studentRegId={StudentRegId}, paymentCount={Count}", studentRegistrationId, payments.Count);
        return payments;
    }

    public async Task<PaymentRequestLog?> GetPaymentLogByInvoiceNumberAsync(string invoiceNumber)
    {
        return await context.Set<PaymentRequestLog>()
            .AsNoTracking()
            .FirstOrDefaultAsync(prl => prl.InvoiceNumber == invoiceNumber);
    }

    public async Task<PaymentRequestLog?> FindPendingPaymentLogByStudentAsync(int studentRegistrationId)
    {
        return await context.Set<PaymentRequestLog>()
            .AsNoTracking()
            .Where(prl => prl.StudentRegistrationId == studentRegistrationId
                       && prl.PaymentRequestLogStatus == null)
            .OrderByDescending(prl => prl.ForwardedTimestamp)
            .FirstOrDefaultAsync();
    }

    public async Task<List<string>> GetMissingMandatoryProfileFieldsAsync(string? userId, string? userEmail, string? phoneNumber, string? profilePath, string? signaturePath)
    {
        var missing = new List<string>();

        var registration = !string.IsNullOrWhiteSpace(userId)
            ? await GetStudentRegistrationByUserIdAsync(userId)
            : await GetStudentRegistrationByEmailAsync(userEmail ?? "");
        var hasPhone = !string.IsNullOrWhiteSpace(phoneNumber)
            || (registration != null && (!string.IsNullOrWhiteSpace(registration.ContactNumber) || !string.IsNullOrWhiteSpace(registration.Phone)));

        if (!hasPhone)
            missing.Add("Phone Number");

        if (registration == null)
        {
            missing.Add("Province");
            missing.Add("District");
            missing.Add("Local Level");
            missing.Add("Gender");
            missing.Add("Ethnicity");
        }
        else
        {
            var permanentAddress = registration.PermanentAddress;
            if (permanentAddress?.LocalLevel == null)
                missing.Add("Local Level");
            if (permanentAddress?.LocalLevel?.District == null)
                missing.Add("District");
            if (permanentAddress?.LocalLevel?.District?.Province == null)
                missing.Add("Province");
            if (registration.Gender == null || registration.GenderId <= 0)
                missing.Add("Gender");
            if (registration.Ethnicity == null)
                missing.Add("Ethnicity");
        }

        if (string.IsNullOrWhiteSpace(profilePath))
            missing.Add("Profile Photo");

        if (string.IsNullOrWhiteSpace(signaturePath))
            missing.Add("Student Signature");

        return missing;
    }
}
