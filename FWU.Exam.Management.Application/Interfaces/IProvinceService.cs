using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities.Location;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IProvinceService
{
    Task<(List<Province> Items, int TotalCount)> GetProvincesAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<Province>> GetFilteredProvincesAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<Province?> GetProvinceByIdAsync(int id);
    Task CreateProvinceAsync(Province province);
    Task UpdateProvinceAsync(Province province);
    Task DeleteProvinceAsync(int id);
    Task<bool> ProvinceExistsAsync(int id);
}
