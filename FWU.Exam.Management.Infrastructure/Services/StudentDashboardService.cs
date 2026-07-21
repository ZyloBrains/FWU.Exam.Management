using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class StudentDashboardService(AppDbContext context, IUserContext userContext) : IStudentDashboardService
{
    public async Task<StudentRegistration?> GetStudentRegistrationByEmailAsync(string email)
    {
        return await context.StudentRegistrations!
            .AsNoTracking()
            .Include(s => s.AcademicYear)
            .Include(s => s.Level)
            .Include(s => s.College)
            .Include(s => s.Gender)
            .Include(s => s.StudentCategory)
            .Include(s => s.Ethnicity)
            .Include(s => s.PermanentAddress).ThenInclude(a => a!.LocalLevel).ThenInclude(l => l!.District).ThenInclude(d => d!.Province)
            .Include(s => s.CurrentAddress).ThenInclude(a => a!.LocalLevel).ThenInclude(l => l!.District).ThenInclude(d => d!.Province)
            .FirstOrDefaultAsync(s => s.Email != null && s.Email == email);
    }

    public async Task<List<ExamSchedule>> GetExamSchedulesForStudentAsync(StudentRegistration student, string userId)
    {
        var studentAdmission = await context.StudentAdmissions!
            .AsNoTracking()
            .Where(sa => sa.IsActive)
            .FirstOrDefaultAsync(sa => sa.AppUserId == userId);

        int programId;
        if (studentAdmission != null)
        {
            programId = studentAdmission.ProgramsId;
        }
        else if (student.ProgramId.HasValue)
        {
            programId = student.ProgramId.Value;
        }
        else
        {
            return [];
        }

        var query = context.ExamSchedules!
            .AsNoTracking()
            .Include(es => es.ExamType)
            .Include(es => es.Program)
            .Include(es => es.Level)
            .Include(es => es.Semester)
            .Include(es => es.AcademicYear)
            .Where(es => es.IsActive && es.ProgramId == programId && (es.ExamType == null || es.ExamType.Name != "Entrance"));

        if (student.LevelId != 0)
        {
            query = query.Where(es => es.LevelId == null || es.LevelId == student.LevelId);
        }

        var allSchedules = await query.ToListAsync();

        var scheduleIds = allSchedules.Select(s => s.Id).ToList();
        var existingRegistrationScheduleIds = await context.ExamRegistrations!
            .AsNoTracking()
            .Where(er => scheduleIds.Contains(er.ExamScheduleId)
                      && er.IsActive
                      && er.IsAppliedByStudent == true)
            .Select(er => er.ExamScheduleId)
            .ToListAsync();

        var filtered = new List<ExamSchedule>();
        foreach (var schedule in allSchedules)
        {
            if (existingRegistrationScheduleIds.Contains(schedule.Id))
                continue;

            var isSupplementary = schedule.ExamType?.Name == "Supplementary";

            if (isSupplementary)
            {
                var hasFailed = await HasFailedSubjectsInSemesterAsync(userId, schedule.SemesterId);
                if (!hasFailed)
                    continue;
            }
            else
            {
                if (schedule.Semester != null && schedule.Semester.Number > 1)
                {
                    var prevSubmitted = await HasSubmittedPreviousSemesterExamFormAsync(userId, schedule.SemesterId, programId);
                    if (!prevSubmitted)
                        continue;
                }
            }

            filtered.Add(schedule);
        }

        return filtered;
    }

    public async Task<List<SubjectOffering>> GetSubjectOfferingsForScheduleAsync(int examScheduleId)
    {
        var schedule = await context.ExamSchedules!
            .AsNoTracking()
            .FirstOrDefaultAsync(es => es.Id == examScheduleId);

        if (schedule == null) return new List<SubjectOffering>();

        return await context.SubjectOfferings!
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .Where(so => so.ProgramId == schedule.ProgramId && so.SemesterId == schedule.SemesterId)
            .OrderBy(so => so.DisplayOrder)
            .ToListAsync();
    }

    public async Task<List<SubjectOffering>> GetSubjectOfferingsByProgramAsync(int programId)
    {
        return await context.SubjectOfferings!
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .Include(so => so.Semester)
            .Where(so => so.ProgramId == programId)
            .OrderBy(so => so.Semester!.Number)
            .ThenBy(so => so.DisplayOrder)
            .ToListAsync();
    }

    public async Task<decimal> GetExamFeeForScheduleAsync(int examScheduleId)
    {
        var schedule = await context.ExamSchedules!
            .AsNoTracking()
            .FirstOrDefaultAsync(es => es.Id == examScheduleId);

        return schedule?.ExamFee ?? 0;
    }

    public async Task<decimal> GetPracticalSubjectFeeForScheduleAsync(int examScheduleId)
    {
        var schedule = await context.ExamSchedules!
            .AsNoTracking()
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

    public async Task<List<PaymentType>> GetActivePaymentTypesAsync()
    {
        return await context.Set<PaymentType>()
            .AsNoTracking()
            .Where(pt => pt.IsActive)
            .ToListAsync();
    }

    public async Task<List<ResultRecord>> GetResultRecordsAsync(string registrationNumber)
    {
        return await context.ResultRecords!
            .AsNoTracking()
            .Include(rr => rr.AcademicYear)
            .Include(rr => rr.Program)
            .Include(rr => rr.ExamType)
            .Include(rr => rr.College)
            .Include(rr => rr.ExamSchedule)
            .Where(rr => rr.RegistrationNumber != null && rr.RegistrationNumber == registrationNumber)
            .ToListAsync();
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
        return await context.StudentAdmissions!
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.AppUserId == userId);
    }

    public async Task<ExamSchedule?> GetExamScheduleByIdAsync(int examScheduleId)
    {
        return await context.ExamSchedules!
            .AsNoTracking()
            .Include(es => es.Program)
            .Include(es => es.Semester)
            .Include(es => es.Level)
            .Include(es => es.AcademicYear)
            .FirstOrDefaultAsync(es => es.Id == examScheduleId);
    }

    public async Task<List<ExamRegistration>> GetStudentExamRegistrationsAsync(string userId)
    {
        var admission = await context.StudentAdmissions!
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.AppUserId == userId);

        if (admission == null) return new();

        return await context.ExamRegistrations!
            .AsNoTracking()
            .Include(er => er.ExamSchedule)
            .Include(er => er.ExamSubjectResults!)
                .ThenInclude(esr => esr.SubjectOffering)
                    .ThenInclude(so => so!.SubjectCatalog)
            .Where(er => er.ProgramsId == admission.ProgramsId && er.IsActive)
            .OrderByDescending(er => er.RegistrationDate)
            .ToListAsync();
    }

    public async Task<List<ExamSubjectResult>> GetExamSubjectResultsForStudentAsync(string userId, int examScheduleId)
    {
        var admission = await context.StudentAdmissions!
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.AppUserId == userId);

        if (admission == null) return new();

        var registrations = await context.ExamRegistrations!
            .AsNoTracking()
            .Include(er => er.ExamSubjectResults!)
                .ThenInclude(esr => esr.SubjectOffering)
                    .ThenInclude(so => so!.SubjectCatalog)
            .Where(er => er.ProgramsId == admission.ProgramsId
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

    public async Task CreateExamRegistrationAsync(int examScheduleId, string userId, decimal amount, List<int> subjectOfferingIds)
    {
        var schedule = await context.ExamSchedules!
            .AsNoTracking()
            .Include(es => es.Semester)
            .FirstOrDefaultAsync(es => es.Id == examScheduleId);

        if (schedule == null) return;

        var admission = await context.StudentAdmissions!
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.AppUserId == userId);

        if (admission == null) return;

        var academicYearId = schedule.AcademicYearId;

        var registration = new ExamRegistration
        {
            ExamScheduleId = examScheduleId,
            CollegeId = admission.CollegeId,
            AcademicYearId = academicYearId,
            ProgramsId = admission.ProgramsId,
            FeeEnclosed = amount,
            RegistrationDate = DateTime.UtcNow,
            Status = RegistrationStatus.Pending,
            IsActive = true,
            IsAppliedByStudent = true
        };

        context.ExamRegistrations!.Add(registration);
        await context.SaveChangesAsync();

        var subjectOfferings = await context.SubjectOfferings!
            .AsNoTracking()
            .Where(so => subjectOfferingIds.Contains(so.Id))
            .ToListAsync();
        var subjectOfferingDict = subjectOfferings.ToDictionary(so => so.Id);

        foreach (var subjectOfferingId in subjectOfferingIds)
        {
            if (!subjectOfferingDict.TryGetValue(subjectOfferingId, out var subjectOffering))
                continue;

            context.ExamSubjectResults!.Add(new ExamSubjectResult
            {
                ExamRegistrationId = registration.Id,
                SubjectOfferingId = subjectOfferingId,
                ExamScheduleId = examScheduleId,
                IsTheoryRegistered = subjectOffering.HasTheory,
                IsPracticalRegistered = subjectOffering.HasPractical,
                IsActive = true,
                IsSubmitted = false
            });
        }

        await context.SaveChangesAsync();
    }

    public async Task<List<int>> GetFailedSubjectOfferingIdsAsync(string userId, int semesterId)
    {
        var admission = await context.StudentAdmissions!
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.AppUserId == userId);

        if (admission == null) return [];

        var enrollments = await context.Set<SemesterEnrollment>()
            .AsNoTracking()
            .Include(se => se.ExamRegistrations!)
                .ThenInclude(er => er.ExamSubjectResults!)
            .Where(se => se.StudentAdmissionId == admission.Id && se.SemesterId != semesterId)
            .ToListAsync();

        if (!enrollments.Any(e => e.ExamRegistrations != null && e.ExamRegistrations.Any(er => er.IsActive)))
            return [];

        var results = enrollments
            .Where(e => e.ExamRegistrations != null)
            .SelectMany(e => e.ExamRegistrations!)
            .Where(er => er.IsActive)
            .SelectMany(er => er.ExamSubjectResults ?? Enumerable.Empty<ExamSubjectResult>())
            .Where(esr => esr.IsActive)
            .ToList();

        var latestPerSubject = results
            .GroupBy(esr => esr.SubjectOfferingId)
            .Select(g => g.OrderByDescending(esr => esr.Id).First())
            .ToList();

        return latestPerSubject
            .Where(esr => IsFailedGrade(esr.GradeLetter))
            .Select(esr => esr.SubjectOfferingId)
            .ToList();
    }

    public async Task<bool> HasSubmittedPreviousSemesterExamFormAsync(string userId, int currentSemesterId, int programId)
    {
        var currentSemester = await context.Semesters!
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == currentSemesterId);

        if (currentSemester == null) return false;

        Semester? previousSemester;
        if (currentSemester.Number > 1)
        {
            previousSemester = await context.Semesters!
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Year == currentSemester.Year
                                       && s.Number == currentSemester.Number - 1);
        }
        else
        {
            previousSemester = await context.Semesters!
                .AsNoTracking()
                .Where(s => s.Year == currentSemester.Year - 1)
                .OrderByDescending(s => s.Number)
                .FirstOrDefaultAsync();
        }

        if (previousSemester == null) return true;

        var previousScheduleIds = await context.ExamSchedules!
            .AsNoTracking()
            .Where(es => es.IsActive
                      && es.ProgramId == programId
                      && es.SemesterId == previousSemester.Id
                      && es.ExamType != null
                      && es.ExamType.Name != "Entrance")
            .Select(es => es.Id)
            .ToListAsync();

        if (previousScheduleIds.Count == 0) return true;

        var admission = await context.StudentAdmissions!
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.AppUserId == userId);

        if (admission == null) return false;

        return await context.ExamRegistrations!
            .AsNoTracking()
            .AnyAsync(er => previousScheduleIds.Contains(er.ExamScheduleId)
                         && er.IsAppliedByStudent == true
                         && er.IsActive);
    }

    public async Task<bool> HasFailedSubjectsInSemesterAsync(string userId, int semesterId)
    {
        var admission = await context.StudentAdmissions!
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.AppUserId == userId);

        if (admission == null) return false;

        var failedIds = await GetFailedSubjectOfferingIdsForSemesterAsync(userId, semesterId);
        return failedIds.Count > 0;
    }

    public async Task<List<int>> GetFailedSubjectOfferingIdsForSemesterAsync(string userId, int semesterId)
    {
        var admission = await context.StudentAdmissions!
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.AppUserId == userId);

        if (admission == null) return [];

        var scheduleIds = await context.ExamSchedules!
            .AsNoTracking()
            .Where(es => es.IsActive
                      && es.ProgramId == admission.ProgramsId
                      && es.SemesterId == semesterId)
            .Select(es => es.Id)
            .ToListAsync();

        if (scheduleIds.Count == 0) return [];

        var results = await context.ExamRegistrations!
            .AsNoTracking()
            .Where(er => scheduleIds.Contains(er.ExamScheduleId) && er.IsActive)
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

    private static bool IsFailedGrade(string? gradeLetter)
    {
        if (string.IsNullOrEmpty(gradeLetter)) return false;
        var upper = gradeLetter.Trim().ToUpperInvariant();
        return upper is "F" or "NG";
    }

    public async Task<int> CreatePaymentRequestLogWithSubjectsAsync(int examScheduleId, int studentRegistrationId, decimal amount, string paymentMethod, string invoiceNumber, List<int> subjectOfferingIds, string? fullName = null, string? email = null, string? mobileNumber = null, string? dateOfBirthAd = null)
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
            FullRequestContent = $"{{\"method\":\"{paymentMethod}\",\"amount\":{amount},\"subjects\":[{string.Join(",", subjectOfferingIds)}]}}",
            PaymentTypeId = paymentType?.Id ?? 0,
            ForwardedTimestamp = DateTime.UtcNow,
            StudentCount = subjectOfferingIds.Count,
            SelectedSubjectIds = string.Join(",", subjectOfferingIds)
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

    public async Task<int> CreatePaymentRequestLogAsync(int examScheduleId, int studentRegistrationId, decimal amount, string paymentMethod, string invoiceNumber, string? fullName = null, string? email = null, string? mobileNumber = null, string? dateOfBirthAd = null)
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
            FullRequestContent = $"{{\"method\":\"{paymentMethod}\",\"amount\":{amount}}}",
            PaymentTypeId = paymentType?.Id ?? 0,
            ForwardedTimestamp = DateTime.UtcNow,
            StudentCount = 1
        };

        context.Set<PaymentRequestLog>().Add(log);
        await context.SaveChangesAsync();
        return log.Id;
    }

    public async Task<List<AdmitCard>> GetAdmitCardsForStudentAsync(string userId)
    {
        var admission = await context.StudentAdmissions!
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.AppUserId == userId);

        if (admission == null) return [];

        var examRegistrationIds = await context.ExamRegistrations!
            .AsNoTracking()
            .Where(er => er.ProgramsId == admission.ProgramsId
                      && er.CollegeId == admission.CollegeId
                      && er.IsActive)
            .Select(er => er.Id)
            .ToListAsync();

        if (examRegistrationIds.Count == 0) return [];

        return await context.Set<AdmitCard>()
            .AsNoTracking()
            .Include(ac => ac.ExamSchedule)
                .ThenInclude(es => es!.Semester)
            .Where(ac => examRegistrationIds.Contains(ac.ExamRegistrationId) && ac.IsActive)
            .OrderByDescending(ac => ac.GeneratedDate)
            .ToListAsync();
    }

    public async Task<bool> HasAdmitCardForScheduleAsync(int examScheduleId, string userId)
    {
        var admission = await context.StudentAdmissions!
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.AppUserId == userId);

        if (admission == null) return false;

        var examRegistrationIds = await context.ExamRegistrations!
            .AsNoTracking()
            .Where(er => er.ProgramsId == admission.ProgramsId
                      && er.CollegeId == admission.CollegeId
                      && er.IsActive)
            .Select(er => er.Id)
            .ToListAsync();

        if (examRegistrationIds.Count == 0) return false;

        return await context.Set<AdmitCard>()
            .AsNoTracking()
            .AnyAsync(ac => ac.ExamScheduleId == examScheduleId
                         && examRegistrationIds.Contains(ac.ExamRegistrationId)
                         && ac.IsActive);
    }

    public async Task<List<PaymentRequestLog>> GetPaymentHistoryForStudentAsync(string email)
    {
        var registration = await context.StudentRegistrations!
            .AsNoTracking()
            .FirstOrDefaultAsync(sr => sr.Email != null && sr.Email == email);

        if (registration == null) return [];

        return await context.Set<PaymentRequestLog>()
            .AsNoTracking()
            .Include(prl => prl.ExamSchedule)
                .ThenInclude(es => es!.Semester)
            .Include(prl => prl.PaymentType)
            .Where(prl => prl.StudentRegistrationId == registration.Id)
            .OrderByDescending(prl => prl.ForwardedTimestamp)
            .ToListAsync();
    }
}
