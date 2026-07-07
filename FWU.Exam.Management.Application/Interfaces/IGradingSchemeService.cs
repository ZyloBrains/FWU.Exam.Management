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
    Task CreateGradingSchemeAsync(GradingScheme gradingScheme);
    Task UpdateGradingSchemeAsync(GradingScheme gradingScheme);
    Task DeleteGradingSchemeAsync(int id);
    Task<bool> GradingSchemeExistsAsync(int id);
    GradingSchemeSelectListsDto GetSelectListData(GradingScheme? gradingScheme = null);
}
