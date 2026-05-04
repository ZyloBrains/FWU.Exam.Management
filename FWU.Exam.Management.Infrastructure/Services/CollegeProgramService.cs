using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class CollegeProgramService : ICollegeProgramService
{
    private readonly AppDbContext _context;

    public CollegeProgramService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<CollegeProgram> Items, int TotalCount)> GetCollegeProgramsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = BuildQuery(search);

        var totalCount = await query.CountAsync();

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(List<CollegeProgram> Items, int TotalCount)> GetFilteredItemsForExportAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        return await GetCollegeProgramsAsync(page, pageSize, search, sort, sortDir);
    }

    public async Task<CollegeProgram?> GetCollegeProgramByIdAsync(int id)
    {
        return await _context.CollegePrograms
            .Include(cp => cp.College)
            .Include(cp => cp.Program)
            .AsNoTracking()
            .FirstOrDefaultAsync(cp => cp.Id == id);
    }

    public async Task CreateCollegeProgramAsync(CollegeProgram collegeProgram)
    {
        _context.CollegePrograms.Add(collegeProgram);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCollegeProgramAsync(CollegeProgram collegeProgram)
    {
        _context.CollegePrograms.Update(collegeProgram);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCollegeProgramAsync(int id)
    {
        var collegeProgram = await _context.CollegePrograms.FindAsync(id);
        if (collegeProgram != null)
        {
            _context.CollegePrograms.Remove(collegeProgram);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> CollegeProgramExistsAsync(int id)
    {
        return await _context.CollegePrograms.AnyAsync(cp => cp.Id == id);
    }

    public async Task<(List<College> Colleges, List<Program> Programs)> GetSelectListsAsync()
    {
        var colleges = await _context.Colleges.AsNoTracking().ToListAsync();
        var programs = await _context.Programs.AsNoTracking().ToListAsync();

        return (colleges, programs);
    }

    private IQueryable<CollegeProgram> BuildQuery(string? search)
    {
        var query = _context.CollegePrograms
            .Include(cp => cp.College)
            .Include(cp => cp.Program)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(cp =>
                cp.College.Code.ToString().Contains(search) ||
                cp.College.Name.Contains(search) ||
                cp.Program.ProgramCode.Contains(search) ||
                cp.Program.ProgramName.Contains(search) ||
                cp.Remarks.Contains(search) ||
                cp.NumberOfStudents.ToString().Contains(search));
        }

        return query;
    }

    private static Expression<Func<CollegeProgram, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "collegecode" => cp => cp.College.Code,
            "collegename" => cp => cp.College.Name,
            "programcode" => cp => cp.Program.ProgramCode,
            "programname" => cp => cp.Program.ProgramName,
            "affiliationdate" => cp => cp.AffiliationDate,
            "numberofstudents" => cp => cp.NumberOfStudents,
            "isactive" => cp => cp.IsActive,
            "remarks" => cp => cp.Remarks ?? "",
            _ => cp => cp.Id
        };
    }
}
