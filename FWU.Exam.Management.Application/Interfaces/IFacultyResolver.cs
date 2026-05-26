using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IFacultyResolver
{
    Task<CurrentFaculty?> ResolveFacultyAsync(string hostname);
    Task<CurrentFaculty?> ResolveFacultyByCodeAsync(string officeCode);
}
