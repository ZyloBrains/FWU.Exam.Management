using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities.Exams;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IExamTypeService
{
    Task<(List<ExamType> Items, int TotalCount)> GetExamTypesAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<ExamType>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<ExamType?> GetExamTypeByIdAsync(int id);
    Task CreateExamTypeAsync(ExamType examType);
    Task UpdateExamTypeAsync(ExamType examType);
    Task DeleteExamTypeAsync(int id);
    Task<bool> ExamTypeExistsAsync(int id);
}
