using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class BoardService(AppDbContext context) : IBoardService
{
    public async Task<(List<Board> Items, int TotalCount)> GetBoardsAsync(int page, int pageSize, string? search, string sort, string sortDir)
    {
        var query = context.Boards.AsNoTracking();

        // Apply search filter
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(b =>
                b.BoardName.Contains(search) ||
                (b.Remarks != null && b.Remarks.Contains(search)));
        }

        // Apply sorting
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

    public async Task<Board?> GetBoardByIdAsync(int id)
    {
        return await context.Boards
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task CreateBoardAsync(Board board)
    {
        context.Boards.Add(board);
        await context.SaveChangesAsync();
    }

    public async Task UpdateBoardAsync(Board board)
    {
        context.Boards.Update(board);
        await context.SaveChangesAsync();
    }

    public async Task DeleteBoardAsync(int id)
    {
        var board = await context.Boards.FindAsync(id);
        if (board != null)
        {
            context.Boards.Remove(board);
            await context.SaveChangesAsync();
        }
    }

    public async Task<bool> BoardExistsAsync(int id)
    {
        return await context.Boards.AnyAsync(e => e.Id == id);
    }

    private static System.Linq.Expressions.Expression<Func<Board, object>> GetSortProperty(string sort)
    {
        return sort.ToLower() switch
        {
            "boardname" => b => b.BoardName,
            "remarks" => b => b.Remarks ?? "",
            "isactive" => b => b.IsActive,
            _ => b => b.BoardName
        };
    }
}
