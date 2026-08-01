using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Interfaces;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ISemesterService
{
    Task<(List<Semester> Items, int TotalCount)> GetSemestersAsync(int page, int pageSize, string? search, string sort, string sortDir, IUserContext userContext);
    Task<List<Semester>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir, IUserContext userContext);
    Task<List<Semester>> GetSemestersByFacultyAsync(int? facultyId);
    Task<Semester?> GetSemesterByIdAsync(int id);
    Task CreateSemesterAsync(Semester semester);
    Task UpdateSemesterAsync(Semester semester);
    Task DeleteSemesterAsync(int id);
    Task<bool> SemesterExistsAsync(int id);

    Task<List<Semester>> GetSemestersByProgramAsync(int programId);
    Task<List<int>> GetAssignedSemesterIdsAsync(int programId);
    Task<List<Semester>> GetAssignableSemestersAsync(IUserContext userContext);
    Task SetProgramSemestersAsync(int programId, IEnumerable<int> semesterIds);
    Task<bool> IsSemesterAssignedToProgramAsync(int programId, int semesterId);
    Task<bool> IsSemesterAssignedToAnyProgramAsync(int semesterId);
    Task AutoLinkProgramSemestersAsync();
}
