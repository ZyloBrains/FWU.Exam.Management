using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IGradingSchemeService
{
    Task<(List<GradingScheme> Items, int TotalCount)> GetGradingSchemesAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<GradingScheme>> GetFilteredItemsAsync(string? search);
    Task<GradingScheme?> GetGradingSchemeByIdAsync(int id);
    Task CreateGradingSchemeAsync(GradingScheme gradingScheme, List<int> programIds, Dictionary<int, int?> programAcademicYears);
    Task UpdateGradingSchemeAsync(GradingScheme gradingScheme, List<int> programIds, Dictionary<int, int?> programAcademicYears);
    Task DeleteGradingSchemeAsync(int id);
    Task<bool> GradingSchemeExistsAsync(int id);
    Task<GradingSchemeSelectListsDto> GetSelectListDataAsync(GradingScheme? gradingScheme = null);
}
