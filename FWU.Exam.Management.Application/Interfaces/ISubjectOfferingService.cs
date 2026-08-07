using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Subjects;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ISubjectOfferingService
{
    Task<(List<SubjectOffering> Items, int TotalProgramCount)> GetSubjectOfferingsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<SubjectOffering>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<SubjectOffering?> GetSubjectOfferingByIdAsync(int id);
    Task CreateSubjectOfferingAsync(SubjectOffering subjectOffering);
    Task CreateSubjectOfferingsAsync(List<SubjectOffering> subjectOfferings);
    Task UpdateSubjectOfferingAsync(SubjectOffering subjectOffering);
    Task DeleteSubjectOfferingAsync(int id);
    Task<bool> SubjectOfferingExistsAsync(int id);
    Task<List<int>> GetExistingSubjectCatalogIdsAsync(int programId, int semesterId, int? curriculumVersionId = null);
    Task<List<SelectOption>> GetAcademicYearsAsync();
    Task<List<SelectOption>> GetSemestersByAcademicYearAsync(int academicYearId, int? programId = null);
    Task<List<ProgramOfferingSummary>> GetProgramsByAcademicYearAsync(int academicYearId);
    Task<List<SemesterOfferingSummary>> GetSemestersByProgramAsync(int programId, int academicYearId);
    Task<List<SubjectOffering>> GetSubjectOfferingsAsync(int programId, int? semesterId = null);
    Task<(List<SubjectCatalog> SubjectCatalogs, List<Program> Programs, List<Semester> Semesters)> GetSelectListsAsync(int? subjectCatalogId = null, int? programId = null, int? semesterId = null);
    Task<bool> IsSemesterAssignedToProgramAsync(int programId, int semesterId);
    Task<List<SelectOption>> GetCurriculumVersionsAsync(int? programId = null);
    Task<List<SubjectOffering>> GetSubjectOfferingsByCurriculumVersionAsync(int curriculumVersionId);
    Task<bool> IsCurriculumVersionForProgramAsync(int curriculumVersionId, int programId);
}
