using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ProgramService : IProgramService
{
    private readonly AppDbContext _context;

    public ProgramService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Program> Items, int TotalCount)> GetProgramsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.Programs
            .Include(p => p.Board)
            .Include(p => p.Faculty)
            .Include(p => p.Level)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p =>
                p.ProgramCode.Contains(search) ||
                p.ProgramName.Contains(search) ||
                p.ShortName.Contains(search) ||
                (p.Remarks != null && p.Remarks.Contains(search)) ||
                (p.Level != null && p.Level.LevelName.Contains(search)) ||
                (p.Faculty != null && p.Faculty.FacultyCode.Contains(search)) ||
                (p.Board != null && p.Board.BoardName.Contains(search)));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Program>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = _context.Programs
            .Include(p => p.Board)
            .Include(p => p.Faculty)
            .Include(p => p.Level)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p =>
                p.ProgramCode.Contains(search) ||
                p.ProgramName.Contains(search) ||
                p.ShortName.Contains(search) ||
                (p.Remarks != null && p.Remarks.Contains(search)) ||
                (p.Level != null && p.Level.LevelName.Contains(search)) ||
                (p.Faculty != null && p.Faculty.FacultyCode.Contains(search)) ||
                (p.Board != null && p.Board.BoardName.Contains(search)));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<Program?> GetProgramByIdAsync(int id)
    {
        return await _context.Programs
            .Include(p => p.Board)
            .Include(p => p.Faculty)
            .Include(p => p.Level)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateProgramAsync(Program program)
    {
        _context.Programs.Add(program);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateProgramAsync(Program program)
    {
        _context.Programs.Update(program);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProgramAsync(int id)
    {
        var program = await _context.Programs.FindAsync(id);
        if (program != null)
        {
            _context.Programs.Remove(program);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ProgramExistsAsync(int id)
    {
        return await _context.Programs.AnyAsync(e => e.Id == id);
    }

    public async Task<(List<Board> Boards, List<Faculty> Faculties, List<Level> Levels)> GetSelectListsAsync(int? boardId = null, int? facultyId = null, int? levelId = null)
    {
        var boards = await _context.Boards.AsNoTracking().ToListAsync();
        var faculties = await _context.Faculties.AsNoTracking().ToListAsync();
        var levels = await _context.Levels.AsNoTracking().ToListAsync();

        return (boards, faculties, levels);
    }

    private static Expression<Func<Program, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "programcode" => p => p.ProgramCode,
            "programname" => p => p.ProgramName,
            "shortname" => p => p.ShortName,
            "level" => p => p.Level.LevelName,
            "faculty" => p => p.Faculty.FacultyCode,
            "board" => p => p.Board.BoardName,
            "duration" => p => p.Duration,
            "grandtotalmarks" => p => p.GrandTotalMarks,
            "numberofseats" => p => p.NumberOfSeats,
            "isactive" => p => p.IsActive,
            _ => p => p.ProgramCode
        };
    }
}
