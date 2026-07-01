using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ITeacherMarksService
{
    Task<TeacherDashboardDto> GetTeacherDashboardAsync(string teacherUserId);
    Task<MarksEntryViewModel> GetMarksEntryViewAsync(int subjectOfferingId, int examScheduleId, string teacherUserId);
    Task<BulkSaveResult> SaveMarksBulkAsync(BulkMarksSaveDto dto, string teacherUserId);
    Task<ExcelImportResultDto> ImportMarksFromExcelAsync(Stream excelStream, int subjectOfferingId, int examScheduleId, string teacherUserId);
    Task<byte[]> ExportMarksTemplateAsync(int subjectOfferingId, int examScheduleId);
    Task<byte[]> ExportMarksAsync(int subjectOfferingId, int examScheduleId);
}

public class BulkSaveResult
{
    public bool Success { get; set; }
    public int SavedCount { get; set; }
    public List<string> Errors { get; set; } = [];
}
