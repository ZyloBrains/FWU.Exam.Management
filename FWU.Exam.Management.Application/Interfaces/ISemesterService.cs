using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities.Semesters;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ISemesterService
{
    Task<(List<Semester> Items, int TotalCount)> GetSemestersAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<Semester>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<Semester?> GetSemesterByIdAsync(int id);
    Task CreateSemesterAsync(Semester semester);
    Task UpdateSemesterAsync(Semester semester);
    Task DeleteSemesterAsync(int id);
    Task<bool> SemesterExistsAsync(int id);
}
