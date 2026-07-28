using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Exams;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IExamSubjectResultService
{
    Task<(List<ExamSubjectResult> Items, int TotalCount)> GetExamSubjectResultsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? examScheduleId = null, int? examRegistrationId = null);
    Task<List<ExamSubjectResult>> GetFilteredItemsAsync(string? search, int? examScheduleId = null);
    Task<ExamSubjectResult?> GetExamSubjectResultByIdAsync(int id);
    Task CreateExamSubjectResultAsync(ExamSubjectResult examSubjectResult);
    Task UpdateExamSubjectResultAsync(ExamSubjectResult examSubjectResult);
    Task DeleteExamSubjectResultAsync(int id);
    Task<bool> ExamSubjectResultExistsAsync(int id);
    Task<ExamSubjectResultSelectListsDto> GetSelectListDataAsync(ExamSubjectResult? examSubjectResult = null);
    Task<(List<ExamRegistrationGroupedDto> Items, int TotalCount)> GetRegistrationsWithSubjectResultsAsync(int page, int pageSize, string? search, int? examScheduleId = null);
}
