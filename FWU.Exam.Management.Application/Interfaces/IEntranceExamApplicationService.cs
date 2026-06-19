using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Enums;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IEntranceExamApplicationService
{
    Task<int> SubmitApplicationAsync(EntranceExamApplication application, string? permanentLocalLevelId, string? permanentWardNumber, string? permanentToleStreet, string? permanentHouseNumber);
    Task<EntranceExamApplication?> GetApplicationByIdAsync(int id);
    Task<(List<EntranceExamApplicationListDto> Data, int TotalCount)> GetPagedApplicationsAsync(string? search, ApplicationStatus? status, int? programId, int? academicYearId, int page, int pageSize);
    Task ReviewApplicationAsync(int id, ApplicationStatus status, string? remarks);
    Task DeleteApplicationAsync(int id);
    Task<bool> ApplicationExistsAsync(int id);
    Task<EntranceExamApplicationSelectListsDto> GetSelectListsAsync();
    Task<List<SelectOption>> GetDistrictsByProvinceAsync(int provinceId);
    Task<List<SelectOption>> GetLocalLevelsByDistrictAsync(int districtId);
    List<Province> GetProvinces();
    Task<List<EntranceExamApplication>> GetAllApplicationsAsync(string? search, ApplicationStatus? status, int? programId, int? academicYearId);
    Task<int> ConvertToAdmissionAsync(int applicationId);
    Task<bool> IsExamScheduleOpenAsync(int programId, int collegeId, int academicYearId);
    Task<ApplicationVoucher?> VerifyPaymentAsync(string transactionCode, string fullName, string contactNumber);
    Task<bool> HasExistingVoucherAsync(int scheduleId, string studentName, string contactNumber);
    Task<int> CreateEsewaPaymentLogAsync(int scheduleId, string studentName, string contactNumber, int paymentTypeId, string transactionUuid);
    Task<int?> GetPaymentLogIdByTransactionUuidAsync(string transactionUuid);
    Task<ApplicationVoucher?> CompleteEsewaPaymentAsync(int logId, decimal amount);
    Task LogEsewaResponseAsync(int logId, string? transactionCode, bool isSuccess, string responseData, string? responseMessage = null);
    Task<EntranceExamApplicationSelectListsDto> GetStepFormSelectListsAsync();
    Task<int> SubmitStepApplicationAsync(EntranceExamApplication application, string? permanentLocalLevelId, string? permanentWardNumber, string? permanentToleStreet, string? permanentHouseNumber, int voucherId);
    Task<List<SelectOption>> GetDistrictsAsync();
    Task<decimal?> GetEntranceFeeForProgramAsync(int programId, int academicYearId);
    Task<List<AvailableScheduleDto>> GetAvailableExamSchedulesAsync();
    Task<ApplicationVoucher?> InitiatePaymentAsync(int scheduleId, string studentName, string contactNumber, int paymentTypeId);
    Task<ApplicationVoucher?> GetVoucherByIdAsync(int voucherId);
    Task<List<PaymentType>> GetActivePaymentTypesAsync();
}
