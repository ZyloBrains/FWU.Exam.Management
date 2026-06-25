using System.Collections.Generic;
using System.Threading.Tasks;
using FWU.Exam.Management.Domain.Entities;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IResultRecordService
{
    Task<(List<ResultRecord> Items, int TotalCount)> GetResultRecordsAsync(int page, int pageSize, string? search, string sort, string sortDir, int? collegeId = null, int? facultyId = null);
    Task<List<ResultRecord>> GetFilteredItemsAsync(string? search, int? collegeId = null, int? facultyId = null);
    Task<ResultRecord?> GetResultRecordByIdAsync(int id);
}
