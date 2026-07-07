using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Exams;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IRetotalRequestService
{
    Task<(List<RetotalRequest> Items, int TotalCount)> GetRetotalRequestsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<RetotalRequest>> GetFilteredItemsAsync(string? search);
    Task<RetotalRequest?> GetRetotalRequestByIdAsync(int id);
    Task CreateRetotalRequestAsync(RetotalRequest retotalRequest);
    Task UpdateRetotalRequestAsync(RetotalRequest retotalRequest);
    Task DeleteRetotalRequestAsync(int id);
    Task<bool> RetotalRequestExistsAsync(int id);
    Task ApproveRetotalRequestAsync(int id, string? retotalledGradeLetter, decimal? retotalledMarks, string? adminRemarks, string reviewedBy);
    Task RejectRetotalRequestAsync(int id, string? adminRemarks, string reviewedBy);
    Task MarkUnderReviewAsync(int id, string reviewedBy);
    RetotalRequestSelectListsDto GetSelectListData(RetotalRequest? retotalRequest = null);
}
