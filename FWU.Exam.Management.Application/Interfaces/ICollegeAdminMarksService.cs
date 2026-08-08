using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ICollegeAdminMarksService
{
    Task<CollegeAdminDashboardDto> GetCollegeAdminDashboardAsync(string collegeAdminUserId);
    Task<MarksEntryViewModel> GetMarksEntryViewAsync(int subjectOfferingId, int examScheduleId, string collegeAdminUserId);
    Task<BulkSaveResult> SaveMarksBulkAsync(BulkMarksSaveDto dto, string collegeAdminUserId);
    Task<BulkSaveResult> SaveCollegeMarksBulkAsync(BulkMarksSaveDto dto, int collegeId, string collegeAdminUserId);
    Task<ExcelImportResultDto> ImportMarksFromExcelAsync(Stream excelStream, int subjectOfferingId, int examScheduleId, string collegeAdminUserId);
    Task<byte[]> ExportMarksTemplateAsync(int subjectOfferingId, int examScheduleId);
    Task<byte[]> ExportMarksAsync(int subjectOfferingId, int examScheduleId);
}

public class BulkSaveResult
{
    public bool Success { get; set; }
    public int SavedCount { get; set; }
    public List<string> Errors { get; set; } = [];
}
