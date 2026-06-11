using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class StudentDashboardService(AppDbContext context) : IStudentDashboardService
{
    public async Task<StudentRegistration?> GetStudentRegistrationByEmailAsync(string email)
    {
        return await context.StudentRegistrations!
            .AsNoTracking()
            .Include(s => s.AcademicYear)
            .Include(s => s.Level)
            .Include(s => s.Department)
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

        if (studentAdmission == null)
            return [];

        var query = context.ExamSchedules!
            .AsNoTracking()
            .Include(es => es.Program)
            .Include(es => es.Level)
            .Include(es => es.Semester)
            .Include(es => es.AcademicYear)
            .Where(es => es.IsActive && es.ProgramId == studentAdmission.ProgramsId);

        if (student.LevelId != 0)
        {
            query = query.Where(es => es.LevelId == null || es.LevelId == student.LevelId);
        }

        if (student.DepartmentId != 0)
        {
            query = query.Where(es => es.Program != null && es.Program.DepartmentId == student.DepartmentId);
        }

        return await query.ToListAsync();
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

    public async Task<decimal> GetExamFeeForScheduleAsync(int examScheduleId)
    {
        var billTitle = await context.Set<BillTitle>()
            .AsNoTracking()
            .FirstOrDefaultAsync(bt => bt.ExamScheduleId == examScheduleId && bt.IsActive);

        if (billTitle?.Amount.HasValue == true)
            return billTitle.Amount.Value;

        var examFee = await context.ExamFees!
            .AsNoTracking()
            .FirstOrDefaultAsync(ef => ef.ExamScheduleId == examScheduleId);

        return examFee?.Amount ?? 0;
    }

    public async Task<decimal> GetPracticalChargeForProgramAsync(int programId)
    {
        var charge = await context.Set<ProgramSubjectPracticalCharge>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pspc => pspc.ProgramsId == programId);

        return charge?.PracticalSubjectCharge ?? 0;
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

    public async Task UpdatePaymentRequestLogAsync(int logId, string transactionId, bool isSuccess, string responseData)
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
            ResponseMessage = isSuccess ? "Payment verified successfully" : "Payment failed",
            FullResponse = responseData
        });

        await context.SaveChangesAsync();
    }

    public async Task<List<int>> GetFailedSubjectOfferingIdsAsync(string userId, int semesterId)
    {
        var admission = await context.StudentAdmissions!
            .AsNoTracking()
            .FirstOrDefaultAsync(sa => sa.AppUserId == userId);

        if (admission == null) return [];

        var enrollment = await context.Set<SemesterEnrollment>()
            .AsNoTracking()
            .Include(se => se.ExamRegistrations!)
                .ThenInclude(er => er.ExamSubjectResults!)
            .FirstOrDefaultAsync(se => se.StudentAdmissionId == admission.Id && se.SemesterId == semesterId);

        if (enrollment?.ExamRegistrations == null || !enrollment.ExamRegistrations.Any(er => er.IsActive))
            return [];

        var results = enrollment.ExamRegistrations
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

    private static bool IsFailedGrade(string? gradeLetter)
    {
        if (string.IsNullOrEmpty(gradeLetter)) return false;
        var upper = gradeLetter.Trim().ToUpperInvariant();
        return upper is "F" or "NG";
    }

    public async Task<int> CreatePaymentRequestLogWithSubjectsAsync(int examScheduleId, int studentRegistrationId, decimal amount, string paymentMethod, string invoiceNumber, List<int> subjectOfferingIds, string? fullName = null, string? email = null, string? mobileNumber = null, string? dateOfBirthAd = null, int? collegeId = null)
    {
        var paymentType = await context.Set<PaymentType>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pt => pt.IsActive && pt.PaymentTypeName != null &&
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
            CollegeId = collegeId,
            DateOfBirthAd = dob,
            FullRequestContent = $"{{\"method\":\"{paymentMethod}\",\"amount\":{amount},\"subjects\":[{string.Join(",", subjectOfferingIds)}]}}",
            PaymentTypeId = paymentType?.Id ?? 0,
            ForwardedTimestamp = DateTime.UtcNow,
            StudentCount = subjectOfferingIds.Count
        };

        context.Set<PaymentRequestLog>().Add(log);
        await context.SaveChangesAsync();

        foreach (var subjectId in subjectOfferingIds)
        {
            context.Set<PaymentPracticalSubjects>().Add(new PaymentPracticalSubjects
            {
                PaymentRequestLogId = log.Id,
                PracticalSubjectsCount = 1,
                TotalAmount = amount
            });
        }
        await context.SaveChangesAsync();

        return log.Id;
    }

    public async Task<int> CreatePaymentRequestLogAsync(int examScheduleId, int studentRegistrationId, decimal amount, string paymentMethod, string invoiceNumber, string? fullName = null, string? email = null, string? mobileNumber = null, string? dateOfBirthAd = null, int? collegeId = null)
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
            CollegeId = collegeId,
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
}
