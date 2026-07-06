using FWU.Exam.Management.Domain.Entities.Payments;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IBillTitleService
{
    Task<(List<BillTitle> Items, int TotalCount)> GetBillTitlesAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<List<BillTitle>> GetFilteredItemsAsync(int page, int pageSize, string? search, string sort, string sortDir);
    Task<BillTitle?> GetBillTitleByIdAsync(int id);
    Task CreateBillTitleAsync(BillTitle billTitle);
    Task UpdateBillTitleAsync(BillTitle billTitle);
    Task DeleteBillTitleAsync(int id);
    Task<bool> BillTitleExistsAsync(int id);
    Task<List<Domain.Entities.Exams.ExamSchedule>> GetExamSchedulesAsync(int? collegeId = null, int? facultyId = null);
    Task<List<Domain.Entities.Program>> GetProgramsAsync();
}
