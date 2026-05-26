using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class FacultyService(AppDbContext context) : IFacultyService
{
    public async Task<List<Faculty>> GetAllFacultiesAsync()
    {
        return await context.Faculties
            .AsNoTracking()
            .OrderBy(f => f.Name)
            .ToListAsync();
    }

    public async Task<Faculty?> GetFacultyByIdAsync(int id)
    {
        return await context.Faculties.FindAsync(id);
    }

    public async Task<Faculty?> GetFacultyByOfficeCodeAsync(string officeCode)
    {
        return await context.Faculties
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.OfficeCode == officeCode);
    }

    public async Task CreateFacultyAsync(Faculty faculty)
    {
        context.Faculties.Add(faculty);
        await context.SaveChangesAsync();
    }

    public async Task UpdateFacultyAsync(Faculty faculty)
    {
        context.Faculties.Update(faculty);
        await context.SaveChangesAsync();
    }

    public async Task DeleteFacultyAsync(int id)
    {
        var faculty = await context.Faculties.FindAsync(id);
        if (faculty != null)
        {
            context.Faculties.Remove(faculty);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> FacultyExistsAsync(int id)
    {
        return await context.Faculties.AnyAsync(f => f.Id == id);
    }
}
