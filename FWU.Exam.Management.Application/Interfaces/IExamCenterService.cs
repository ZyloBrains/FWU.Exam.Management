using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IExamCenterService
{
    Task<(List<ExamCenter> Items, int TotalCount)> GetExamCentersAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<ExamCenter>> GetFilteredItemsAsync(string? search, string sort, string sortDir);
    Task<ExamCenter?> GetExamCenterByIdAsync(int id);
    Task CreateExamCenterAsync(ExamCenter examCenter);
    Task CreateExamCenterWithCollegesAsync(ExamCenter examCenter, List<int> venueCollegeIds, List<int> sourceCollegeIds);
    Task UpdateExamCenterAsync(ExamCenter examCenter);
    Task UpdateExamCenterWithCollegesAsync(ExamCenter examCenter, List<int> venueCollegeIds, List<int> sourceCollegeIds);
    Task DeleteExamCenterAsync(int id);
    Task<bool> ExamCenterExistsAsync(int id);
    Task<List<College>> GetVenueCollegesAsync(int examCenterId);
    Task<List<College>> GetSourceCollegesAsync(int examCenterId);
}
