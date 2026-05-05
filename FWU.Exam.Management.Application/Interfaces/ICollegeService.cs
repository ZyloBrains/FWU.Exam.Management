using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ICollegeService
{
    Task<(List<College> Items, int TotalCount)> GetCollegesAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<College>> GetFilteredItemsAsync(string? search, string sort, string sortDir);
    Task<College?> GetCollegeByIdAsync(int id);
    Task<int> CreateCollegeAsync(College college, string? localLevelId, string? wardNumber, string? toleStreet, string? houseNumber);
    Task<int> UpdateCollegeAsync(College college, string? localLevelId, string? wardNumber, string? toleStreet, string? houseNumber);
    Task DeleteCollegeAsync(int id);
    Task<bool> CollegeExistsAsync(int id);
    Task<List<CollegeType>> GetCollegeTypesAsync();
}
