using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IFacultyService
{
    Task<List<Faculty>> GetAllFacultiesAsync();
    Task<(List<Faculty> Items, int TotalCount)> GetFacultiesPagedAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<Faculty?> GetFacultyByIdAsync(int id);
    Task<Faculty?> GetFacultyByOfficeCodeAsync(string officeCode);
    Task<string> CreateFacultyAsync(Faculty faculty, string adminPassword);
    Task UpdateFacultyAsync(Faculty faculty);
    Task DeleteFacultyAsync(int id);
    Task<(bool canDelete, List<string> blockingEntities)> CheckDeleteDependenciesAsync(int id);
    Task<bool> FacultyExistsAsync(int id);
}
