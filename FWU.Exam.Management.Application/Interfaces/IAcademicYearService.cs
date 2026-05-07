using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IAcademicYearService
{
    //(List<Board> Items, int TotalCount)
    Task<(List<AcademicYear> Items, int TotalCount)> GetAllAcademicYearsAsync(int page, int pageSize, string? search);
    //Task<(List<Board> Items, int TotalCount)> GetBoardsAsync( string sort, string sortDir);
    Task<AcademicYear?> GetAcademicYearByIdAsync(int id);
    Task CreateAcademicYearAsync(AcademicYear academicYear);
    Task UpdateAcademicYearAsync(AcademicYear academicYear);
    Task DeleteAcademicYearAsync(int id);
    Task<bool> AcademicYearExistsAsync(int id);
}
