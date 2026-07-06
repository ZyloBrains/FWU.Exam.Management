using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities.Subjects;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ISubjectCatalogService
{
    Task<(List<SubjectCatalog> Items, int TotalCount)> GetSubjectCatalogsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? facultyId = null);
    Task<List<SubjectCatalog>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? facultyId = null);
    Task<SubjectCatalog?> GetSubjectCatalogByIdAsync(int id);
    Task CreateSubjectCatalogAsync(SubjectCatalog subjectCatalog);
    Task UpdateSubjectCatalogAsync(SubjectCatalog subjectCatalog);
    Task DeleteSubjectCatalogAsync(int id);
    Task<bool> SubjectCatalogExistsAsync(int id);
    Task<List<SubjectType>> GetSelectListsAsync(int? subjectTypeId = null);
    Task BulkCreateAsync(List<SubjectCatalog> items);
    Task<List<string?>> GetExistingSubjectCodesAsync();
}
