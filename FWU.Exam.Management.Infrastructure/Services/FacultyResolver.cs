using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class FacultyResolver(AppDbContext context) : IFacultyResolver
{
    public async Task<CurrentFaculty?> ResolveFacultyAsync(string hostname)
    {
        var dotIndex = hostname.IndexOf('.');
        if (dotIndex < 0) return null;

        var subdomain = hostname[..dotIndex];

        var faculty = await context.Faculties
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Name.Contains(subdomain) || f.OfficeCode.Contains(subdomain));

        if (faculty == null) return null;

        return new CurrentFaculty
        {
            Id = faculty.Id,
            Name = faculty.Name,
            OfficeCode = faculty.OfficeCode,
            LogoPath = faculty.LogoPath
        };
    }

    public async Task<CurrentFaculty?> ResolveFacultyByCodeAsync(string officeCode)
    {
        var faculty = await context.Faculties
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.OfficeCode == officeCode);

        if (faculty == null) return null;

        return new CurrentFaculty
        {
            Id = faculty.Id,
            Name = faculty.Name,
            OfficeCode = faculty.OfficeCode,
            LogoPath = faculty.LogoPath
        };
    }
}
