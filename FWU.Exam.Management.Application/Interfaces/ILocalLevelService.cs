using FWU.Exam.Management.Domain.Entities.Location;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ILocalLevelService
{
    Task<(List<LocalLevel> Items, int TotalCount)> GetLocalLevelsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<LocalLevel>> GetFilteredLocalLevelsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<LocalLevel?> GetLocalLevelByIdAsync(int id);
    Task CreateLocalLevelAsync(LocalLevel localLevel);
    Task UpdateLocalLevelAsync(LocalLevel localLevel);
    Task DeleteLocalLevelAsync(int id);
    Task<bool> LocalLevelExistsAsync(int id);
    Task<List<District>> GetActiveDistrictsAsync();
}
