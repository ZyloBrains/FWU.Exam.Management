using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Exams;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IExamRegistrationService
{
    Task<(List<ExamRegistration> Items, int TotalCount)> GetExamRegistrationsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? collegeId = null, int? facultyId = null, int? examScheduleId = null);
    Task<List<ExamRegistration>> GetFilteredItemsAsync(string? search, int? collegeId = null, int? facultyId = null);
    Task<ExamRegistration?> GetExamRegistrationByIdAsync(int id);
    Task CreateExamRegistrationAsync(ExamRegistration examRegistration);
    Task UpdateExamRegistrationAsync(ExamRegistration examRegistration);
    Task DeleteExamRegistrationAsync(int id);
    Task<bool> ExamRegistrationExistsAsync(int id);
    Task VerifyExamRegistrationAsync(int id);
    Task ApproveExamRegistrationAsync(int id);
    ExamRegistrationSelectListsDto GetSelectListData(ExamRegistration? examRegistration = null);
}
