using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities.Colleges;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ICollegeTypeService
{
    Task<(List<CollegeType> Items, int TotalCount)> GetCollegeTypesAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<CollegeType>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<CollegeType?> GetCollegeTypeByIdAsync(int id);
    Task CreateCollegeTypeAsync(CollegeType collegeType);
    Task UpdateCollegeTypeAsync(CollegeType collegeType);
    Task DeleteCollegeTypeAsync(int id);
    Task<bool> CollegeTypeExistsAsync(int id);
}
