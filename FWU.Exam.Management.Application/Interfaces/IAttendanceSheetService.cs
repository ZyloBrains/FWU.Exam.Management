using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IAttendanceSheetService
{
    Task<AttendanceSheetReportDto?> BuildAsync(
        int? examScheduleId,
        int? collegeId,
        int? programId = null);
}