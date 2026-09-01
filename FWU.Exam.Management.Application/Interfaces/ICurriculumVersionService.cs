using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Subjects;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ICurriculumVersionService
{
    Task<(List<CurriculumVersion> Items, int TotalCount)> GetCurriculumVersionsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<CurriculumVersion>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<CurriculumVersion?> GetCurriculumVersionByIdAsync(int id);
    Task CreateCurriculumVersionAsync(CurriculumVersion curriculumVersion);
    Task UpdateCurriculumVersionAsync(CurriculumVersion curriculumVersion);
    Task<(bool Deleted, int SkippedOfferings)> DeleteCurriculumVersionAsync(int id);
    Task<bool> CurriculumVersionExistsAsync(int id);
    Task<(List<Program> Programs, List<AcademicYear> AcademicYears)> GetSelectListsAsync(int? programId = null, int? academicYearId = null);
    Task<List<CurriculumVersion>> GetCurriculumVersionsByProgramAsync(int programId);
    Task<CurriculumVersion?> CopyCurriculumVersionAsync(int sourceVersionId, int targetAcademicYearId, string name);
}
