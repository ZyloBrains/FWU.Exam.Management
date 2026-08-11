using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ITheoryMarksService
{
    Task<TheoryMarksPageViewModel> GetTheoryMarksPageAsync();
    Task<List<SelectOption>> GetFacultiesAsync();
    Task<List<SelectOption>> GetCollegesAsync(int? facultyId);
    Task<List<SelectOption>> GetAcademicYearsAsync(int collegeId);
    Task<List<SelectOption>> GetLevelsAsync(int collegeId, int academicYearId);
    Task<List<SelectOption>> GetExamSchedulesAsync(int collegeId, int academicYearId, int levelId);
    Task<ScheduleDetailDto> GetScheduleDetailAsync(int examScheduleId, int collegeId);
    Task<List<SubjectOptionDto>> GetSubjectsByScheduleAsync(int examScheduleId, int collegeId);
    Task<SubjectDetailDto> GetSubjectDetailAsync(int subjectOfferingId, int collegeId);
    Task<StudentTheoryMarksViewModel> GetStudentsForTheoryMarksAsync(int examScheduleId, int subjectOfferingId, int collegeId);
    Task<BulkSaveResult> SaveTheoryMarksAsync(TheoryMarksSaveDto dto);
}
