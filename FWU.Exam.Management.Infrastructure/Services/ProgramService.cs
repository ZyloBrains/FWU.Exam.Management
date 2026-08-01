using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class ProgramService(AppDbContext context, IUserContext userContext) : IProgramService
{
    public async Task<(List<Program> Items, int TotalCount)> GetProgramsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.Programs
            .Include(p => p.Board)
            .Include(p => p.Level)
            .AsNoTracking();
        query = query.ApplyScope(userContext);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p =>
                p.ProgramCode.Contains(search) ||
                p.ProgramName.Contains(search) ||
                p.ShortName.Contains(search) ||
                (p.Remarks != null && p.Remarks.Contains(search)) ||
                (p.Level != null && p.Level.LevelName.Contains(search)) ||
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
        var query = context.Programs
            .Include(p => p.Board)
            .Include(p => p.Level)
            .AsNoTracking();
        query = query.ApplyScope(userContext);

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p =>
                p.ProgramCode.Contains(search) ||
                p.ProgramName.Contains(search) ||
                p.ShortName.Contains(search) ||
                (p.Remarks != null && p.Remarks.Contains(search)) ||
                (p.Level != null && p.Level.LevelName.Contains(search)) ||
                (p.Board != null && p.Board.BoardName.Contains(search)));
        }

        query = sortDir.ToLower() == "desc"
            ? query.OrderByDescending(GetSortProperty(sort))
            : query.OrderBy(GetSortProperty(sort));

        return await query.ToListAsync();
    }

    public async Task<Program?> GetProgramByIdAsync(int id)
    {
        return await context.Programs
            .Include(p => p.Board)
            .Include(p => p.Level)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task CreateProgramAsync(Program program)
    {
        context.Programs.Add(program);
        await context.SaveChangesAsync();
    }

    public async Task UpdateProgramAsync(Program program)
    {
        context.Programs.Update(program);
        await context.SaveChangesAsync();
    }

    public async Task DeleteProgramAsync(int id)
    {
        var program = await context.Programs.FindAsync(id);
        if (program != null)
        {
            context.Programs.Remove(program);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> ProgramExistsAsync(int id)
    {
        return await context.Programs.AnyAsync(e => e.Id == id);
    }

    public async Task<(List<Board> Boards, List<Level> Levels)> GetSelectListsAsync(int? boardId = null, int? levelId = null)
    {
        var boards = await context.Boards.AsNoTracking().ToListAsync();
        var levels = await context.Levels.AsNoTracking().ToListAsync();

        return (boards, levels);
    }

    private static Expression<Func<Program, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "programcode" => p => p.ProgramCode,
            "programname" => p => p.ProgramName,
            "shortname" => p => p.ShortName,
            "level" => p => p.Level.LevelName,
            "board" => p => p.Board.BoardName,
            "duration" => p => p.Duration,
            "grandtotalmarks" => p => p.GrandTotalMarks,
            "numberofseats" => p => p.NumberOfSeats,
            "isactive" => p => p.IsActive,
            _ => p => p.ProgramCode
        };
    }
}
