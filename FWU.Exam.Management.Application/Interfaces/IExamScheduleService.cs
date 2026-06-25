using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Exams;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IExamScheduleService
{
    Task<(List<ExamSchedule> Items, int TotalCount)> GetExamSchedulesAsync(int page, int pageSize, string? search, string sort, string sortDir, int? collegeId = null, int? facultyId = null, string? examTypeName = null);
    Task<List<ExamSchedule>> GetFilteredItemsAsync(string? search, int? collegeId = null, int? facultyId = null, string? examTypeName = null);
    Task<ExamSchedule?> GetExamScheduleByIdAsync(int id);
    Task CreateExamScheduleAsync(ExamSchedule examSchedule);
    Task UpdateExamScheduleAsync(ExamSchedule examSchedule);
    Task DeleteExamScheduleAsync(int id);
    Task<bool> ExamScheduleExistsAsync(int id);
    Task DeactivateExpiredSchedulesAsync();
    ExamScheduleSelectListsDto GetSelectListData(ExamSchedule? examSchedule = null);
}
