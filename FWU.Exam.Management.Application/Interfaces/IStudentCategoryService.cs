using FWU.Exam.Management.Domain.Entities.Students;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IStudentCategoryService
{
    Task<(List<StudentCategory> Items, int TotalCount)> GetStudentCategoriesAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<StudentCategory>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<StudentCategory?> GetStudentCategoryByIdAsync(int id);
    Task CreateStudentCategoryAsync(StudentCategory studentCategory);
    Task UpdateStudentCategoryAsync(StudentCategory studentCategory);
    Task DeleteStudentCategoryAsync(int id);
    Task<bool> StudentCategoryExistsAsync(int id);
}
