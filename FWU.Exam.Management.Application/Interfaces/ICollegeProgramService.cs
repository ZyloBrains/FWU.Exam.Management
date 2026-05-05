using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ICollegeProgramService
{
    Task<(List<CollegeProgram> Items, int TotalCount)> GetCollegeProgramsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<(List<CollegeProgram> Items, int TotalCount)> GetFilteredItemsForExportAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<CollegeProgram?> GetCollegeProgramByIdAsync(int id);
    Task CreateCollegeProgramAsync(CollegeProgram collegeProgram);
    Task UpdateCollegeProgramAsync(CollegeProgram collegeProgram);
    Task DeleteCollegeProgramAsync(int id);
    Task<bool> CollegeProgramExistsAsync(int id);
    Task<(List<College> Colleges, List<Program> Programs)> GetSelectListsAsync();
}
