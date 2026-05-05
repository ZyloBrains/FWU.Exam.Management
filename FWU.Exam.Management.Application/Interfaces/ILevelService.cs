using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ILevelService
{
    Task<(List<Level> Items, int TotalCount)> GetLevelsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<Level>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<Level?> GetLevelByIdAsync(int id);
    Task CreateLevelAsync(Level level);
    Task UpdateLevelAsync(Level level);
    Task DeleteLevelAsync(int id);
    Task<bool> LevelExistsAsync(int id);
}
