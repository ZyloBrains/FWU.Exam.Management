using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IDepartmentService
{
    Task<(List<Department> Items, int TotalCount)> GetDepartmentsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<Department>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<Department?> GetDepartmentByIdAsync(int id);
    Task CreateDepartmentAsync(Department department);
    Task UpdateDepartmentAsync(Department department);
    Task DeleteDepartmentAsync(int id);
    Task<bool> DepartmentExistsAsync(int id);
}
