using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IFacultyService
{
    Task<List<Faculty>> GetAllFacultiesAsync();
    Task<Faculty?> GetFacultyByIdAsync(int id);
    Task<Faculty?> GetFacultyByOfficeCodeAsync(string officeCode);
    Task CreateFacultyAsync(Faculty faculty);
    Task UpdateFacultyAsync(Faculty faculty);
    Task DeleteFacultyAsync(int id);
    Task<bool> FacultyExistsAsync(int id);
}
