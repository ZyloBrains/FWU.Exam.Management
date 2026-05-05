using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities.Location;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IDistrictService
{
    Task<(List<District> Items, int TotalCount)> GetDistrictsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<District>> GetFilteredDistrictsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<District?> GetDistrictByIdAsync(int id);
    Task CreateDistrictAsync(District district);
    Task UpdateDistrictAsync(District district);
    Task DeleteDistrictAsync(int id);
    Task<bool> DistrictExistsAsync(int id);
    Task<List<Province>> GetActiveProvincesAsync();
}
