using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ISubjectOfferingService
{
    Task<(List<SubjectOffering> Items, int TotalCount)> GetSubjectOfferingsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<SubjectOffering>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<SubjectOffering?> GetSubjectOfferingByIdAsync(int id);
    Task CreateSubjectOfferingAsync(SubjectOffering subjectOffering);
    Task CreateSubjectOfferingsAsync(List<SubjectOffering> subjectOfferings);
    Task UpdateSubjectOfferingAsync(SubjectOffering subjectOffering);
    Task DeleteSubjectOfferingAsync(int id);
    Task<bool> SubjectOfferingExistsAsync(int id);
    Task<List<int>> GetExistingSubjectCatalogIdsAsync(int programId);
    Task<(List<SubjectCatalog> SubjectCatalogs, List<Program> Programs, List<Semester> Semesters)> GetSelectListsAsync(int? subjectCatalogId = null, int? programId = null, int? semesterId = null);
}
