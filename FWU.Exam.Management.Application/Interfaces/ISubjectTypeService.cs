using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities.Subjects;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ISubjectTypeService
{
    Task<(List<SubjectType> Items, int TotalCount)> GetSubjectTypesAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<SubjectType>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<SubjectType?> GetSubjectTypeByIdAsync(int id);
    Task CreateSubjectTypeAsync(SubjectType subjectType);
    Task UpdateSubjectTypeAsync(SubjectType subjectType);
    Task DeleteSubjectTypeAsync(int id);
    Task<bool> SubjectTypeExistsAsync(int id);
}
