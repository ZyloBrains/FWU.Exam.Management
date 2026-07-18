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
    Task<List<SelectOption>> GetFacultiesByLevelAsync(int levelId);
    Task<List<SelectOption>> GetProgramsByCollegeAsync(int collegeId, int? levelId = null);
    Task<List<Province>> GetProvincesAsync();
    Task SaveQualificationsAsync(int studentRegistrationId, List<StudentQualification> qualifications);
    Task<List<StudentQualification>> GetQualificationsByRegistrationAsync(int studentRegistrationId);
    Task SaveGuardiansAsync(int studentRegistrationId, StudentGuardian guardian);
    Task<StudentGuardian?> GetGuardianByRegistrationAsync(int studentRegistrationId);
    Task<string?> GenerateRegistrationNumberAsync(int studentRegistrationId);
}
