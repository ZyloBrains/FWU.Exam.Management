using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IBoardService
{
    Task<(List<Board> Items, int TotalCount)> GetBoardsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<Board?> GetBoardByIdAsync(int id);
    Task CreateBoardAsync(Board board);
    Task UpdateBoardAsync(Board board);
    Task DeleteBoardAsync(int id);
    Task<bool> BoardExistsAsync(int id);
}
