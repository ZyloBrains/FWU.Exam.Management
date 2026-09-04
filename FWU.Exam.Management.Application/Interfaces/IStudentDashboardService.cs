using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Helpers;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IStudentDashboardService
{
    Task<StudentRegistration?> GetStudentRegistrationByEmailAsync(string email);
    Task<List<ExamSchedule>> GetExamSchedulesForStudentAsync(StudentRegistration student, string userId);
    Task<bool> IsScheduleVisibleToStudentAsync(StudentRegistration student, string userId, int examScheduleId);
    Task<StudentRegistration?> GetStudentRegistrationByUserIdAsync(string userId);
    Task<List<SubjectOffering>> GetSubjectOfferingsForScheduleAsync(int examScheduleId);
    Task<List<SubjectOffering>> GetReExamSelectableOfferingsAsync(int examScheduleId, string userId);
    Task<decimal> GetExamFeeForScheduleAsync(int examScheduleId);
    Task<decimal> GetPracticalSubjectFeeForScheduleAsync(int examScheduleId);
    Task<bool> HasExistingPaymentAsync(int examScheduleId, int studentRegistrationId);
    Task<bool> IsRejectedOnlyForScheduleAsync(int examScheduleId, string userId);
    Task<string?> GetLatestRejectionReasonAsync(int examScheduleId, string userId);
    Task<(bool Success, string Message)> ReapplyExamRegistrationAsync(int examScheduleId, string userId, int studentRegistrationId, List<int> subjectOfferingIds, Dictionary<int, ReExamLegs>? subjectLegs = null);
    Task<bool> HasExistingExamRegistrationAsync(int examScheduleId, string userId);
    Task<List<PaymentType>> GetActivePaymentTypesAsync();
    Task<List<ResultRecord>> GetResultRecordsAsync(string registrationNumber);
    Task<List<ExamSubjectResult>> GetExamSubjectResultsAsync(int examRegistrationId);
    Task<ExamSchedule?> GetExamScheduleByIdAsync(int examScheduleId);
    Task<List<ExamSchedule>> GetExamSchedulesByIdsAsync(IEnumerable<int> ids);
    Task<StudentAdmission?> GetStudentAdmissionByUserIdAsync(string userId);
    Task<int> CreatePaymentRequestLogAsync(int examScheduleId, int studentRegistrationId, decimal amount, string paymentMethod, string invoiceNumber, string? fullName = null, string? email = null, string? mobileNumber = null, string? dateOfBirthAd = null, string? transactionUuid = null);
    Task<int> CreatePaymentRequestLogWithSubjectsAsync(int examScheduleId, int studentRegistrationId, decimal amount, string paymentMethod, string invoiceNumber, Dictionary<int, ReExamLegs> subjectSelection, string? fullName = null, string? email = null, string? mobileNumber = null, string? dateOfBirthAd = null, string? transactionUuid = null);
    Task UpdatePaymentRequestLogAsync(int logId, string transactionId, bool isSuccess, string responseData, string? responseMessage = null);
    Task UpdatePaymentRequestLogTransactionIdAsync(int logId, string transactionId);
    Task<decimal> ComputeSelectionFeeAsync(int examScheduleId, Dictionary<int, ReExamLegs> selection);
    Task<bool> TryCompleteApplyAgainTopUpAsync(int logId, string userId);
    Task SupersedeOpenApplyAgainPaymentsAsync(int examScheduleId, int studentRegistrationId, int exceptLogId);
    Task<bool> HasOpenApplyAgainPaymentAsync(int examScheduleId, int studentRegistrationId);
    Task<List<SubjectOffering>> GetSubjectOfferingsForStudentAsync(string userId, int programId);
    Task<PaymentRequestLog?> GetPaymentLogByIdAsync(int logId);
    Task CreateExamRegistrationAsync(int examScheduleId, string userId, decimal amount, List<int> subjectOfferingIds, int studentRegistrationId, Dictionary<int, ReExamLegs>? subjectLegs = null);
    Task<List<ExamRegistration>> GetStudentExamRegistrationsAsync(string userId);
    Task<List<ExamSubjectResult>> GetExamSubjectResultsForStudentAsync(string userId, int examScheduleId);
    Task<int?> GetCurrentSemesterIdForStudentAsync(string userId);
    Task<bool> HasFailedSubjectsInSemesterAsync(string userId, int semesterId, int programId);
    bool IsReExamType(string? examTypeName);
    Task<bool> HasAnyExamResultsInSemesterAsync(string userId, int semesterId, int programId);
    Task<bool> IsEligibleForReExamAsync(string userId, int semesterId, int programId);
    Task<List<int>> GetFailedSubjectOfferingIdsForSemesterAsync(string userId, int semesterId, int programId);
    Task<List<FailedSubjectOption>> GetFailedSubjectOptionsForStudentAsync(int examScheduleId, string userId);
    Task<List<AdmitCard>> GetAdmitCardsForStudentAsync(string userId, int studentRegistrationId);
    Task<bool> HasAdmitCardForScheduleAsync(int examScheduleId, string userId, int studentRegistrationId);
    Task<int?> GetAdmitCardIdForScheduleAsync(int examScheduleId, string userId, int studentRegistrationId);
    Task<List<PaymentRequestLog>> GetPaymentHistoryForStudentAsync(int studentRegistrationId);
    Task<PaymentRequestLog?> GetPaymentLogByInvoiceNumberAsync(string invoiceNumber);
    Task<PaymentRequestLog?> FindPendingPaymentLogByStudentAsync(int studentRegistrationId);
    Task<List<string>> GetMissingMandatoryProfileFieldsAsync(string? userId, string? userEmail, string? phoneNumber, string? profilePath, string? signaturePath);
}
