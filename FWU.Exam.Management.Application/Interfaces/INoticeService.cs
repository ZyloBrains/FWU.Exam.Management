using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface INoticeService
{
    Task<(List<Notice> Items, int TotalCount)> GetNoticesAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<Notice>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<Notice?> GetNoticeByIdAsync(int id);
    Task CreateNoticeAsync(Notice notice);
    Task UpdateNoticeAsync(Notice notice);
    Task DeleteNoticeAsync(int id);
    Task<bool> NoticeExistsAsync(int id);
    Task<List<Notice>> GetLatestNoticesAsync(int count);
}
