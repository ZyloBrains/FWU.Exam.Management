using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IAcademicYearService
{
    Task<List<AcademicYear>> GetAllAcademicYearsAsync();
    Task<AcademicYear?> GetAcademicYearByIdAsync(int id);
    Task CreateAcademicYearAsync(AcademicYear academicYear);
    Task UpdateAcademicYearAsync(AcademicYear academicYear);
    Task DeleteAcademicYearAsync(int id);
    Task<bool> AcademicYearExistsAsync(int id);
}
