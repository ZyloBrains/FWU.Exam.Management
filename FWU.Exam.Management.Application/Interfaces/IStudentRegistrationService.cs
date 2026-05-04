using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Students;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IStudentRegistrationService
{
    Task<List<StudentRegistration>> GetAllStudentRegistrationsAsync();
    Task<StudentRegistration?> GetStudentRegistrationByIdAsync(int id);
    Task<int> CreateStudentRegistrationAsync(StudentRegistration studentRegistration, string? permanentLocalLevelId, string? permanentWardNumber, string? permanentToleStreet, string? permanentHouseNumber);
    Task UpdateStudentRegistrationAsync(StudentRegistration studentRegistration, string? permanentLocalLevelId, string? permanentWardNumber, string? permanentToleStreet, string? permanentHouseNumber);
    Task DeleteStudentRegistrationAsync(int id);
    Task<bool> StudentRegistrationExistsAsync(int id);
    Task<(List<object> Data, int TotalCount)> GetPagedDataAsync(string searchTerm, int page, int pageSize);
    Task UpdateStatusAsync(int id, bool isActive);
    Task<object> GetSelectListDataAsync(StudentRegistration? studentRegistration = null);
    Task<List<object>> GetDistrictsByProvinceAsync(int provinceId);
    Task<List<object>> GetLocalLevelsByDistrictAsync(int districtId);
    Task<List<Province>> GetProvincesAsync();
}
