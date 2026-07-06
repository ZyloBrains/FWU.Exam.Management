using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Exams;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IExamSubjectResultService
{
    Task<(List<ExamSubjectResult> Items, int TotalCount)> GetExamSubjectResultsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? examScheduleId = null, int? examRegistrationId = null, int? facultyId = null);
    Task<List<ExamSubjectResult>> GetFilteredItemsAsync(string? search, int? examScheduleId = null, int? facultyId = null);
    Task<ExamSubjectResult?> GetExamSubjectResultByIdAsync(int id);
    Task CreateExamSubjectResultAsync(ExamSubjectResult examSubjectResult);
    Task UpdateExamSubjectResultAsync(ExamSubjectResult examSubjectResult);
    Task DeleteExamSubjectResultAsync(int id);
    Task<bool> ExamSubjectResultExistsAsync(int id);
    ExamSubjectResultSelectListsDto GetSelectListData(ExamSubjectResult? examSubjectResult = null, int? collegeId = null, int? facultyId = null);
}
