using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IProgramService
{
    Task<(List<Program> Items, int TotalCount)> GetProgramsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<Program>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<Program?> GetProgramByIdAsync(int id);
    Task CreateProgramAsync(Program program);
    Task UpdateProgramAsync(Program program);
    Task DeleteProgramAsync(int id);
    Task<bool> ProgramExistsAsync(int id);
    Task<(List<Board> Boards, List<Department> Departments, List<Level> Levels)> GetSelectListsAsync(int? boardId = null, int? departmentId = null, int? levelId = null);
}
