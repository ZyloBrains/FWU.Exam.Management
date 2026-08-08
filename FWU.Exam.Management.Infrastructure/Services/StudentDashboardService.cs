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
            .FirstOrDefaultAsync(s => (s.Email != null && s.Email == email) || s.RegistrationNumber == email);
    }

    public async Task<List<ExamSchedule>> GetExamSchedulesForStudentAsync(StudentRegistration student)
    {
        var enrolledSemesterIds = await context.SemesterEnrollments!
            .AsNoTracking()
            .Where(se => se.StudentAdmissionId == student.StudentAdmissionId)
            .Select(se => se.SemesterId)
            .Distinct()
            .ToListAsync();

        if (enrolledSemesterIds.Count == 0) return [];

        var query = context.ExamSchedules!
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Include(es => es.ExamType)
            .Include(es => es.Program)
            .Include(es => es.Level)
            .Include(es => es.Semester)
            .Include(es => es.AcademicYear)
            .Where(es => es.IsActive
                      && es.ProgramId == student.ProgramId
                      && enrolledSemesterIds.Contains(es.SemesterId)
                      && es.ExamType!.Name != "Entrance");

        if (student.LevelId != 0)
        {
            query = query.Where(es => es.LevelId == null || es.LevelId == student.LevelId);
        }

        return await query.ToListAsync();
    }

    public async Task<List<SubjectOffering>> GetSubjectOfferingsForScheduleAsync(int examScheduleId)
    {
        var schedule = await context.ExamSchedules!
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(es => es.Id == examScheduleId);

        if (schedule == null) return new List<SubjectOffering>();

        var query = context.SubjectOfferings!
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .ThenInclude(sc => sc!.SubjectType)
            .Where(so => so.ProgramId == schedule.ProgramId && so.SemesterId == schedule.SemesterId);

        if (schedule.CurriculumVersionId is > 0)
            query = query.Where(so => so.CurriculumVersionId == schedule.CurriculumVersionId);

        return await query
            .OrderBy(so => so.DisplayOrder)
            .ToListAsync();
    }

    public async Task<List<SubjectOffering>> GetSubjectOfferingsForStudentAsync(string userId, int programId)
    {
        var admission = await ResolveStudentAdmissionAsync(userId);

        var query = context.SubjectOfferings!
            .AsNoTracking()
            .Include(so => so.SubjectCatalog)
            .Include(so => so.Semester)
            .Where(so => so.ProgramId == programId);

        var activeVersionId = await context.CurriculumVersions!
            .AsNoTracking()
            .Where(cv => cv.ProgramId == programId && cv.IsActive)
            .Select(cv => (int?)cv.Id)
            .FirstOrDefaultAsync();

        var academicYearId = admission?.AcademicYearId;

        if (admission != null)
        {
            var enrolledSemesterId = await context.Set<SemesterEnrollment>()
                .AsNoTracking()
                .Where(se => se.StudentAdmissionId == admission.Id
                          && se.EnrollmentStatus == StudentEnrollmentStatus.Active
                          && se.DropDate == null)
                .OrderByDescending(se => se.EnrolledDate)
                .Select(se => (int?)se.SemesterId)
                .FirstOrDefaultAsync();

            if (enrolledSemesterId.HasValue)
            {
                if (activeVersionId.HasValue)
                {
                    return await query
                        .Where(so => so.SemesterId == enrolledSemesterId.Value
                                  && so.CurriculumVersionId == activeVersionId.Value)
                        .OrderBy(so => so.DisplayOrder)
                        .ToListAsync();
                }

                return await query
                    .Where(so => so.SemesterId == enrolledSemesterId.Value)
                    .OrderBy(so => so.DisplayOrder)
                    .ToListAsync();
            }
        }
        else
        {
            var user = await context.Users.FindAsync(userId);
            if (user?.Email != null)
            {
                var registration = await context.StudentRegistrations!
                    .AsNoTracking()
                    .FirstOrDefaultAsync(sr => (sr.Email != null && sr.Email == user.Email) || sr.RegistrationNumber == user.Email);
                if (registration != null && registration.AcademicYearId != 0)
                {
                    academicYearId = registration.AcademicYearId;
                }
            }
        }

        if (!academicYearId.HasValue) return new List<SubjectOffering>();

        query = query.Where(so => so.Semester != null && so.Semester.AcademicYearId == academicYearId.Value);

        var firstSemesterNumber = await query
            .Where(so => so.Semester != null)
            .Select(so => (int?)so.Semester!.Number)
            .OrderBy(n => n)
            .FirstOrDefaultAsync();

        if (!firstSemesterNumber.HasValue) return new List<SubjectOffering>();

        return await query
            .Where(so => so.Semester != null && so.Semester.Number == firstSemesterNumber.Value)
            .OrderBy(so => so.DisplayOrder)
            .ToListAsync();
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

    public async Task<bool> HasExistingExamRegistrationAsync(int examScheduleId, string userId)
    {
        var studentErIds = await GetStudentExamRegistrationIdsAsync(userId);

        return await context.ExamRegistrations!
            .AsNoTracking()
            .AnyAsync(er => er.ExamScheduleId == examScheduleId
                         && studentErIds.Contains(er.Id)
                         && er.IsAppliedByStudent == true);
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
            .Include(es => es.Semester)
            .Include(es => es.Level)
            .Include(es => es.AcademicYear)
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
            .Include(es => es.Semester)
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

        var academicYearId = schedule.AcademicYearId;

        int? semesterEnrollmentId = null;
        if (admission != null)
        {
            semesterEnrollmentId = await context.SemesterEnrollments!
                .AsNoTracking()
                .Where(se => se.StudentAdmissionId == admission.Id
                          && se.SemesterId == schedule.SemesterId
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
            Status = RegistrationStatus.Registered,
            IsActive = true,
            IsAppliedByStudent = true,
            ApplicationVoucherId = voucher.Id
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

        foreach (var subjectOfferingId in subjectOfferingIds)
        {
            if (!subjectOfferingDict.TryGetValue(subjectOfferingId, out var subjectOffering))
                continue;

            context.ExamSubjectResults!.Add(new ExamSubjectResult
            {
                ExamRegistrationId = registration.Id,
                SubjectOfferingId = subjectOfferingId,
                ExamScheduleId = examScheduleId,
                ExamTypeId = schedule.ExamTypeId,
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
        var admission = await ResolveStudentAdmissionAsync(userId);
        if (admission == null) return [];

        var scheduleIds = await context.ExamSchedules!
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(es => es.IsActive
                      && es.ProgramId == admission.ProgramsId
                      && es.SemesterId != semesterId
                      && es.ExamType != null
                      && es.ExamType.Name != "Entrance")
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

    public async Task<int?> GetCurrentSemesterIdForStudentAsync(string userId)
    {
        var admission = await ResolveStudentAdmissionAsync(userId);
        if (admission == null) return null;

        return await context.SemesterEnrollments!
            .AsNoTracking()
            .Where(se => se.StudentAdmissionId == admission.Id
                      && se.EnrollmentStatus == StudentEnrollmentStatus.Active)
            .OrderByDescending(se => se.Semester!.Year)
            .ThenByDescending(se => se.Semester!.Number)
            .ThenByDescending(se => se.EnrolledDate)
            .Select(se => (int?)se.SemesterId)
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
                      && es.SemesterId == semesterId)
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

    private async Task<StudentAdmission?> ResolveStudentAdmissionAsync(string userId, string? email = null)
    {
        var admission = await context.StudentAdmissions!
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.AppUserId == userId);
        if (admission != null) return admission;

        if (string.IsNullOrEmpty(email))
        {
            var user = await context.Users.FindAsync(userId);
            email = user?.Email;
        }
        if (string.IsNullOrEmpty(email)) return null;

        var sr = await context.StudentRegistrations!
            .AsNoTracking()
            .FirstOrDefaultAsync(s => (s.Email != null && s.Email == email) || s.RegistrationNumber == email);
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
            .Include(es => es.Semester)
            .Include(es => es.Level)
            .Include(es => es.ExamType)
            .Include(es => es.AcademicYear)
            .Include(es => es.Program)
            .Where(es => ids.Contains(es.Id))
            .ToListAsync();
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
}
