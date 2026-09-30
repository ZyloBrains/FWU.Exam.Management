using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ICollegeProgramService
{
    /// <summary>
    /// Colleges paged as whole groups. A college is included when any one of its
    /// programs matches the search term, and carries all of its programs.
    /// Returns the matching college count, the matching program count, and the
    /// total student count across the returned groups.
    /// </summary>
    Task<(List<CollegeProgramGroupDto> Colleges, int TotalCollegeCount, int TotalProgramCount)> GetPagedCollegesWithProgramsAsync(int page, int pageSize, string? search, string sort, string sortDir);

    /// <summary>
    /// Every row matching the search, ignoring pagination, so exports are not
    /// silently limited to the page currently on screen.
    /// </summary>
    Task<(List<CollegeProgram> Items, int TotalCount)> GetFilteredItemsForExportAsync(string? search, string sort, string sortDir);
    Task<CollegeProgram?> GetCollegeProgramByIdAsync(int id);
    Task CreateCollegeProgramAsync(CollegeProgram collegeProgram);
    Task CreateCollegeProgramsAsync(List<CollegeProgram> collegePrograms);
    Task<List<int>> GetExistingProgramIdsAsync(int collegeId);
    Task UpdateCollegeProgramAsync(CollegeProgram collegeProgram);
    Task DeleteCollegeProgramAsync(int id);
    Task<bool> CollegeProgramExistsAsync(int id);
    Task<(List<College> Colleges, List<Program> Programs)> GetSelectListsAsync();

    /// <summary>
    /// Colleges in the current scope that have no CollegeProgram rows yet.
    /// Queried independently of any pagination so the counts stay accurate.
    /// </summary>
    Task<List<College>> GetCollegesWithoutProgramsAsync();
}
