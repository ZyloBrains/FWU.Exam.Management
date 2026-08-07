using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Students;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IStudentAdmissionService
{
    Task<(List<StudentAdmission> Items, int TotalCount)> GetAdmissionsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<StudentAdmission>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<StudentAdmission?> GetAdmissionByIdAsync(int id);
    Task<int> CreateAdmissionAsync(StudentAdmission admission);
    Task UpdateAdmissionAsync(StudentAdmission admission);
    Task DeleteAdmissionAsync(int id);
    Task<bool> AdmissionExistsAsync(int id);
    Task CompleteAdmissionAsync(int id, string userId);
    Task<List<Program>> GetCollegeProgramsAsync(int collegeId);
    Task<List<SelectOption>> GetCollegeSelectListAsync();
    Task<StudentAdmission?> GetAdmissionByUserIdAsync(string userId);
    Task<List<StudentRegistration>> GetAvailableStudentRegistrationsAsync(int collegeId);
    Task<string?> GetAppUserIdByEmailAsync(string email);
}
