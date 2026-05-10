using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.EntranceExams;
using FWU.Exam.Management.Domain.Entities.Location;
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
}
