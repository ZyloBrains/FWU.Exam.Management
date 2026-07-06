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
    Task<List<SubjectOffering>> GetSubjectOfferingsForScheduleAsync(int examScheduleId);
    Task<decimal> GetExamFeeForScheduleAsync(int examScheduleId);
    Task<decimal> GetPracticalSubjectFeeForScheduleAsync(int examScheduleId);
    Task<bool> HasExistingPaymentAsync(int examScheduleId, int studentRegistrationId);
    Task<List<PaymentType>> GetActivePaymentTypesAsync();
    Task<List<ResultRecord>> GetResultRecordsAsync(string registrationNumber);
    Task<List<ExamSubjectResult>> GetExamSubjectResultsAsync(int examRegistrationId);
    Task<ExamSchedule?> GetExamScheduleByIdAsync(int examScheduleId);
    Task<StudentAdmission?> GetStudentAdmissionByUserIdAsync(string userId);
    Task<int> CreatePaymentRequestLogAsync(int examScheduleId, int studentRegistrationId, decimal amount, string paymentMethod, string invoiceNumber, string? fullName = null, string? email = null, string? mobileNumber = null, string? dateOfBirthAd = null, int? collegeId = null);
    Task<int> CreatePaymentRequestLogWithSubjectsAsync(int examScheduleId, int studentRegistrationId, decimal amount, string paymentMethod, string invoiceNumber, List<int> subjectOfferingIds, string? fullName = null, string? email = null, string? mobileNumber = null, string? dateOfBirthAd = null, int? collegeId = null);
    Task UpdatePaymentRequestLogAsync(int logId, string transactionId, bool isSuccess, string responseData, string? responseMessage = null);
    Task<List<int>> GetFailedSubjectOfferingIdsAsync(string userId, int semesterId);
    Task<List<SubjectOffering>> GetSubjectOfferingsByProgramAsync(int programId);
    Task<PaymentRequestLog?> GetPaymentLogByIdAsync(int logId);
    Task CreateExamRegistrationAsync(int examScheduleId, string userId, decimal amount, List<int> subjectOfferingIds);
    Task<List<ExamRegistration>> GetStudentExamRegistrationsAsync(string userId);
    Task<List<ExamSubjectResult>> GetExamSubjectResultsForStudentAsync(string userId, int examScheduleId);
}
