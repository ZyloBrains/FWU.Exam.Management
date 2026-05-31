using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Students;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IStudentRegistrationService
{
    Task<List<StudentRegistration>> GetAllStudentRegistrationsAsync(List<int>? collegeIds = null);
    Task<StudentRegistration?> GetStudentRegistrationByIdAsync(int id);
    Task<int> CreateStudentRegistrationAsync(StudentRegistration studentRegistration, string? permanentLocalLevelId, string? permanentWardNumber, string? permanentToleStreet, string? permanentHouseNumber);
    Task UpdateStudentRegistrationAsync(StudentRegistration studentRegistration, string? permanentLocalLevelId, string? permanentWardNumber, string? permanentToleStreet, string? permanentHouseNumber);
    Task DeleteStudentRegistrationAsync(int id);
    Task<bool> StudentRegistrationExistsAsync(int id);
    Task<(List<StudentRegistrationListDto> Data, int TotalCount)> GetPagedDataAsync(string searchTerm, int page, int pageSize, List<int>? collegeIds = null);
    Task UpdateStatusAsync(int id, bool isActive);
    Task<StudentRegistrationSelectListsDto> GetSelectListDataAsync(StudentRegistration? studentRegistration = null);
    Task<List<SelectOption>> GetDistrictsByProvinceAsync(int provinceId);
    Task<List<SelectOption>> GetLocalLevelsByDistrictAsync(int districtId);
    List<Province> GetProvinces();
}
