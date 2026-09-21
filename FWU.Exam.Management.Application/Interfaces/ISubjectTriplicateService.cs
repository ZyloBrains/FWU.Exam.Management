using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Application.Interfaces;

public interface ISubjectTriplicateService
{
    Task<SubjectTriplicateReportDto?> BuildAsync(
        int? examScheduleId,
        int? collegeId,
        int? programId = null,
        int? semesterId = null,
        int? examTypeId = null);
}
