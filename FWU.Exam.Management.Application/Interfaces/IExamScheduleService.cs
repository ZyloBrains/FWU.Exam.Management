using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Exams;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IExamScheduleService
{
    Task<(List<ExamSchedule> Items, int TotalCount)> GetExamSchedulesAsync(int page, int pageSize, string? search, string sort, string sortDir, string? examTypeName = null);
    Task<List<ExamSchedule>> GetFilteredItemsAsync(string? search, string? examTypeName = null);
    Task<ExamSchedule?> GetExamScheduleByIdAsync(int id);
    Task CreateExamScheduleAsync(ExamSchedule examSchedule);
    Task UpdateExamScheduleAsync(ExamSchedule examSchedule);
    Task DeleteExamScheduleAsync(int id);
    Task<bool> ExamScheduleExistsAsync(int id);
    Task DeactivateExpiredSchedulesAsync();
    Task<ExamScheduleSelectListsDto> GetSelectListDataAsync(ExamSchedule? examSchedule = null);
    Task<List<SelectOption>> GetSemestersByAcademicYearAsync(int academicYearId, int? programId = null);
    Task<List<SelectOption>> GetCurriculumVersionsByProgramAsync(int programId);
    Task<List<SelectOption>> GetSemestersByCurriculumVersionAsync(int curriculumVersionId);
}
