using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ICollegeAdminMarksService
{
    Task<InternalMarksPageViewModel> GetInternalMarksPageAsync();
    Task<List<SelectOption>> GetFacultiesAsync();
    Task<List<SelectOption>> GetCollegesAsync(int? facultyId);
    Task<List<SelectOption>> GetAcademicYearsAsync(int collegeId);
    Task<List<SelectOption>> GetLevelsAsync(int collegeId, int academicYearId);
    Task<List<SelectOption>> GetExamSchedulesAsync(int collegeId, int academicYearId, int levelId);
    Task<ScheduleDetailDto> GetScheduleDetailAsync(int examScheduleId, int collegeId);
    Task<List<SubjectOptionDto>> GetSubjectsByScheduleAsync(int examScheduleId, int collegeId);
    Task<SubjectDetailDto> GetSubjectDetailAsync(int subjectOfferingId, int collegeId);
    Task<StudentInternalMarksViewModel> GetStudentsForInternalMarksAsync(int examScheduleId, int subjectOfferingId, int collegeId);
    Task<BulkSaveResult> SaveInternalMarksAsync(InternalMarksSaveDto dto);
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
