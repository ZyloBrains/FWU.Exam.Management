using FWU.Exam.Management.Domain.Entities.Exams;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IExamCenterService
{
    Task<(List<ExamCenter> Items, int TotalCount)> GetExamCentersAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<ExamCenter>> GetFilteredItemsAsync(string? search, string sort, string sortDir);
    Task<ExamCenter?> GetExamCenterByIdAsync(int id);
    Task CreateExamCenterAsync(ExamCenter examCenter);
    Task UpdateExamCenterAsync(ExamCenter examCenter);
    Task DeleteExamCenterAsync(int id);
    Task<bool> ExamCenterExistsAsync(int id);
}
