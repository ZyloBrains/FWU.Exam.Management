using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IFacultyService
{
    Task<(List<Faculty> Items, int TotalCount)> GetFacultiesAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<Faculty>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<Faculty?> GetFacultyByIdAsync(int id);
    Task CreateFacultyAsync(Faculty faculty);
    Task UpdateFacultyAsync(Faculty faculty);
    Task DeleteFacultyAsync(int id);
    Task<bool> FacultyExistsAsync(int id);
}
