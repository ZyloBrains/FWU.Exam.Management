using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Exams;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IExamRegistrationService
{
    Task<(List<ExamRegistration> Items, int TotalCount)> GetExamRegistrationsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? examScheduleId = null);
    Task<List<ExamRegistration>> GetFilteredItemsAsync(string? search);
    Task<ExamRegistration?> GetExamRegistrationByIdAsync(int id);
    Task CreateExamRegistrationAsync(ExamRegistration examRegistration);
    Task UpdateExamRegistrationAsync(ExamRegistration examRegistration);
    Task DeleteExamRegistrationAsync(int id);
    Task<bool> ExamRegistrationExistsAsync(int id);
    Task VerifyExamRegistrationAsync(int id);
    Task ApproveExamRegistrationAsync(int id);
    Task<(bool Success, string Message)> RejectExamRegistrationAsync(int id, string? reason);
    Task<ExamRegistrationSelectListsDto> GetSelectListDataAsync(ExamRegistration? examRegistration = null);
    Task<ExamFormsAdminResult> GetStudentExamFormsAsync(int? academicYearId, int? levelId, int? examScheduleId, string? search, int page, int pageSize);
    Task<ExamFormAdminDto?> GetStudentExamFormDetailAsync(int id);
    Task<ExamFormEditableSubjectsDto?> GetEditableSubjectsAsync(int examRegistrationId);
    Task<(bool Success, string Message)> UpdateRegistrationSubjectsAsync(int examRegistrationId, List<int> subjectOfferingIds, Dictionary<int, FWU.Exam.Management.Application.Helpers.ReExamLegs>? subjectLegs = null);
    Task<List<SelectOption>> GetFilterAcademicYearsAsync();
    Task<List<SelectOption>> GetFilterLevelsAsync(int academicYearId);
    Task<List<SelectOption>> GetFilterExamSchedulesAsync(int academicYearId, int levelId);
}
