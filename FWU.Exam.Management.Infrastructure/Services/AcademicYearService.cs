using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class AcademicYearService : IAcademicYearService
{
    private readonly AppDbContext _context;

    public AcademicYearService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AcademicYear>> GetAllAcademicYearsAsync()
    {
        return await _context.AcademicYears
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<AcademicYear?> GetAcademicYearByIdAsync(int id)
    {
        return await _context.AcademicYears
            .AsNoTracking()
            .FirstOrDefaultAsync(ay => ay.Id == id);
    }

    public async Task CreateAcademicYearAsync(AcademicYear academicYear)
    {
        _context.AcademicYears.Add(academicYear);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAcademicYearAsync(AcademicYear academicYear)
    {
        _context.AcademicYears.Update(academicYear);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAcademicYearAsync(int id)
    {
        var academicYear = await _context.AcademicYears.FindAsync(id);
        if (academicYear != null)
        {
            _context.AcademicYears.Remove(academicYear);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> AcademicYearExistsAsync(int id)
    {
        return await _context.AcademicYears.AnyAsync(ay => ay.Id == id);
    }
}
